﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarsAndPitsWPF
{
    class ValuesNetElement : FrameworkElement
    {
        Size size;
        ValuesNet net = null;
        Rect viewRect;
        List<Rect> rectsCache = new List<Rect>();
        Square baseViewSquare;
        List<int> levelsToRender;
        int maxRectsCount = 5000;
        Matrix matrix;
        Canvas canvas;
        public int visibleSquaresCount = 0;

        public ValuesNetElement(Canvas canvas, ValuesNet net)
        {
            this.canvas = canvas;
            this.net = net;            

            viewRect = new Rect(0, 0, 360, 180);
            baseViewSquare = net.zeroSquare;
            levelsToRender = new List<int>() { 0, 1, 2 };

            matrix = new Matrix();
            matrix.Scale(4, 4);
            matrix.Translate(180, 90);

            RenderTransform = new MatrixTransform(matrix);

            SizeChanged += ValuesNetElement_SizeChanged;
            canvas.MouseWheel += ValuesNetElement_MouseWheel;            
        }

        private void ValuesNetElement_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(canvas);
            if (e.Delta > 0)
                matrix.ScaleAt(1.1, 1.1, p.X, p.Y);
            else
                matrix.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y);

            RenderTransform = new MatrixTransform(matrix);

            //getLayersToRenderList();
            //InvalidateVisual();
        }
        

        private void ValuesNetElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            size = e.NewSize;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawRect(new SquareRect(net.zeroSquare, net.maxValue), drawingContext);
            foreach (Square square in net.getChildSquares(net.zeroSquare))
                drawRect(new SquareRect(square, net.maxValue), drawingContext);

            /*SquareRect[] sRects = generateSRects(getVisibleSquares().ToArray());
            foreach (SquareRect sRect in sRects)
                drawRect(sRect, drawingContext);*/

            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.LightBlue, 1), new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight));
        }

        private SquareRect[] generateSRects(Square[] squares)
        {
            SquareRect[] sRects = new SquareRect[squares.Length];
            /*Parallel.For(0, squares.Length, index =>
            {
                sRects[index] = new SquareRect(squares[index], net.maxValue);
            });*/

            for (int i = 0; i < squares.Length; i++)
                sRects[i] = new SquareRect(squares[i], net.maxValue);

            return sRects;
        }

        private void drawRect(SquareRect squareRect, DrawingContext context)
        {                                                                                                  
            context.DrawRectangle(squareRect.brush, squareRect.pen, squareRect.rect);
        }   
        
        private void getLayersToRenderList()
        {
            Matrix invMatrix = matrix;
            invMatrix.Invert();
            Point zeroPoint = invMatrix.Transform(new Point(0, 0));
            Point endPoint = invMatrix.Transform(new Point(canvas.ActualWidth, canvas.ActualHeight));            

            //Square baseSquare = net.getSquare(zeroPoint.Y, zeroPoint.X);
            //Square upperSquare = net.getUpperParent(baseSquare, );

            List<Square> visibleSquares = net.getSquaresInViewRect(net.zeroSquare, new Rect(zeroPoint, endPoint));            
            visibleSquaresCount = visibleSquares.Count;
        }             

        private List<Square> getVisibleSquares()
        {
            Matrix invMatrix = matrix;
            invMatrix.Invert();
            Point zeroPoint = invMatrix.Transform(new Point(0, 0));
            Point endPoint = invMatrix.Transform(new Point(canvas.ActualWidth, canvas.ActualHeight));

            List<Square> visibleSquares = net.getSquaresInViewRect(net.zeroSquare, new Rect(zeroPoint, endPoint));
            visibleSquaresCount = visibleSquares.Count;

            return visibleSquares;
        }
    }

    struct SquareRect
    {
        public Rect rect;
        public Brush brush;
        public Pen pen;
        public int[] path;

        public SquareRect(Square square, double maxValue)
        {
            double width = 360 / Math.Pow(2, square.level);
            double height = 180 / Math.Pow(2, square.level);
            double left = square.lng;// + 180;
            double top = square.lat;// + 90;
            rect = new Rect(left, top, width, height);

            double intesity = 255 - ((square.value != 0 && square.level != 0) ? square.value : 1) / maxValue * 255;
            Color color = Color.FromRgb(255, (byte)intesity, (byte)intesity);
            brush = new SolidColorBrush(color);
            brush.Opacity = 0.1;

            pen = new Pen(Brushes.Black, 1 / Math.Pow(1.8, square.level));

            path = square.path;
        }
    }
}
