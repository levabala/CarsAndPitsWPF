﻿using System;
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
            maxValue = zeroSquare.value * 1.001;
        }

        /*public List<Square> generateChildSquares(Square square, int deepness = -1)
        {
            if (deepness == 0)
                return new List<Square>();
            deepness--;

            List<Square>[] rectOfrects = new List<Square>[] {
                    new List<Square>(),new List<Square>(),new List<Square>(),new List<Square>()
                };
            for (int i = 0; i < 4; i++)
                if (square.children[i] != null)
                {
                    rectOfrects[i].Add(generateWPFSquare(square.children[i]));
                    rectOfrects[i].AddRange(generateChildSquares(square.children[i], deepness));
                }

            List<Square> rects = new List<Square>();
            bool nextAvailable = (rectOfrects[0].Count + rectOfrects[1].Count + rectOfrects[2].Count + rectOfrects[3].Count) > 0;
            int index = 0;
            while (nextAvailable) {
                nextAvailable = false;              
                if (index < rectOfrects[0].Count)
                {
                    rects.Add(rectOfrects[0][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[1].Count)
                {
                    rects.Add(rectOfrects[1][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[2].Count)
                {
                    rects.Add(rectOfrects[2][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[3].Count)
                {
                    rects.Add(rectOfrects[3][index]);
                    nextAvailable = true;
                }
                index++;
            }

            return rects;
        }

        public List<Square> generaterectangelsLayer(int level)
        {
            List<Square> layer = new List<Square>();
            foreach (Square s in getLayer(zeroSquare, level))
                layer.Add(generateWPFSquare(s));
            return layer;
        }*/

        public List<Square> getChildSquares(Square square, int deepness = -1)
        {
            if (deepness == 0)
                return new List<Square>();
            deepness--;

            List<Square>[] rectOfrects = new List<Square>[] {
                    new List<Square>(),new List<Square>(),new List<Square>(),new List<Square>()
                };
            for (int i = 0; i < 4; i++)
                if (square.children[i] != null)
                {
                    rectOfrects[i].Add(square.children[i]);
                    rectOfrects[i].AddRange(getChildSquares(square.children[i], deepness));
                }

            List<Square> rects = new List<Square>();
            bool nextAvailable = (rectOfrects[0].Count + rectOfrects[1].Count + rectOfrects[2].Count + rectOfrects[3].Count) > 0;
            int index = 0;
            while (nextAvailable)
            {
                nextAvailable = false;
                if (index < rectOfrects[0].Count)
                {
                    rects.Add(rectOfrects[0][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[1].Count)
                {
                    rects.Add(rectOfrects[1][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[2].Count)
                {
                    rects.Add(rectOfrects[2][index]);
                    nextAvailable = true;
                }
                if (index < rectOfrects[3].Count)
                {
                    rects.Add(rectOfrects[3][index]);
                    nextAvailable = true;
                }
                index++;
            }

            return rects;
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

        public Square calcChild(Square s, int index)
        {
            return new Square(s, index, s.value);
        }
        
        public Square getSquare(double lat, double lng)
        {            
            Square square = zeroSquare;

            int[] path = getPathToSquare(lat, lng);
            foreach (int i in path)
                if (square.children[i] == null) return square;
                else square = square.children[i];

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

        public Square getUpperParent(Square baseSquare, int level)
        {
            return getSquare(baseSquare.path.Take(level).ToArray());
        }        

        public Square getMostSquare(Square baseSquare, int[] preferMap, int maxLevel = -1)
        {            
            bool childAvailable = baseSquare.children[0] != null || baseSquare.children[1] != null || baseSquare.children[2] != null || baseSquare.children[3] != null;
            Square square = baseSquare;
            while (childAvailable && square.level >= maxLevel)
            {                
                foreach (int i in preferMap)
                    if (square.children[i] != null)
                    {
                        square = square.children[i];
                        childAvailable = true;
                        break;
                    }
            }

            return square;
        }
        
        public List<Square> getSquaresInViewRect(Square baseSquare, Rect viewRect, int deepness = -1)
        {            
            if (deepness == -1) deepness = maxDepth;

            List<Square> visibleSquares = new List<Square>();
            Square square = baseSquare;
            Point[] points = new Point[]
            {
                new Point(viewRect.X, viewRect.Y),
                new Point(viewRect.X + viewRect.Width, viewRect.Y),                
                new Point(viewRect.X, viewRect.Y + viewRect.Height),
                new Point(viewRect.X + viewRect.Width, viewRect.Y + viewRect.Height)
            };

            for (int ii = 0; ii < 4; ii++)
            {
                if (points[ii].X > 180) points[ii].X = 180;
                else if (points[ii].X < -180) points[ii].X = -180;
                if (points[ii].Y > 90) points[ii].Y = 90;
                else if (points[ii].Y < -90) points[ii].Y = -90;
            }

            int[][] pathes = new int[][]
            {
                getPathToSquare(points[0].Y, points[0].X, baseSquare.level),
                getPathToSquare(points[1].Y, points[1].X, baseSquare.level),
                getPathToSquare(points[2].Y, points[2].X, baseSquare.level),
                getPathToSquare(points[3].Y, points[3].X, baseSquare.level)
            };

            Point centerPoint = new Point(viewRect.X + viewRect.Width / 2, viewRect.Y + viewRect.Height / 2);
            int[] centerPath = getPathToSquare(centerPoint.Y, centerPoint.X, baseSquare.level);

            int depth = Math.Max(Math.Max(pathes[0].Length, pathes[1].Length), Math.Max(pathes[2].Length, pathes[3].Length));
            if (depth > deepness) depth = deepness;

            #region precalculated chooseMap
            int[][][] chooseMap = new int[][][]
            {
                new int[][]
                {
                    new int[] {0,1,2,3},
                    new int[] {1,3},
                    new int[] {2,3},
                    new int[] {3},
                    new int[] {}
                },
                new int[][]
                {
                    new int[] {0,2},
                    new int[] {0,1,2,3},
                    new int[] {2},
                    new int[] {2,3},
                    new int[] {}
                },
                new int[][]
                {
                    new int[] {0,1},
                    new int[] {1},
                    new int[] {0,1,2,3},
                    new int[] {1,3},
                    new int[] {}
                },
                new int[][]
                {
                    new int[] {0},
                    new int[] {0,1},
                    new int[] {0,2},
                    new int[] {0,1,2,3},
                    new int[] {}
                }
            };
            #endregion

            Square[] centerSquares = getSquaresArray(centerPath, true);
            Square lowermostCommonSquare = baseSquare;
            for (int i = centerSquares.Length - 1; i > -1; i--) {
                Square s = centerSquares[i];
                bool p0In = pointInRect(new Rect(s.lng, s.lat, getSWidth(s), getSHeight(s)), points[0]);
                bool p1In = pointInRect(new Rect(s.lng, s.lat, getSWidth(s), getSHeight(s)), points[1]);
                bool p2In = pointInRect(new Rect(s.lng, s.lat, getSWidth(s), getSHeight(s)), points[2]);
                bool p3In = pointInRect(new Rect(s.lng, s.lat, getSWidth(s), getSHeight(s)), points[3]);
                if (p0In && p1In && p2In && p3In)
                {
                    if (i == centerSquares.Length - 1) i--;
                    lowermostCommonSquare = centerSquares[i];
                    break;
                }
            }

            visibleSquares.Add(lowermostCommonSquare);
            visibleSquares.AddRange(getChildSquares(lowermostCommonSquare, 8));

            /*int i = 1;
            bool commonSquare = (pathes[0][i] == pathes[1][i] && pathes[1][i] == pathes[2][i] && pathes[2][i] == pathes[3][i]);
            while (commonSquare && i < depth)
            {
                commonSquare = pathes[0][i] == pathes[1][i] && pathes[1][i] == pathes[2][i] && pathes[2][i] == pathes[3][i];
                for (int p = 0; p < pathes.Length; p++)
                {

                }
                
                square = square.children[pathes[0][i]];
                visibleSquares.Add(square);                
                i++;
            }*/

                    //visibleSquares = getSquaresInViewRect(square, depth, pathes, chooseMap, points);            

            return visibleSquares;
        }        

        private List<Square> getSquaresInViewRect(Square square, int deepness, int[][] pathes, int[][][] chooseMap, Point[] points)
        {
            if (deepness <= 0) return new List<Square>();
            deepness--;

            List<Square> visibleSquares = new List<Square>();
            int[] indexes = new int[4];
            for (int ii = 0; ii < 4; ii++)
            {
                int[] inds = chooseMap[ii][getChildIndex(square.lat, square.lng, points[ii].Y, points[ii].X, square.level)];
                foreach (int iii in inds)
                    if (!indexes.Contains(iii)) indexes[iii] = iii;
            }

            List<Square> children = getChildSquares(square, indexes);

            foreach (Square s in children)
                visibleSquares.AddRange(getSquaresInViewRect(s, deepness, pathes, chooseMap, points));

            return visibleSquares;
        }

        public int[] getPathToSquare(double lat, double lng, int startLevel = 0)
        {
            int[] path = new int[maxDepth-1];            
            double squareLat = -getSHeight(startLevel) / 2;
            double squareLng = -getSWidth(startLevel) / 2;
            for (int level = startLevel; level < maxDepth - 1; level++)
                path[level] = getChildIndex(ref squareLat, ref squareLng, lat, lng, level);

            return path;
        }     

        public Square[] getSquaresArray(int[] path, bool emulateNullSquares = false)
        {
            List<Square> output = new List<Square>();
            Square s = zeroSquare;
            foreach (int i in path)
                if (s.children[i] != null)
                {
                    output.Add(s);
                    s = s.children[i];
                }
                else if (emulateNullSquares) {
                    for (int ii = i; ii < path.Length; ii++)
                    {
                        output.Add(s);
                        s = calcChild(s, ii);
                    }
                    break;
                }
                else break;

            return output.ToArray();
        }        

        public bool pointInRect(Rect rect, Point p)
        {
            return p.X > rect.X && p.X < rect.X + rect.Width && p.Y > rect.Y && p.Y < rect.Y + rect.Height;
        }

        private int getChildIndex(ref double squareLat, ref double squareLng, double lat, double lng, int level)
        {
            double sHeight = getSHeight(level);
            double sWidth = getSWidth(level); //squareWidth
            double centerLat = squareLat + sHeight / 2;
            double centerLng = squareLng + sWidth / 2;

            //can I replace it??
            int index = 0;
            if (lat >= centerLat)
            {
                if (lng >= centerLng)
                {
                    index = 3;
                    squareLng = centerLng;
                }
                else index = 2;
                squareLat = centerLat;
            }
            else if (lng >= centerLng)
            {
                index = 1;
                squareLng = centerLng;
            }

            return index;
        }

        private int getChildIndex(double squareLat, double squareLng, double lat, double lng, int level)
        {
            double sHeight = getSHeight(level);
            double sWidth = getSWidth(level); //squareWidth
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

            return index;
        }

        private double getSWidth(Square s)
        {
            return 360 / Math.Pow(2, s.level);
        }

        private double getSWidth(int level)
        {
            return 360 / Math.Pow(2, level);
        }

        private double getSHeight(Square s)
        {
            return 180 / Math.Pow(2, s.level);
        }

        private double getSHeight(int level)
        {
            return 180 / Math.Pow(2, level);
        }

        private List<Square> getChildSquares(Square baseSquare, int[] indexes)
        {            
            List<Square> children = new List<Square>();
            foreach (int index in indexes)
                if (baseSquare.children[index] != null) children.Add(baseSquare.children[index]);

            return children;
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
