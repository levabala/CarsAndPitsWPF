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

namespace CarsAndPitsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        ValuesNet net;
        int currentLevel = 0;
        int deepness = 7;
        int maxDepth = 30;
        long valuesCount = 10000;
        Polygon zeroPolygon;
        Matrix globalM = new Matrix();
         
        public MainWindow()
        {
            InitializeComponent();
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().Value)
            {

                List<string> directories = new List<string>();
                List<string> files = new List<string>();
                foreach (string path in Directory.GetDirectories(dialog.SelectedPath))
                {
                    if (Directory.Exists(path)) directories.Add(path);
                    else if (File.Exists(path)) files.Add(path);
                }

                List<CPDataGeo> data = new List<CPDataGeo>();
                if (directories.Count > 0)
                    foreach (string path in directories)
                    {
                        Dictionary<SensorType, CPData> CPdata = CPData.fromDirectory(path);
                        data.Add(new CPDataGeo(CPdata[SensorType.ACCELEROMETER], CPdata[SensorType.GPS]));
                    }

                init(data);
            }              

            MyWindow.Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            sliderValuesCount.ValueChanged += SliderValuesCount_ValueChanged;
            sliderLevelsCount.ValueChanged += SliderLevelsCount_ValueChanged;
            KeyDown += MainWindow_KeyDown;
            buttonRecalculate.Click += ButtonRecalculate_Click;
            MainCanvas.MouseWheel += MainCanvas_MouseWheel;
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(MainCanvas);
            if (e.Delta > 0)
                globalM.ScaleAt(1.1, 1.1, p.X, p.Y);
            else
                globalM.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y);

            foreach (UIElement element in MainCanvas.Children)
                element.RenderTransform = new MatrixTransform(globalM);
        }

        private void SliderLevelsCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            maxDepth = (int)e.NewValue;
        }

        private void SliderValuesCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            valuesCount = (long)e.NewValue;
        }

        private void ButtonRecalculate_Click(object sender, RoutedEventArgs e)
        {
            init();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
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
                        MainCanvas.Children.Add(zeroPolygon);
                    }
                    addpolysToCanvas(net.generatepolyangelsLayer(currentLevel));                    
                    break;
                case Key.Subtract:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
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
                        MainCanvas.Children.Add(zeroPolygon);
                    }
                    addpolysToCanvas(net.generatepolyangelsLayer(currentLevel));                    
                    break;
                case Key.C:
                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(zeroPolygon);
                    break;
                case Key.B:
                    setBaseScale();
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
            }
        }

        public void init()
        {
            net = new ValuesNet(maxDepth);
            Random rnd = new Random();                        
            for (int i = 0; i < valuesCount; i++)
            {
                Point p = new Point(rnd.NextDouble() * 360 - 180, rnd.NextDouble() * 180 - 90);
                net.putValue(p.Y, p.X, 3);// rnd.NextDouble() * 10 - 5);

                /*Ellipse circle = new Ellipse();                
                circle.Width = 3;
                circle.Height = 3;
                circle.Opacity = 1;
                circle.Fill = Brushes.Green;
                Canvas.SetLeft(circle, points[i].X + 180 - 1.5);
                Canvas.SetTop(circle, points[i].Y + 90 - 1.5);
                MainCanvas.Children.Add(circle);*/
            }

            SquaresCount.Content = net.totalSquaresCount.ToString();
            Ratio.Content = ((double)valuesCount / net.totalSquaresCount).ToString("0.####");
            MemoryUsage.Content = GC.GetTotalMemory(true) / 1024 / 1024 + "MB";
            Accuracy.Content = "(Accuracy: " + net.accuracy + ")";


            //List<Polygon> polys = new List<Polygon>();            
            //polys.AddRange(net.generatepolyangelsLayer(3));

            //addpolysToCanvas(polys);
            //setBaseScale();            
        }

        private void init(List<CPDataGeo> CPdata)
        {
            net = new ValuesNet(maxDepth);
            foreach (CPDataGeo data in CPdata)
                foreach (DataTuplyaGeo tuplya in data.geoData) {
                    double value = 0;
                    foreach (double v in tuplya.values)
                        value += Math.Abs(v);
                    net.putValue(tuplya.coordinate.Latitude, tuplya.coordinate.Longitude, value);
                }

            SquaresCount.Content = net.totalSquaresCount.ToString();
            Ratio.Content = ((double)valuesCount / net.totalSquaresCount).ToString("0.####");
            MemoryUsage.Content = GC.GetTotalMemory(true) / 1024 / 1024 + "MB";
            Accuracy.Content = "(Accuracy: " + net.accuracy + ")";

            zeroPolygon = net.generateWPFPolygon(net.zeroSquare);
            MainCanvas.Children.Add(zeroPolygon);
            setBaseScale();            
        }

        private void setBaseScale()
        {
            globalM.Scale(4, 4);            
            foreach (UIElement element in MainCanvas.Children)            
                element.RenderTransform = new MatrixTransform(globalM);            
        }

        private void fitToPolygon(Polygon poly)
        {
            
        }

        private void addpolysToCanvas(List<Polygon> polys)
        {
            if (polys.Count < 4000)
                foreach (Polygon poly in polys)
                {                                
                    poly.RenderTransform = new MatrixTransform(globalM);
                    MainCanvas.Children.Add(poly);
                    poly.MouseUp += poly_MouseUp;
                }

            Title = string.Format("Total values: {0}, Total Polygons: {1}, Level: {2}, Level Polygons: {3}",
                valuesCount.ToString(), net.totalSquaresCount.ToString(), currentLevel.ToString(), polys.Count.ToString());
        }

        private void poly_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Clear();
            MainCanvas.Children.Add(zeroPolygon);
            Polygon poly = (Polygon)sender;
            //fitToPolygon(poly);
            addpolysToCanvas(net.generateChildPolygons(net.getSquareByPath((int[])poly.DataContext), 7));            
        }
    }
}
