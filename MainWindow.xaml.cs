using System;
using System.Collections.Generic;
using System.Linq;
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
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using System.ComponentModel;

namespace CarsAndPitsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        ValuesNet net;
        ValuesNetElement vnet;      
        int maxDepth = 40;
        long valuesCount = 10000;                
         
        public MainWindow()
        {
            InitializeComponent();            
            MyWindow.Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            KeyDown += MainWindow_KeyDown;            
            buttonSelectFolder.Click += delegate
            {
                net = new ValuesNet(maxDepth);
                init(selectFolder());
            };

            string folder = Properties.Settings.Default.LastFolder;
            if (folder == "null")
                folder = selectFolder();

            init(folder);
        }

        private string selectFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().Value)
            {
                Properties.Settings.Default.LastFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();
                return dialog.SelectedPath;
            }
            else return "null";
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                #region reservedButtons
                case Key.Add:
                    /*if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        deepness++;
                        if (deepness > 15) deepness = 15;
                        Title = "Deepness " + deepness.ToString();
                        break;
                    }
                    if (currentLevel >= net.maxDepth-1)
                        break;
                    currentLevel++;
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        MainCanvas.Children.Clear();
                        MainCanvas.Children.Add(zerorectgon);
                    }
                    addrectsToCanvas(net.generaterectangelsLayer(currentLevel));     */               
                    break;
                case Key.Subtract:
                    /*if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        deepness--;
                        if (deepness < 1) deepness = 1;
                        Title = "Deepness " + deepness.ToString();
                        break;
                    }
                    if (currentLevel < 2)
                        break;
                    currentLevel--;
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        MainCanvas.Children.Clear();
                        MainCanvas.Children.Add(zerorectgon);
                    }
                    addrectsToCanvas(net.generaterectangelsLayer(currentLevel));     */               
                    break;
                case Key.C:
                    MainCanvas.Children.Clear();
                    //MainCanvas.Children.Add(zerorectgon);
                    break;
                case Key.B:
                    //setBaseScale();
                    break;
#endregion
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
            }
        }    

        public void init() //random initializer
        {
            net = new ValuesNet(maxDepth);
            Random rnd = new Random();                        
            for (int i = 0; i < valuesCount; i++)
            {
                Point p = new Point(rnd.NextDouble() * 360 - 180, rnd.NextDouble() * 180 - 90);
                net.putValue(p.Y, p.X, 3);
            }

            SquaresCount.Content = net.totalSquaresCount.ToString();
            Ratio.Content = ((double)valuesCount / net.totalSquaresCount).ToString("0.####");
            MemoryUsage.Content = GC.GetTotalMemory(true) / 1024 / 1024 + "MB";
            Accuracy.Content = "(Accuracy: " + net.accuracy + ")";

            vnet = new ValuesNetElement(MainCanvas, net);
            vnet.MouseMove += delegate
            {
                Title = String.Format("X: {0}  Y: {1}", vnet.coordinates.X, vnet.coordinates.Y);
            };
            vnet.MouseWheel += delegate
            {
                Title = vnet.visibleSquaresCount.ToString();
            };

            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(vnet);
        }

        private void init(string folder)
        {
            List<string> directories = new List<string>();
            List<string> files = new List<string>();
            foreach (string path in Directory.GetDirectories(folder))
            {
                if (Directory.Exists(path)) directories.Add(path);
                else if (File.Exists(path)) files.Add(path);
            }

            List<CPDataGeo> data = new List<CPDataGeo>();

            Dictionary<SensorType, CPData> CPdata;

            updateActionProgress("Files reading", 0);
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, e) =>
            {
                double i = 0;
                foreach (string path in directories)
                {
                    CPdata = CPData.fromDirectory(path);
                    if (CPdata.ContainsKey(SensorType.ACCELEROMETER) && CPdata.ContainsKey(SensorType.GPS))
                        data.Add(new CPDataGeo(CPdata[SensorType.ACCELEROMETER], CPdata[SensorType.GPS]));
                    i++;

                    bw.ReportProgress((int)(i / directories.Count * 100));
                }

                CPdata = CPData.fromDirectory(folder);
                if (CPdata.ContainsKey(SensorType.ACCELEROMETER) && CPdata.ContainsKey(SensorType.GPS))
                    data.Add(new CPDataGeo(CPdata[SensorType.ACCELEROMETER], CPdata[SensorType.GPS]));
            };
            bw.ProgressChanged += (o,args) => {
                updateActionProgress("Files reading", args.ProgressPercentage);
            };
            bw.RunWorkerCompleted += delegate
            {
                updateActionProgress("Files reading", 100);
                init(data);
            };
            bw.RunWorkerAsync();
        }

        private void init(List<CPDataGeo> CPdata)
        {
            net = new ValuesNet(maxDepth);
            updateActionProgress("ValuesNet filling", 0);            

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, e) =>
            {
                double counter = 0;
                double totalCount = 0;
                foreach (CPDataGeo data in CPdata)
                    totalCount += data.geoData.Length;

                foreach (CPDataGeo data in CPdata)
                {
                    int part = 0;
                    int triggerPart = data.geoData.Length / 100;
                    foreach (DataTuplyaGeo tuplya in data.geoData)
                    {
                        double value = 0;
                        foreach (double v in tuplya.values)
                            value += Math.Abs(v);
                        net.putValue(tuplya.coordinate.Latitude, tuplya.coordinate.Longitude, value);
                        counter++;
                        part++;

                        if (part >= triggerPart)
                        {
                            part = 0;
                            bw.ReportProgress((int)(counter / totalCount * 100));
                        }
                    }                    
                }    
            };
            bw.ProgressChanged += (o, args) => {
                updateActionProgress("ValuesNet filling", args.ProgressPercentage);
            };
            bw.RunWorkerCompleted += delegate
            {
                updateActionProgress("ValuesNet filling", 100);

                SquaresCount.Content = net.totalSquaresCount.ToString();
                Ratio.Content = ((double)valuesCount / net.totalSquaresCount).ToString("0.####");
                MemoryUsage.Content = GC.GetTotalMemory(true) / 1024 / 1024 + "MB";
                Accuracy.Content = "(Accuracy: " + net.accuracy + ")";

                vnet = new ValuesNetElement(MainCanvas, net);
                vnet.MouseMove += delegate
                {
                    Title = String.Format("X: {0}  Y: {1}", vnet.coordinates.X, vnet.coordinates.Y);
                };
                vnet.MouseWheel += delegate
                {
                    Title = vnet.visibleSquaresCount.ToString();
                };

                MainCanvas.Children.Clear();
                MainCanvas.Children.Add(vnet);
            };
            bw.RunWorkerAsync();                       
        }

        public void updateActionProgress(string title, double progress)
        {
            progressBarFileLoading.Value = progress / 100;
            Title = title + "(" + (progress).ToString("n0") + "%)";

            //MyWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);            
            //progressBarFileLoading.Dispatcher.Invoke(() => progressBarFileLoading.Value = progress / 100, DispatcherPriority.Background);
            //progressBarFileLoading.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
        private static Action EmptyDelegate = delegate () { };
    }    
}
