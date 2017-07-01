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

namespace CarsAndPitsWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        ValuesNet net;
        int currentLevel = 3;
        int deepness = 7;
        int maxDepth = 20;
        long valuesCount = 10000;              
         
        public MainWindow()
        {
            InitializeComponent();
            init();            

            MyWindow.Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            sliderValuesCount.ValueChanged += SliderValuesCount_ValueChanged;
            sliderLevelsCount.ValueChanged += SliderLevelsCount_ValueChanged;
            KeyDown += MainWindow_KeyDown;
            buttonRecalculate.Click += ButtonRecalculate_Click;
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
                    if (currentLevel >= net.maxDepth)
                        break;
                    currentLevel++;                    
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                        MainCanvas.Children.Clear();
                    addRectsToCanvas(net.generateRectangelsLayer(currentLevel));                    
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
                        MainCanvas.Children.Clear();
                    addRectsToCanvas(net.generateRectangelsLayer(currentLevel));                    
                    break;
                case Key.C:
                    MainCanvas.Children.Clear();
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


            //List<Rectangle> rects = new List<Rectangle>();            
            //rects.AddRange(net.generateRectangelsLayer(3));

            //addRectsToCanvas(rects);
            //setBaseScale();            
        }

        private void setBaseScale()
        {            
            Matrix m = new Matrix();            
            m.Translate(0, 0);
            m.Scale(4, 4);

            MatrixTransform mTransform = new MatrixTransform(m);

            MainCanvas.RenderTransform = mTransform;
            MainCanvas.UpdateLayout();
        }

        private void fitToRectangle(Rectangle rect)
        {
            double ScaleRateX = MyGrid.RenderSize.Width / rect.Width;
            double ScaleRateY = MyGrid.RenderSize.Height / rect.Height;
            double ScaleRate = Math.Min(ScaleRateX, ScaleRateY) * 0.96;            
            Matrix m = new Matrix();
            double translateLeft = Canvas.GetLeft(rect);
            double translateTop = Canvas.GetTop(rect);
            m.Translate(-translateLeft, -translateTop);
            m.Scale(ScaleRate, ScaleRate);

            MatrixTransform mTransform = new MatrixTransform(m);

            MainCanvas.RenderTransform = mTransform;
            MainCanvas.UpdateLayout();            
        }

        private void addRectsToCanvas(List<Rectangle> rects)
        {
            foreach (Rectangle rect in rects)
            {
                MainCanvas.Children.Add(rect);
                rect.MouseUp += Rect_MouseUp;
            }

            Title = string.Format("Total values: {0}, Total rectangles: {1}, Level: {2}, Level rectangles: {3}",
                valuesCount.ToString(), net.totalSquaresCount.ToString(), currentLevel.ToString(), rects.Count.ToString());
        }

        private void Rect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainCanvas.Children.Clear();
            Rectangle rect = (Rectangle)sender;
            fitToRectangle(rect);
            addRectsToCanvas(net.generateChildRectangles(net.getSquareByPath((int[])rect.DataContext), deepness));            
        }
    }
}
