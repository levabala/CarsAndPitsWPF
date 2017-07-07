﻿using GMap.NET;
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

        private void drawRect(SquareRect sRect)
        {
            GMapPolygon polygon = new GMapPolygon(new List<PointLatLng>()
            {
                new PointLatLng(sRect.rect.Y, sRect.rect.X),
                new PointLatLng(sRect.rect.Y + sRect.rect.Height, sRect.rect.X),
                new PointLatLng(sRect.rect.Y + sRect.rect.Height, sRect.rect.X + sRect.rect.Width),
                new PointLatLng(sRect.rect.Y, sRect.rect.X + sRect.rect.Width),
            });

            polygon.RegenerateShape(mapView);            

            Path p = polygon.Shape as Path;
            p.Stroke = sRect.pen.Brush;
            p.StrokeThickness = sRect.pen.Thickness;
            p.Fill = sRect.brush;
            p.Opacity = 0.2;

            mapView.Markers.Add(polygon);                        
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);            

            /*Point[] edges = new Point[]
            {
                new Point(mapView.ViewArea.Lng, mapView.ViewArea.Lat),
                new Point(mapView.ViewArea.Lng + mapView.ViewArea.WidthLng, mapView.ViewArea.Lat + mapView.ViewArea.HeightLat)
            };
            bool changed = net.findSquaresInViewRect(net.zeroSquare, new Rect(edges[0], edges[1]), 600);

            Square[] squaresToRender = net.getCachedSquares();
            if (squaresToRender == null)
            {
                mapView.Markers.Clear();
                drawRect(new SquareRect(net.zeroSquare, net.maxValue));
                foreach (Square square in net.getChildSquares(net.zeroSquare, 2))
                    drawRect(new SquareRect(square, net.maxValue));
                return;
            }

            if (changed)
            {
                sRectsCache = generateSRects(squaresToRender);
                mapView.Markers.Clear();

                visibleSquaresCount = squaresToRender.Length;
                foreach (SquareRect sRect in sRectsCache)
                    drawRect(sRect);                
            }*/

            Brush brush = new SolidColorBrush(Colors.Green);
            brush.Opacity = 0.3;
            Point p1 = new Point(0, 0);
            Point p2 = new Point(50, 50);
            p1 = mapView.TranslatePoint(p1, (UIElement)Parent);
            p2 = mapView.TranslatePoint(p2, (UIElement)Parent);
            Matrix m = mapView.RenderTransform.Value;
            Matrix m2 = mapView.LayoutTransform.Value;
            
            drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 1), 
                new Rect(p1, p2));
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
