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
        ValuesNet net = new ValuesNet(23);
        int currentLevel = 3;
        int valuesCount = 1000000;              
         
        public MainWindow()
        {
            InitializeComponent();            
            init();

            MyWindow.Loaded += MyWindow_Loaded;
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                    if (currentLevel >= net.maxDepth)
                        break;
                    currentLevel++;                    
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                        MainCanvas.Children.Clear();
                    addRectsToCanvas(net.generateRectangelsLayer(currentLevel));                    
                    break;
                case Key.Subtract:
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
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
            }
        }

        public void init()
        {
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

            List<Rectangle> rects = new List<Rectangle>();
            //rects.AddRange(net.generateChildRectangles(net.baseSquare));            
            rects.AddRange(net.generateRectangelsLayer(3));

            addRectsToCanvas(rects);                        

            double ScaleRateX = MyWindow.Width / 180;
            double ScaleRateY = MyWindow.Height / 180;
            double ScaleRate = 4;// Math.Min(ScaleRateX, ScaleRateY);
            ScaleTransform scale = new ScaleTransform(
                MainCanvas.LayoutTransform.Value.M11 * ScaleRate, MainCanvas.LayoutTransform.Value.M22 * ScaleRate
                );
            MainCanvas.LayoutTransform = scale;
            MainCanvas.UpdateLayout();
        }

        private void addRectsToCanvas(List<Rectangle> rects)
        {
            if (currentLevel < 8)
                foreach (Rectangle rect in rects)
                    MainCanvas.Children.Add(rect);

            Title = string.Format("Total values: {0}, Total rectangles: {1}, Level: {2}, Level rectangles: {3}",
                valuesCount.ToString(), net.totalSquaresCount.ToString(), currentLevel.ToString(), rects.Count.ToString());
        }
    }
}
