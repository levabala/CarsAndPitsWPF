using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarsAndPitsWPF
{
    class ValuesNetGMap : FrameworkElement
    {
        GMapControl mapView;
        ValuesNet net;
        SquareRect[] sRectsCache = new SquareRect[0];

        public int visibleSquaresCount = 0;

        public ValuesNetGMap(ValuesNet net, GMapControl mapView)
        {
            this.net = net;
            this.mapView = mapView;

            setUpMap(mapView);

            //drawRect(new SquareRect(net.zeroSquare, net.maxValue));            

            InvalidateVisual();
        }

        private void setUpMap(GMapControl mapView)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;                        
            mapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 20;            
            mapView.Zoom = 2;            
            mapView.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mapView.ShowCenter = false;            
            mapView.CanDragMap = true;            
            mapView.DragButton = MouseButton.Left;

            mapView.OnMapDrag += delegate
            {
                InvalidateVisual();
            };
            mapView.OnMapZoomChanged += delegate
            {
                InvalidateVisual();                
            };
        }

        private void drawRect(SquareRect sRect, DrawingContext context)
        {

            GPoint[] points = new GPoint[]
            {
                mapView.FromLatLngToLocal(new PointLatLng(sRect.rect.Y + sRect.rect.Height, sRect.rect.X)),                
                mapView.FromLatLngToLocal(new PointLatLng(sRect.rect.Y, sRect.rect.X + sRect.rect.Width))                
            };

            Rect rect = new Rect(points[0].X, points[0].Y, points[1].X - points[0].X, points[1].Y - points[0].Y); ;

            context.DrawRectangle(sRect.brush, sRect.pen, rect);
       }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
                    
            Point[] edges = new Point[]
            {
                new Point(mapView.ViewArea.Lng, mapView.ViewArea.Lat),
                new Point(mapView.ViewArea.Lng + mapView.ViewArea.WidthLng, mapView.ViewArea.Lat + mapView.ViewArea.HeightLat)
            };
            bool changed = net.findSquaresInViewRect(net.zeroSquare, new Rect(edges[0], edges[1]), 1500);

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
    }
}
