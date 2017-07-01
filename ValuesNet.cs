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
using System.Device.Location;

namespace CarsAndPitsWPF
{
    class ValuesNet
    {        
        public Square zeroSquare = new Square(-90, -180, 0, 0);
        public int maxDepth;
        public double maxValue = 200;
        public int totalSquaresCount = 0;
        public string accuracy = "0m";

        public ValuesNet(int maxDepth = 5)
        {
            this.maxDepth = maxDepth;

            double latDist = 180d / Math.Pow(2, maxDepth - 1);
            double lngDist = 360d / Math.Pow(2, maxDepth - 1);
            double accuracy = new GeoCoordinate(0, 0).GetDistanceTo(new GeoCoordinate(latDist, lngDist));
            string postFix = "m";            
            int digit = (int)Math.Log10(accuracy);
            if (digit >= 9)
            {
                postFix = "Gm";
                accuracy /= 1000 / 1000 / 1000;
            }
            else if (digit >= 6)
            {
                postFix = "Mm";
                accuracy /= 1000 / 1000;
            }
            else if (digit >= 3)
            {
                postFix = "km";
                accuracy /= 1000;
            }
            string preFix = accuracy.ToString("n2");
            this.accuracy = preFix + postFix;
        }

        public void putValue(double lat, double lng, double value)
        {            
            putToSquareTree(lat, lng, value);
        }

        public List<Polygon> generateChildPolygons(Square square, int deepness = -1)
        {
            if (deepness == 0)
                return new List<Polygon>();
            deepness--;

            List<Polygon>[] polyOfpolys = new List<Polygon>[] {
                    new List<Polygon>(),new List<Polygon>(),new List<Polygon>(),new List<Polygon>()
                };
            for (int i = 0; i < 4; i++)
                if (square.children[i] != null)
                {
                    polyOfpolys[i].Add(generateWPFPolygon(square.children[i]));
                    polyOfpolys[i].AddRange(generateChildPolygons(square.children[i], deepness));
                }

            List<Polygon> polys = new List<Polygon>();
            bool nextAvailable = (polyOfpolys[0].Count + polyOfpolys[1].Count + polyOfpolys[2].Count + polyOfpolys[3].Count) > 0;
            int index = 0;
            while (nextAvailable) {
                nextAvailable = false;              
                if (index < polyOfpolys[0].Count)
                {
                    polys.Add(polyOfpolys[0][index]);
                    nextAvailable = true;
                }
                if (index < polyOfpolys[1].Count)
                {
                    polys.Add(polyOfpolys[1][index]);
                    nextAvailable = true;
                }
                if (index < polyOfpolys[2].Count)
                {
                    polys.Add(polyOfpolys[2][index]);
                    nextAvailable = true;
                }
                if (index < polyOfpolys[3].Count)
                {
                    polys.Add(polyOfpolys[3][index]);
                    nextAvailable = true;
                }
                index++;
            }

            return polys;
        }

        public Polygon generateWPFPolygon(Square square)
        {
            maxValue = zeroSquare.value * 1.001;
            
            Polygon poly = new Polygon();
            poly.DataContext = square.path;
            poly.Stroke = Brushes.Black;
            poly.StrokeThickness = 1 / Math.Pow(1.8, square.level);
            double intesity = 255 - ((square.value != 0 && square.level != 0) ? square.value : 1) / maxValue * 255;
            Color color = Color.FromRgb(255, (byte)intesity, (byte)intesity);
            Brush fillBrush = new SolidColorBrush(color);
            fillBrush.Opacity = 0.1;
            poly.Fill = fillBrush;                    
                
            double width = 360 / Math.Pow(2, square.level);
            double height = 180 / Math.Pow(2, square.level); //we use 180 instead 360 to get squares (not Polygons)
            double left = square.lng + 180;
            double top = square.lat + 90;
            poly.Points = new PointCollection()
            {
                new Point(left,top), new Point(left + width,  top),
                new Point(left + width, top + height), new Point(left, top + height)
            };                                    
            return poly;
        }

        public List<Polygon> generatepolyangelsLayer(int level)
        {
            List<Polygon> layer = new List<Polygon>();
            foreach (Square s in getLayer(zeroSquare, level))
                layer.Add(generateWPFPolygon(s));
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
