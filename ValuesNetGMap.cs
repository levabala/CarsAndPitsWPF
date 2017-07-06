using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CarsAndPitsWPF
{
    class ValuesNetGMap : FrameworkElement
    {
        GMapControl mapView;

        public ValuesNetGMap(ValuesNet net)
        {

        }

        private GMapControl generateMap()
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;            
            GMapControl mapView = new GMapControl();
            mapView.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 17;            
            mapView.Zoom = 2;            
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            mapView.ShowCenter = false;            
            mapView.CanDragMap = true;            
            mapView.DragButton = MouseButton.Left;

            return mapView;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);            
        }
    }
}
