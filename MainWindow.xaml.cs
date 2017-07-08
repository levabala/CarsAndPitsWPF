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
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.Specialized;

namespace CarsAndPitsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        ValuesNet net;
        ValuesNetElement vnet;
        ValuesNetGMap vnetGMap;
        int maxDepth = 25; //so we'll get accuracy close to 7 meters
        long valuesCount = 10000;                
         
        public MainWindow()
        {            
            InitializeComponent();            
            MyWindow.Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string folder;

            KeyDown += MainWindow_KeyDown;            
            buttonSelectFolder.Click += delegate
            {
                net = new ValuesNet(maxDepth);
                folder = selectFolder();
                if (folder == "null")
                    return;
                init(folder);
            };

            //MyGrid.Children.Add(mapView);

            StringCollection cachedData = Properties.Settings.Default.CachedData;

            folder = Properties.Settings.Default.LastFolder;
            if (folder == "null")
            {
                folder = selectFolder();
                if (folder == "null")
                    return;
            }
                

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

            ValuesNetManager.fillValuesNet(net, rnd, valuesCount,
                (s, args) =>
                {
                    updateActionProgress(args.ProgressPercentage);
                },
                (s, args) =>
                {
                    //vnet = new ValuesNetElement(MainCanvas, net);                    
                    ValuesNetGMap vnetGMap = new ValuesNetGMap(net, mapView);    

                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(vnetGMap);                    
                });            
        }

        private void init(string folder)
        {
            net = new ValuesNet(maxDepth);            
            ValuesNetManager.fillValuesNet(net, folder,
                (s, args) =>
                {
                    updateActionProgress( args.ProgressPercentage);
                },
                (s, args) =>
                {
                    vnetGMap = new ValuesNetGMap(net, mapView);
                    mapView.OnMapZoomChanged += delegate
                    {
                        Title = vnetGMap.visibleSquaresCount.ToString();
                    };
                    mapView.OnMapDrag += delegate
                    {
                        Title = vnetGMap.visibleSquaresCount.ToString();
                    };

                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(vnetGMap);

                    /*vnet = new ValuesNetElement(MainCanvas, net);

                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(vnet);*/
                });
        }

        private void init(List<CPDataGeo> CPdata)
        {
            net = new ValuesNet(maxDepth);            

            ValuesNetManager.fillValuesNet(net, CPdata,
                (s, args) =>
                {
                    updateActionProgress(args.ProgressPercentage);
                },
                (s, args) =>
                {
                    vnet = new ValuesNetElement(MainCanvas, net);

                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(vnet);
                });            
        }

        public void updateActionProgress(string title, double progress)
        {
            progressBarFileLoading.Value = progress / 100;
            Title = title + "(" + (progress).ToString("n0") + "%)";            
        }

        public void updateActionProgress(double progress)
        {
            progressBarFileLoading.Value = progress / 100;            
        }

        private static Action EmptyDelegate = delegate () { };
    }    
}
