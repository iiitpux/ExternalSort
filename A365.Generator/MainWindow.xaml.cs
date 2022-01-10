﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;

namespace A365.Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private Dictionary<int, (int, string)> _fileSizeDict = new Dictionary<int, (int, string)>();

        public string FirstString
        {
            get { return (string)GetValue(FirstStringProperty); }
            set { SetValue(FirstStringProperty, value); }
        }
        public static readonly DependencyProperty FirstStringProperty = DependencyProperty.Register(nameof(FirstString), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public int FileSizeValue
        {
            get { return (int)GetValue(FileSizeValueProperty); }
            set { SetValue(FileSizeValueProperty, value); }
        }
        public static readonly DependencyProperty FileSizeValueProperty = DependencyProperty.Register(nameof(FileSizeValue), typeof(int), typeof(MainWindow), new PropertyMetadata(2));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public int CoreCountValue
        {
            get { return (int)GetValue(CoreCountValueProperty); }
            set { SetValue(CoreCountValueProperty, value); }
        }
        public static readonly DependencyProperty CoreCountValueProperty = DependencyProperty.Register(nameof(CoreCountValue), typeof(int), typeof(MainWindow), new PropertyMetadata(2));

        public string CoreCountLabelContent
        {
            get { return (string)GetValue(CoreCountLabelContentProperty); }
            set { SetValue(CoreCountLabelContentProperty, value); }
        }
        public static readonly DependencyProperty CoreCountLabelContentProperty = DependencyProperty.Register(nameof(CoreCountLabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        public bool SsdDriveChecked
        {
            get { return (bool)GetValue(SsdDriveCheckedProperty); }
            set { SetValue(SsdDriveCheckedProperty, value); }
        }
        public static readonly DependencyProperty SsdDriveCheckedProperty = DependencyProperty.Register(nameof(SsdDriveChecked), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public int ThreadCountValue
        {
            get { return (int)GetValue(ThreadCountValueProperty); }
            set { SetValue(ThreadCountValueProperty, value); }
        }
        public static readonly DependencyProperty ThreadCountValueProperty = DependencyProperty.Register(nameof(ThreadCountValue), typeof(int), typeof(MainWindow), new PropertyMetadata(2));

        public string ThreadCountLabelContent
        {
            get { return (string)GetValue(ThreadCountLabelContentProperty); }
            set { SetValue(ThreadCountLabelContentProperty, value); }
        }
        public static readonly DependencyProperty ThreadCountLabelContentProperty = DependencyProperty.Register(nameof(ThreadCountLabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public string ResultLabelContent
        {
            get { return (string)GetValue(ResultLabelContentProperty); }
            set { SetValue(ResultLabelContentProperty, value); }
        }
        public static readonly DependencyProperty ResultLabelContentProperty = DependencyProperty.Register(nameof(ResultLabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        private CancellationTokenSource _cancellationTokenSource;
        private string _outputFileName = "output";
        //выводить сколько процентов выполнено
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            FileSize.Ticks = new DoubleCollection() { 1, 2, 3, 4, 5 };
            FileSize.Minimum = 1;
            FileSize.Maximum = 5;
            FileSize.IsSnapToTickEnabled = true;
            FileSize.ValueChanged += FileSize_ValueChanged;
            _fileSizeDict.Add(1, (1, "1 Gb"));
            _fileSizeDict.Add(2, (10, "10 Gb"));
            _fileSizeDict.Add(3, (100, "100 Gb"));
            _fileSizeDict.Add(4, (500, "500 Gb"));
            _fileSizeDict.Add(5, (1000, "1 Tb"));

            CoreCount.Ticks = new DoubleCollection() { 1, 2, 4, 6, 8 };
            CoreCount.Minimum = 1;
            CoreCount.Maximum = 8;
            CoreCount.IsSnapToTickEnabled = true;
            CoreCount.ValueChanged += CoreCount_ValueChanged;

            ThreadCount.Ticks = new DoubleCollection() { 1, 2, 4 };
            ThreadCount.Minimum = 1;
            ThreadCount.Maximum = 4;
            ThreadCount.IsSnapToTickEnabled = true;
            ThreadCount.ValueChanged += ThreadCount_ValueChanged; ;

            SsdDriveChecked = true;
            ResultLabel.Visibility = Visibility.Collapsed;

            Reset();
        }

        private void ThreadCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThreadCountLabelContent = "Потоков: " + ThreadCountValue;
        }

        private void CoreCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CoreCountLabelContent = "Ядер у процессора: " + CoreCountValue;
        }

        private void FileSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.FirstString = "Размер файла: " + _fileSizeDict[FileSizeValue].Item2;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)//Уточнить как правильно работать с async в wpf
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                System.Windows.MessageBox.Show("Нужно выбрать директорию для файла");
                return;
            }

            var drive = DriveInfo.GetDrives().SingleOrDefault(d => d.IsReady && d.Name == System.IO.Path.GetPathRoot(FilePath));

            var minFileSizeInByte = (long)_fileSizeDict[FileSizeValue].Item1 * 1024 * 1024 * 1024;
            if (drive == null || drive.AvailableFreeSpace < minFileSizeInByte + 200)
            {
                System.Windows.MessageBox.Show("Недостаточно места на дистке");
                return;
            }

            GenerateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
            ResultLabel.Visibility = Visibility.Visible;
            ResultLabelContent = "Идет генерация";
            DirectoryButton.IsEnabled = false;

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new List<Task>();
            var token = _cancellationTokenSource.Token;
            var count = Math.Min(CoreCountValue, ThreadCountValue);
            var filePath = FilePath;
            for (int i = 0; i < count; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() => Generate(index, minFileSizeInByte / count, filePath, token), token));
            }
            await Task.WhenAll(tasks);

            if (token.IsCancellationRequested)
            {
                for (int i = 0; i < count; i++)
                {
                    File.Delete($@"{FilePath}\{_outputFileName}{i}.txt");
                }
            }
            else
            {
                if (count != 1)
                {
                    using (var destStream = File.Create($@"{FilePath}\{_outputFileName}.txt"))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            using (var srcStream = File.OpenRead($@"{FilePath}\{_outputFileName}{i}.txt")) srcStream.CopyTo(destStream);
                            File.Delete($@"{FilePath}\{_outputFileName}{i}.txt");
                        }
                    }
                }
                else
                {
                    File.Move($@"{FilePath}\{_outputFileName}0.txt", $@"{FilePath}\{_outputFileName}.txt");
                }
            }
            sw.Stop();

            Reset();

            ResultLabelContent = $"Сгенерировано за {sw.ElapsedMilliseconds/1000} секунд";
        }

        private void Generate(int index, long minFileSizeInByte, string filePath, CancellationToken tokeb)
        {
            long fileSizeInByte = 0;
            var wordGenerator = new WordGenerator();
            using (StreamWriter file = new StreamWriter(@$"{filePath}\{_outputFileName}{index}.txt", true))//имя файла из фронта
            {
                Random rnd = new Random();
                while (fileSizeInByte < minFileSizeInByte)
                {
                    if (tokeb.IsCancellationRequested)
                        break;

                    var value = wordGenerator.Next();
                    fileSizeInByte += Encoding.UTF8.GetByteCount(value);
                    file.WriteLine(value);
                    fileSizeInByte += 2;
                }
            }
        }

        public class WordGenerator
        {
            private List<int> _pattern = new List<int>();
            private string[] _dict = new string[] { "accept", "barefoot", "coast", "define", "enable", "fault", "glass",
                                                    "handle", "include", "kernel", "leak", "match", "network", "offset",
                                                    "patch", "quality", "rate", "salt", "tag", "unique", "visible", "weak",
                                                    "yield", "zookeeper"};
            private string _separtor = ". ";
            private int _maxValue;
            private StringBuilder _stringBuilder;
            private Random _random;
            private List<string> _duplicate;

            public WordGenerator()
            {
                _maxValue = _dict.Length - 1;
                _stringBuilder = new StringBuilder();
                _duplicate = new List<string>();

                _random = new Random();

                for (int i = _dict.Length - 1; i > 0; i--)
                {
                    int swapIndex = _random.Next(i + 1);
                    var tmp = _dict[i];
                    _dict[i] = _dict[swapIndex];
                    _dict[swapIndex] = tmp;
                }

                _pattern.Add(0);
            }

            public string Next()
            {
                _stringBuilder.Clear();

                _stringBuilder.Append(_random.Next(0, int.MaxValue));
                _stringBuilder.Append(_separtor);

                if (_random.Next(0, 100) == 7 && _duplicate.Any())
                {
                    var str = _duplicate[_random.Next(0, _duplicate.Count - 1)];
                    _stringBuilder.Append(str);

                    if (_duplicate.Count > 100)
                        _duplicate.RemoveAt(_random.Next(0, _duplicate.Count - 1));

                    return _stringBuilder.ToString();
                }

                PlusOne(_pattern.Count - 1);

                _stringBuilder.Append(_dict[_random.Next(0, _maxValue)]);
                _stringBuilder.Append(" ");

                for (int i = 0; i < _pattern.Count; i++)
                {
                    _stringBuilder.Append(_dict[_pattern[i]]);

                    if (i != _pattern.Count - 1)
                        _stringBuilder.Append(" ");
                }

                var result = _stringBuilder.ToString();

                if (_random.Next(0, 100) == 5)
                {
                    _duplicate.Add(result.Split(_separtor)[1]);
                }

                return result;
            }

            private void PlusOne(int index)
            {
                if (_pattern[index] + 1 > _maxValue)
                {
                    if (index == 0)
                    {
                        _pattern.Add(0);
                        FillPattern();
                        return;
                    }
                    _pattern[index] = 0;
                    PlusOne(index - 1);
                }
                else
                    _pattern[index]++;
            }

            private void FillPattern()
            {
                for (int i = 0; i < _pattern.Count; i++)
                {
                    _pattern[i] = _random.Next(0, _maxValue);
                }
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath = dialog.SelectedPath;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ThreadCount.IsEnabled = SsdDriveChecked;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            ResultLabelContent = "Задача отменена";
            Reset();
        }

        private void Reset()
        {
            GenerateButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;
            DirectoryButton.IsEnabled = true;
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

}
