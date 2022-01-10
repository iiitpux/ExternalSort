using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A365
{
    class SimpleSort
    {
        #region simple
        //public partial class MainWindow : Window
        //{
        //    public MainWindow()
        //    {
        //        InitializeComponent();
        //        //создавать папку для каждого раза
        //        //вынести все по правилам единой ответственности
        //    }

        //    //1588 секунд при 2 мб файл
        //    //1415 секунд при 10 мб файл
        //    //834 секунд при 100 мб файл


        //    private Dictionary<char, int> _filesSize;//Может внутри метода объявить

        //    //todo- а может вариант когда пробегаемся по файлу и собираем все с буквой а в начале и сортируем сразу в памяти
        //    //но тогда файл надо 27 раз прогнать
        //    //проверять размер файла, если меньше 2 гигов, то множно его в оперативку засунуть или сразу сортировать группами и сливать потом
        //    private async void Button_Click(object sender, RoutedEventArgs e)
        //    {
        //        Stopwatch stopwatch = new Stopwatch();
        //        stopwatch.Start();

        //        var dict = "abcdefghijklmnopqrstuvwxyz";
        //        _filesSize = dict.Select(p => p).ToDictionary(p => p, p => 0);

        //        using (var pool = new FilePool())
        //        {
        //            using (StreamReader sr = new StreamReader(@"C:\1\Nabor1.txt", System.Text.Encoding.Default))
        //            {
        //                string line;
        //                while ((line = await sr.ReadLineAsync()) != null)
        //                {
        //                    //проверить строку на корректность что там есть точка и парситься вообще строка
        //                    pool.SaveInto(line);
        //                }
        //            }
        //        }

        //        int y = 0;
        //        // на каждую букву создаем файл и туда все пихаем - если переполнение, то след файл
        //        var list = new List<string>();

        //        foreach (var postFix in dict)
        //        {
        //            foreach (var fileName in Directory.GetFiles(@"C:\1\", $"Nabor{postFix}*.txt"))
        //            {
        //                using (StreamReader sr = new StreamReader(fileName, System.Text.Encoding.Default))
        //                {
        //                    string line;//тут надо все линии сразу
        //                    while ((line = await sr.ReadLineAsync()) != null)
        //                    {
        //                        list.Add(line);
        //                    }
        //                }

        //                File.Delete(fileName);

        //                using (StreamWriter file = new StreamWriter($@"{fileName.Split('.')[0]}_sorted.txt", true))
        //                {
        //                    Random rnd = new Random();
        //                    foreach (var item in list.OrderBy(p => p))
        //                    {
        //                        await file.WriteLineAsync(item);
        //                    }
        //                }
        //                list.Clear();
        //            }
        //        }
        //        GC.Collect();
        //        y = 1;
        //        //todo открывать файлы только на чтение или запись - проверить это как то влияет на скорость
        //        foreach (var postFix in dict)
        //        {
        //            var fileNames = Directory.GetFiles(@"C:\1\", $"Nabor{postFix}*_sorted.txt");
        //            if (fileNames.Length > 1)//тут можкт быть переполнение, надо как то проверять
        //            {
        //                var readers = new List<LineStreamReader>(fileNames.Length);
        //                for (int i = 0; i < fileNames.Length; i++)
        //                {
        //                    var reader = new LineStreamReader(fileNames[i]);
        //                    await reader.Init();
        //                    readers.Add(reader);
        //                }

        //                using (StreamWriter file = new StreamWriter($@"C:\1\Nabor{postFix}_result.txt", true))
        //                {
        //                    while (readers.Any(r => r.IsActive))//todo может после отработки сразу кикать
        //                    {
        //                        var line = await readers.Where(r => r.IsActive).OrderBy(r => r.Value).First().PopAsync();
        //                        await file.WriteLineAsync(line);
        //                    }
        //                }
        //            }
        //            else if (fileNames.Length == 1)
        //            {
        //                //todo- этот уже отсортирован и просто переименовываем
        //                File.Move(fileNames[0], $@"C:\1\Nabor{postFix}_result.txt");
        //            }
        //            //файлы удалить
        //            foreach (var fileName in fileNames)
        //            {
        //                File.Delete(fileName);
        //            }
        //        }

        //        using (var destStream = File.Create(@"C:\1\fileOutput.txt"))
        //        {
        //            foreach (var postFix in dict)
        //            {
        //                using (var srcStream = File.OpenRead($@"C:\1\Nabor{postFix}_result.txt")) srcStream.CopyTo(destStream);
        //                File.Delete($@"C:\1\Nabor{postFix}_result.txt");
        //            }
        //        }

        //        y = 2;
        //        stopwatch.Stop();
        //        var t = stopwatch.ElapsedMilliseconds / 1000;
        //        int yy = 0;
        //        //todo- просто проверку файла итогового на корректность, то есть след строка должн быть больше текущей
        //    }
        //}

        //class LineStreamReader
        //{
        //    private string _fileName;
        //    public LineStreamReader(string fileName)
        //    {
        //        if (string.IsNullOrWhiteSpace(fileName))
        //            throw new ArgumentNullException(nameof(fileName));

        //        _fileName = fileName;
        //    }

        //    public async Task Init()
        //    {
        //        StreamReader = new StreamReader(_fileName);
        //        await SetValueAsync();
        //    }

        //    public bool IsActive { private set; get; } = true;

        //    public string Value { get; set; }

        //    public StreamReader StreamReader { get; set; } //приват наверно

        //    private async Task SetValueAsync()
        //    {
        //        Value = await StreamReader.ReadLineAsync();
        //        if (Value == null)
        //        {
        //            IsActive = false;
        //            StreamReader.Dispose();
        //            //dispose streamReader
        //        }
        //    }

        //    public async Task<string> PopAsync()
        //    {
        //        var val = Value;

        //        await SetValueAsync();

        //        return val;
        //    }
        //}

        //class Item
        //{
        //    public string Value { get; set; }

        //    public int StreamReaderIndex { get; set; }
        //}

        //class FilePool : IDisposable
        //{
        //    private Dictionary<char, Writer> _writers =
        //        new Dictionary<char, Writer>();

        //    private long maxFileSizeIbByte = 100 * 1024 * 1024;//(делаем 1 гиг на всякий) может и int хватит
        //                                                       //todo- а если думать про будущий парсинг то int32.Max максимальное значение буфера

        //    private int maxLineCount = Int32.MaxValue; //что бы потом влезло в List

        //    public void SaveInto(string line)//todo- async
        //    {
        //        if (string.IsNullOrEmpty(line))
        //            return;

        //        var writer = Get(line[0]);
        //        writer.FileSize += Encoding.UTF8.GetByteCount(line) + 2;//2 байта перевод строки
        //        writer.LineCount++;
        //        writer.StreamWriter.WriteLine(line);

        //        if (writer.FileSize > maxFileSizeIbByte || writer.LineCount > maxLineCount)
        //        {
        //            writer.StreamWriter.Dispose();
        //            writer.FilePostfix++;
        //            writer.FileSize = 0;
        //            writer.LineCount = 0;
        //            writer.StreamWriter = new StreamWriter($@"C:\1\Nabor{line[0]}{writer.FilePostfix}.txt");//todo- покарсивше сделать создание, чтоб в одном месте было
        //        }
        //    }

        //    private Writer Get(char id)
        //    {
        //        if (!_writers.ContainsKey(id))
        //            InsertWriter(id);
        //        return _writers[id];
        //    }

        //    void InsertWriter(char id)//при заполнение удалять и создавать новый
        //    {
        //        _writers.Add(id, new Writer()
        //        {
        //            StreamWriter = new StreamWriter($@"C:\1\Nabor{id}.txt"),
        //            FileSize = 0
        //        });
        //    }

        //    public void Dispose()
        //    {
        //        foreach (var writer in _writers.Values)
        //            writer.StreamWriter.Dispose();
        //        _writers.Clear();
        //    }

        //    private class Writer
        //    {
        //        public StreamWriter StreamWriter { get; set; }
        //        public int FileSize { get; set; }

        //        public int LineCount { get; set; }
        //        public int FilePostfix { get; set; }
        //    }
        //}
        #endregion
    }
}
