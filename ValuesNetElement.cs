using System;
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
        ValuesNet net = null;
        Rect viewRect;
        SquareRect[] sRectsCache = new SquareRect[0];
        Square baseViewSquare;
        List<int> levelsToRender;
        int maxRectsCount = 5000;
        Matrix matrix;
        MatrixTransform transform;
        Point pressedMouse;        
        Canvas canvas;
        public int visibleSquaresCount = 0;
        public Point coordinates;

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
            //transform = new MatrixTransform(matrix);

            //RenderTransform = transform;

            SizeChanged += ValuesNetElement_SizeChanged;
            canvas.MouseWheel += ValuesNetElement_MouseWheel;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseUp += Canvas_MouseUp;            
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Invalidate();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pressedMouse = getInvertedPoint(e.GetPosition(canvas));
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = getInvertedPoint(e.GetPosition(canvas));
            coordinates = mouse;
            if (e.LeftButton == MouseButtonState.Pressed)
            {                
                Vector delta = Point.Subtract(mouse, pressedMouse); // delta from old mouse to current mouse
                matrix.Translate(delta.X, delta.Y);                
                //transform = new MatrixTransform(matrix);

                //RenderTransform = transform;
                e.Handled = true;

                InvalidateVisual();
            }
        }

        private Point getInvertedPoint(Point p)
        {
            Matrix m = matrix;
            m.Invert();
            return m.Transform(p);
        }

        private void ValuesNetElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(canvas);
            //double reduceCoeff = matrix.M11 * matrix.M11 / 50000 / 50000;
            //if (reduceCoeff < 1) reduceCoeff = 1;

            if (e.Delta > 0)
                matrix.ScaleAt(1.1, 1.1, p.X, p.Y);
            else
                matrix.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y);

            //RenderTransform = new MatrixTransform(matrix);

            InvalidateVisual();
        }
        

        private void ValuesNetElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            size = e.NewSize;
            InvalidateVisual();
        }

        public void Invalidate()
        {
            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Point[] edges = getViewEdges();
            bool changed = net.findSquaresInViewRect(net.zeroSquare, new Rect(edges[0], edges[1]));            

            Square[] squaresToRender = net.getCachedSquares();
            if (squaresToRender == null)
            {
                drawRect(new SquareRect(net.zeroSquare, net.maxValue), drawingContext);
                foreach (Square square in net.getChildSquares(net.zeroSquare, 2))
                    drawRect(new SquareRect(square, net.maxValue), drawingContext);
                return;
            }

            if (changed)
                sRectsCache = generateSRects(squaresToRender);

            visibleSquaresCount = squaresToRender.Length;                        
            foreach (SquareRect sRect in sRectsCache)
                drawRect(sRect, drawingContext);                                    
        }        

        private SquareRect[] generateSRects(Square[] squares)
        {
            SquareRect[] sRects = new SquareRect[squares.Length];           

            for (int i = 0; i < squares.Length; i++)
                sRects[i] = new SquareRect(squares[i], net.maxValue);

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

            double intesity = ((square.value != 0 && square.level != 0) ? square.value : 1) / maxValue * 255;
            Color color = Color.FromRgb(255, (byte)intesity, (byte)intesity);
            brush = new SolidColorBrush(color);
            brush.Opacity = 0.1;

            pen = new Pen(Brushes.Black, width * 0.003 * Math.Pow(2, square.level));

            path = square.path;
        }
    }
}
