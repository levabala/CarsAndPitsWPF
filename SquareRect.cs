using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CarsAndPitsWPF
{
    struct SquareRect
    {
        public Rect rect;
        public Brush brush;
        public Pen pen;
        public int[] path;

        public SquareRect(Square square, double averageBottomValue, int maxDepth)
        {
            double width = 360 / Math.Pow(2, square.level);
            double height = 180 / Math.Pow(2, square.level);
            double left = square.lng;// + 180;
            double top = square.lat;// + 90;
            rect = new Rect(left, top, width, height);

            double intesity =
                ((square.value != 0 && square.level != 0) ? square.value : 1) /
                averageBottomValue / (Math.Pow(3, maxDepth - square.level));

            Color color;
            color = Color.FromRgb(255, (byte)(255 * intesity), 255);

            brush = new SolidColorBrush(color);
            brush.Opacity = 0.1;

            pen = new Pen(Brushes.Black, width * 0.003 * Math.Pow(2, square.level));

            path = square.path;
        }
    }
}
