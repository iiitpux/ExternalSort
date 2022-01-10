using A365.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A365
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(MainWindow), new PropertyMetadata(""));
        public string RamSizeLabelContent
        {
            get { return (string)GetValue(RamSizeLabelContentProperty); }
            set { SetValue(RamSizeLabelContentProperty, value); }
        }
        public static readonly DependencyProperty RamSizeLabelContentProperty = DependencyProperty.Register(nameof(RamSizeLabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public int RamSizeValue
        {
            get { return (int)GetValue(RamSizeValueProperty); }
            set { SetValue(RamSizeValueProperty, value); }
        }
        public static readonly DependencyProperty RamSizeValueProperty = DependencyProperty.Register(nameof(RamSizeValue), typeof(int), typeof(MainWindow), new PropertyMetadata(8));

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

        public bool SsdDriveChecked2
        {
            get { return (bool)GetValue(SsdDriveChecked2Property); }
            set { SetValue(SsdDriveChecked2Property, value); }
        }
        public static readonly DependencyProperty SsdDriveChecked2Property = DependencyProperty.Register(nameof(SsdDriveChecked2), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public bool UseSecondDrive
        {
            get { return (bool)GetValue(UseSecondDriveProperty); }
            set { SetValue(UseSecondDriveProperty, value); }
        }
        public static readonly DependencyProperty UseSecondDriveProperty = DependencyProperty.Register(nameof(UseSecondDrive), typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

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

        public int ThreadCount2Value
        {
            get { return (int)GetValue(ThreadCount2ValueProperty); }
            set { SetValue(ThreadCount2ValueProperty, value); }
        }
        public static readonly DependencyProperty ThreadCount2ValueProperty = DependencyProperty.Register(nameof(ThreadCount2Value), typeof(int), typeof(MainWindow), new PropertyMetadata(2));

        public string ThreadCount2LabelContent
        {
            get { return (string)GetValue(ThreadCount2LabelContentProperty); }
            set { SetValue(ThreadCount2LabelContentProperty, value); }
        }
        public static readonly DependencyProperty ThreadCount2LabelContentProperty = DependencyProperty.Register(nameof(ThreadCount2LabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public string DirectoryPath
        {
            get { return (string)GetValue(DirectoryPathProperty); }
            set { SetValue(DirectoryPathProperty, value); }
        }
        public static readonly DependencyProperty DirectoryPathProperty = DependencyProperty.Register(nameof(DirectoryPath), typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public string ResultLabelContent
        {
            get { return (string)GetValue(ResultLabelContentProperty); }
            set { SetValue(ResultLabelContentProperty, value); }
        }
        public static readonly DependencyProperty ResultLabelContentProperty = DependencyProperty.Register(nameof(ResultLabelContent), typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        #endregion

        public MainWindow()
        {
            InitializeComponent();
            //создавать папку для каждого раза
            //вынести все по правилам единой ответственности
            this.DataContext = this;

            FileButton.Click += FileButton_Click;

            RamSize.Ticks = new DoubleCollection() { 1, 2, 4, 8, 16, 32 };
            RamSize.Minimum = 1;
            RamSize.Maximum = 32;
            RamSize.IsSnapToTickEnabled = true;
            RamSize.ValueChanged += RamSize_ValueChanged;

            CoreCount.Ticks = new DoubleCollection() { 1, 2, 4, 6, 8 };
            CoreCount.Minimum = 1;
            CoreCount.Maximum = 8;
            CoreCount.IsSnapToTickEnabled = true;
            CoreCount.ValueChanged += CoreCount_ValueChanged;

            ThreadCount.Ticks = new DoubleCollection() { 1, 2, 4 };
            ThreadCount.Minimum = 1;
            ThreadCount.Maximum = 4;
            ThreadCount.IsSnapToTickEnabled = true;
            ThreadCount.ValueChanged += ThreadCount_ValueChanged;

            ThreadCount2.Ticks = new DoubleCollection() { 1, 2, 4 };
            ThreadCount2.Minimum = 1;
            ThreadCount2.Maximum = 4;
            ThreadCount2.IsSnapToTickEnabled = true;
            ThreadCount2.ValueChanged += ThreadCount2_ValueChanged;

            SecondDrive.Click += SecondDrive_Click;
        }

        #region Events
        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath = dialog.FileName;
                }
            }
        }

        private void SecondDrive_Click(object sender, RoutedEventArgs e)
        {
            if (UseSecondDrive)
            {
                Ssd2.IsEnabled = true;
                Hdd2.IsEnabled = true;
                ThreadCount2.IsEnabled = true;
            }
            else
            {
                Ssd2.IsEnabled = false;
                Hdd2.IsEnabled = false;
                ThreadCount2.IsEnabled = false;
            }
        }

        private void RamSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RamSizeLabelContent = $"Будет использовано RAM: {RamSizeValue} Gb";
        }

        private void CoreCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CoreCountLabelContent = "Ядер у процессора: " + CoreCountValue;
        }

        private void ThreadCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThreadCountLabelContent = "Потоков: " + ThreadCountValue;
        }

        private void ThreadCount2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThreadCount2LabelContent = "Потоков: " + ThreadCount2Value;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ThreadCount.IsEnabled = SsdDriveChecked;
        }

        private void RadioButton_Checked2(object sender, RoutedEventArgs e)
        {
            ThreadCount2.IsEnabled = SsdDriveChecked;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryPath = dialog.SelectedPath;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                ResultLabelContent = "Задача отменена";
            }
        }
        #endregion

        private CancellationTokenSource _cancellationTokenSource;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                System.Windows.MessageBox.Show("Нужно выбрать файл для сортировки");
                return;
            }

            if (UseSecondDrive && string.IsNullOrWhiteSpace(DirectoryPath))
            {
                System.Windows.MessageBox.Show("Нужно выбрать директорию на втором диске");
                return;
            }

            GenerateButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
            ResultLabel.Visibility = Visibility.Visible;
            ResultLabelContent = "Идет сортировка";
            DirectoryButton.IsEnabled = false;
            FileButton.IsEnabled = false;

            var fileSize = new FileInfo(FilePath).Length;

            if (fileSize < (long)RamSizeValue * 1024 * 1024 * 1024 / 4)
            {
                var sw = new Stopwatch();
                sw.Start();
                var list = File.ReadAllLines(FilePath);
                File.WriteAllLines(@$"{System.IO.Path.GetDirectoryName(FilePath)}\output_result.txt", list.OrderBy(p => p, new ItemComparer()));
                sw.Stop();
                ResultLabelContent = $"Отсортировано в памяти за {sw.ElapsedMilliseconds / 1000} секунд";
                Reset();
                return;
            }

            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

            if (UseSecondDrive)
            {
                var baseDrive = drives.SingleOrDefault(d => d.Name == System.IO.Path.GetPathRoot(FilePath));

                if (baseDrive == null || baseDrive.AvailableFreeSpace < fileSize)
                {
                    System.Windows.MessageBox.Show($"Недостаточно места на диске: {baseDrive.Name}");
                    return;
                }

                var secondDrive = drives.SingleOrDefault(d => d.Name == System.IO.Path.GetPathRoot(DirectoryPath));

                if (secondDrive == null || secondDrive.AvailableFreeSpace < fileSize + fileSize / Sorter.Dict.Length)
                {
                    System.Windows.MessageBox.Show($"Недостаточно места на диске: {secondDrive.Name}");
                    return;
                }
            }
            else
            {
                var baseDrive = drives.SingleOrDefault(d => d.Name == System.IO.Path.GetPathRoot(FilePath));

                if (baseDrive == null || baseDrive.AvailableFreeSpace < fileSize * 2 + fileSize / Sorter.Dict.Length)
                {
                    System.Windows.MessageBox.Show($"Недостаточно места на диске: {baseDrive.Name}");
                    return;
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sorter = new Sorter();
            var request = new Sorter.Request()
            {
                Token = _cancellationTokenSource.Token,
                FilePath = FilePath,
                DirectoryPath = DirectoryPath,
                UseSecondDrive = UseSecondDrive,
                RamSizeValue = RamSizeValue,
                CoreCountValue = CoreCountValue,
                ThreadCountValue = ThreadCountValue,
                ThreadCount2Value = ThreadCount2Value
            };
            await sorter.Sort(request);

            stopwatch.Stop();

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Reset();
                return;
            }

            ResultLabelContent = $"Отсортировано за {stopwatch.ElapsedMilliseconds / 1000} секунд";
        }

        
        private void Reset()
        {
            GenerateButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;
            DirectoryButton.IsEnabled = true;
            FileButton.IsEnabled = true;
        }
    }
}