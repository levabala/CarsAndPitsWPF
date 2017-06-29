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
    class ValuesNet
    {
        public Square zeroSquare = new Square(-90, -180, 0, 0);
        public int maxDepth = 5;
        public double maxValue = 200;
        public int totalSquaresCount = 0;        

        public ValuesNet(int maxDepth = 5)
        {
            this.maxDepth = maxDepth;
        }

        public void putValue(double lat, double lng, double value)
        {            
            putToSquareTree(lat, lng, value);
        }

        public List<Rectangle> generateChildRectangles(Square square, int deepness = -1)
        {
            if (deepness == 0)
                return new List<Rectangle>();
            deepness--;

            List<Rectangle>[] rectOfRects = new List<Rectangle>[] {
                    new List<Rectangle>(),new List<Rectangle>(),new List<Rectangle>(),new List<Rectangle>()
                };
            for (int i = 0; i < 4; i++)
                if (square.children[i] != null)
                {
                    rectOfRects[i].Add(generateWPFRectange(square.children[i]));
                    rectOfRects[i].AddRange(generateChildRectangles(square.children[i], deepness));
                }

            List<Rectangle> rects = new List<Rectangle>();
            bool nextAvailable = (rectOfRects[0].Count + rectOfRects[1].Count + rectOfRects[2].Count + rectOfRects[3].Count) > 0;
            int index = 0;
            while (nextAvailable) {
                nextAvailable = false;              
                if (index < rectOfRects[0].Count)
                {
                    rects.Add(rectOfRects[0][index]);
                    nextAvailable = true;
                }
                if (index < rectOfRects[1].Count)
                {
                    rects.Add(rectOfRects[1][index]);
                    nextAvailable = true;
                }
                if (index < rectOfRects[2].Count)
                {
                    rects.Add(rectOfRects[2][index]);
                    nextAvailable = true;
                }
                if (index < rectOfRects[3].Count)
                {
                    rects.Add(rectOfRects[3][index]);
                    nextAvailable = true;
                }
                index++;
            }

            return rects;
        }

        public Rectangle generateWPFRectange(Square square)
        {
            maxValue = zeroSquare.value * 1.001;

            Rectangle rect = new Rectangle();
            rect.DataContext = square.path;
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1 / Math.Pow(1.7, square.level);
            double intesity = 255 - ((square.value != 0 && square.level != 0) ? square.value : 1) / maxValue * 255;
            Color color = Color.FromRgb(255, (byte)intesity, (byte)intesity);
            Brush fillBrush = new SolidColorBrush(color);
            fillBrush.Opacity = 0.1;
            rect.Fill = fillBrush;                        
            rect.Width = 360 / Math.Pow(2, square.level);
            rect.Height = 180 / Math.Pow(2, square.level); //we use 180 instead 360 to get squares (not rectangles)
            double left = square.lng + 180;
            double top = square.lat + 90;
            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);                        
            return rect;
        }

        public List<Rectangle> generateRectangelsLayer(int level)
        {
            List<Rectangle> layer = new List<Rectangle>();
            foreach (Square s in getLayer(zeroSquare, level))
                layer.Add(generateWPFRectange(s));
            return layer;
        }

        public List<Square> getLayer(Square startSquare, int level)
        {
            List<Square> layer = new List<Square>();
            for (int i = 0; i < startSquare.children.Length; i++)
                if (startSquare.children[i] == null) continue;
                else if (startSquare.children[i].level == level)
                    layer.Add(startSquare.children[i]);
                else layer.AddRange(getLayer(startSquare.children[i], level));

            return layer;
        }

        public void putToSquareTree(double lat, double lng, double value)
        {
            Square square = zeroSquare;
            zeroSquare.value += value;

            int[] path = getPathToSquare(lat, lng);
            foreach (int i in path)
            {
                if (square.children[i] == null)
                {
                    square.children[i] = new Square(square, i, 0);
                    totalSquaresCount++;
                }

                square = square.children[i];
                square.value += value;
            }
        }
        
        public Square getSquare(double lat, double lng)
        {            
            Square square = zeroSquare;

            int[] path = getPathToSquare(lat, lng);
            square = getSquare(path);

            return square;
        }

        public Square getSquare(int[] path)
        {
            Square square = zeroSquare;            
            foreach (int i in path)
            {
                if (square.children == null)
                {
                    square.children = new Square[4];
                    square.children[i] = new Square(square, i, 0);
                    totalSquaresCount++;
                }
                else if (square.children[i].children == null)
                {
                    square.children[i] = new Square(square, i, 0);
                    totalSquaresCount++;
                }

                square = square.children[i];                
            }                

            return square;
        }

        public int[] getPathToSquare(double lat, double lng)
        {
            int[] path = new int[maxDepth-1];            
            double squareLat = -90;
            double squareLng = -180;
            for (int level = 0; level < maxDepth-1; level++)
            {                
                double sHeight = 180 / Math.Pow(2, level);
                double sWidth = 360 / Math.Pow(2, level); //squareWidth
                double centerLat = squareLat + sHeight / 2;
                double centerLng = squareLng + sWidth / 2;

                //can I replace it??
                int index = 0;
                if (lat > centerLat)
                {
                    if (lng > centerLng)
                    {
                        index = 3;
                        squareLng = centerLng;
                    }
                    else index = 2;                                            
                    squareLat = centerLat;
                }
                else if (lng > centerLng)
                {
                    index = 1;
                    squareLng = centerLng;
                }                

                path[level] = index;
            }

            return path;
        }     
        
        public Square getSquareByPath(int[] path)
        {
            Square square = zeroSquare;
            foreach (int i in path)
                square = square.children[i];
            return square;
        }        
    }

    class Square
    {
        public int level;
        public double lat;
        public double lng ;
        public double value;
        public Square[] children;
        public int[] path;           

        public Square(double lat, double lng, int level, double value)
        {
            this.level = level;
            this.lat = lat;
            this.lng = lng;
            this.value = value;
            path = new int[0];
            children = new Square[4];            
        }

        public Square(Square parent, int index, double value)
        {
            level = parent.level + 1;
            lat = parent.lat + ((index > 1) ? 180 / Math.Pow(2, parent.level) / 2 : 0);
            lng = parent.lng + ((index == 1 || index == 3) ? 360 / Math.Pow(2, parent.level) / 2 : 0);            
            children = new Square[4];

            //path remembering
            path = new int[parent.path.Length + 1];
            parent.path.CopyTo(path, 0);
            path[path.Length - 1] = index;

            this.value = value;
        }
    }
}
