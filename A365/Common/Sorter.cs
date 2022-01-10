using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace A365.Common
{
    public class Sorter
    {
        public class Request
        {
            public string FilePath { set; get; }

            public string DirectoryPath { set; get; }

            public bool UseSecondDrive { set; get; }

            public int RamSizeValue { set; get; }

            public int ThreadCountValue { set; get; }

            public int ThreadCount2Value { set; get; }

            public int CoreCountValue { set; get; }

            public CancellationToken Token { set; get; }
        }
        public static string Dict = "abcdefghijklmnopqrstuvwxyz";//А вдруг не все символы есть, можно не 
        private string _filePrefix = "sorter";
        private static Dictionary<string, Buffer> _sorted;
        private class Buffer
        {
            public bool IsBusy { set; get; }
            public List<string> Items { set; get; }
        }

        public async Task Sort(Request request)
        {
            _sorted = new Dictionary<string, Buffer>();
            foreach (var item in Dict)
            {
                _sorted.Add(item.ToString() + "0", new Buffer() { IsBusy = false, Items = new List<string>() });
            }

            var filePath = request.FilePath;
            var firstPath = System.IO.Path.GetDirectoryName(filePath);
            var secondPath = request.UseSecondDrive ? request.DirectoryPath : firstPath;

            foreach (var file in Directory.GetFiles(firstPath, _filePrefix + "*.txt", SearchOption.TopDirectoryOnly))
                File.Delete(file);

            if (request.UseSecondDrive)
                foreach (var file in Directory.GetFiles(secondPath, _filePrefix + "*.txt", SearchOption.TopDirectoryOnly))
                    File.Delete(file);

            var indexes = new int[Dict.Length];
            var size = new int[Dict.Length];
            long _maxFileSizeIbByte = ((long)request.RamSizeValue * 1024 * 1024 * 1024) / (Dict.Length * 5);//5 эмпирически выведена

            var splitCount = Math.Min(request.ThreadCountValue, request.CoreCountValue);
            var mergeCount = splitCount;


            if (request.UseSecondDrive)
                mergeCount = Math.Min(mergeCount, request.ThreadCount2Value);

            var tasks = new List<Task>();

            using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))
            {
                string line;
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    var id = char.ToLower(line.Split(". ")[1][0]);
                    var ind = Dict.IndexOf(id);
                    if (ind > -1)
                    {
                        size[ind] += Encoding.UTF8.GetByteCount(line) + 2;//2 байта перевод строки
                        var key = id + indexes[ind].ToString();
                        _sorted[key].Items.Add(line);

                        if (request.Token.IsCancellationRequested)
                            break;

                        if (size[ind] > _maxFileSizeIbByte)
                        {
                            indexes[ind]++;
                            size[ind] = 0;
                            _sorted.Add(id + indexes[ind].ToString(), new Buffer() { IsBusy = false, Items = new List<string>() });
                            _sorted[key].IsBusy = true;

                            var bufferTasks = tasks.Where(t => t.Status == TaskStatus.Running);
                            if (bufferTasks.Count() > splitCount - 1)
                                await Task.WhenAny(bufferTasks);

                            tasks.Add(Task.Run(() => Saver(key, secondPath, _filePrefix), request.Token));

                            var toDelete = tasks.Where(t => t.Status == TaskStatus.RanToCompletion).ToList();
                            foreach (var taskToDelete in toDelete)
                            {
                                tasks.Remove(taskToDelete);
                            }

                            foreach (var toDel in _sorted.Where(p => p.Value.Items == null).ToList())
                            {
                                _sorted.Remove(toDel.Key);
                            }
                        }
                    }
                }
            }

            if (request.Token.IsCancellationRequested)
            {
                await Task.WhenAll(tasks.Where(t => t.Status != TaskStatus.Canceled));

                foreach (var file in Directory.GetFiles(secondPath, _filePrefix + "*.txt", SearchOption.TopDirectoryOnly))
                    File.Delete(file);

                return;
            }

            foreach (var key in _sorted.Where(p => !p.Value.IsBusy))
            {
                tasks.Add(Task.Run(() => Saver(key.Key, secondPath, _filePrefix), request.Token));
            }

            await Task.WhenAll(tasks.Where(t => t.Status != TaskStatus.Canceled));

            if (request.Token.IsCancellationRequested)
            {
                foreach (var file in Directory.GetFiles(secondPath, _filePrefix + "*.txt", SearchOption.TopDirectoryOnly))
                    File.Delete(file);

                return;
            }

            var tasks2 = new List<Task>();
            foreach (var chunk in Dict.ToList().Chunk(mergeCount))
            {
                //оставить Ienumerable
                tasks2.Add(Task.Run(() =>
                {
                    foreach (var postFix in chunk)
                    {
                        Merge(postFix, firstPath, secondPath, _filePrefix, request.Token);
                    }
                }, request.Token));
            }

            await Task.WhenAll(tasks2);

            if (request.Token.IsCancellationRequested)
            {
                foreach (var file in Directory.GetFiles(secondPath, _filePrefix + "*.txt", SearchOption.TopDirectoryOnly))
                    File.Delete(file);

                return;
            }

            using (var destStream = File.Create(@$"{firstPath}\output_result.txt"))
            {
                foreach (var postFix in Dict)
                {
                    using (var srcStream = File.OpenRead($@"{firstPath}\{_filePrefix}{postFix}_result.txt")) srcStream.CopyTo(destStream);
                    File.Delete($@"{firstPath}\{_filePrefix}{postFix}_result.txt");
                }
            }
        }

        private void Saver(string key, string path, string prefix)
        {
            var fileName = $@"{path}/{prefix}{key}.txt";
            File.WriteAllLines(fileName, _sorted[key].Items.OrderBy(p => p, new ItemComparer()));
            _sorted[key].Items = null;
        }

        private void Merge(char postFix, string firstPath, string secondPath, string prefix, CancellationToken token)
        {
            var fileNames = Directory.GetFiles(secondPath, prefix + postFix + "*.txt", SearchOption.TopDirectoryOnly);
            if (fileNames.Length > 1)//тут можкт быть переполнение, надо как то проверять
            {
                var readers = new List<LineStreamReader>(fileNames.Length);
                for (int i = 0; i < fileNames.Length; i++)
                {
                    var reader = new LineStreamReader(fileNames[i]);
                    reader.Init();
                    readers.Add(reader);
                }

                var comparer = new ItemComparer();
                using (StreamWriter file = new StreamWriter($@"{firstPath}\{prefix}{postFix}_result.txt", true))
                {
                    while (readers.Any(r => r.IsActive))//todo может после отработки сразу кикать
                    {
                        var line = readers.Where(r => r.IsActive).OrderBy(r => r.Value, comparer).First().PopAsync();
                        file.WriteLine(line);

                        if (token.IsCancellationRequested)
                            break;
                    }
                }
            }
            else if (fileNames.Length == 1)
            {
                File.Move(fileNames[0], $@"{firstPath}\{prefix}{postFix}_result.txt");
            }

            foreach (var fileName in fileNames)
            {
                File.Delete(fileName);
            }
        }

    }
}

