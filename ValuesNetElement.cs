﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarsAndPitsWPF
{
    class ValuesNetElement : FrameworkElement
    {
        Size size;
        ValuesNet net;        
        SquareRect[] sRectsCache = new SquareRect[0];        
        Matrix matrix;                
        Canvas canvas;
        Point pressedMouse;

        public int visibleSquaresCount = 0;
        public Point coordinates;

        public ValuesNetElement(Canvas canvas, ValuesNet net)
        {
            this.canvas = canvas;
            this.net = net;                                       

            matrix = new Matrix();
            matrix.Scale(4, -4);
            matrix.Translate(180, 90);

            SizeChanged += ValuesNetElement_SizeChanged;
            canvas.MouseWheel += ValuesNetElement_MouseWheel;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseDown += Canvas_MouseDown;          
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pressedMouse = e.GetPosition(canvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = e.GetPosition(canvas);
            coordinates = mouse;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Vector delta = Point.Subtract(mouse, pressedMouse); // delta from old mouse to current mouse 
                pressedMouse = mouse;
                matrix.Translate(delta.X, delta.Y);                
                e.Handled = true;

                InvalidateVisual();
            }
        }

        private void ValuesNetElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(canvas);

            if (e.Delta > 0)
                matrix.ScaleAt(1.1, 1.1, p.X, p.Y);
            else
                matrix.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y);

            InvalidateVisual();
        }
        

        private void ValuesNetElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            size = e.NewSize;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Point[] edges = getViewEdges();
            bool changed = net.findSquaresInViewRect(net.zeroSquare, new Rect(edges[0], edges[1]));            

            Square[] squaresToRender = net.getCachedSquares();
            if (squaresToRender == null)
            {
                drawRect(new SquareRect(net.zeroSquare, net.averageBottomValue, net.maxDepth), drawingContext);
                foreach (Square square in net.getChildSquares(net.zeroSquare, 2))
                    drawRect(new SquareRect(square, net.averageBottomValue, net.maxDepth), drawingContext);
                return;
            }

            if (changed)
                sRectsCache = generateSRects(squaresToRender);

            visibleSquaresCount = squaresToRender.Length;                        
            foreach (SquareRect sRect in sRectsCache)
                drawRect(sRect, drawingContext);                                    
        }

        private Point getInvertedPoint(Point p)
        {
            Matrix m = matrix;
            m.Invert();
            return m.Transform(p);
        }

        private SquareRect[] generateSRects(Square[] squares)
        {
            SquareRect[] sRects = new SquareRect[squares.Length];           

            for (int i = 0; i < squares.Length; i++)
                sRects[i] = new SquareRect(squares[i], net.averageBottomValue, net.maxDepth);

            return sRects;
        }

        private void drawRect(SquareRect squareRect, DrawingContext context)
        {
            Rect rect = squareRect.rect;
            rect.Transform(matrix);
            context.DrawRectangle(squareRect.brush, squareRect.pen, rect);
        }                             

        private Point[] getViewEdges()
        {
            Matrix invMatrix = matrix;
            invMatrix.Invert();
            Point zeroPoint = invMatrix.Transform(new Point(0, 0));
            Point endPoint = invMatrix.Transform(new Point(canvas.ActualWidth, canvas.ActualHeight));

            return new Point[] { zeroPoint, endPoint };
        }
    }    
}
