using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CarsAndPitsWPF
{
    class GMapPro : GMapControl
    {
        /*public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("transform", typeof(Transform), typeof(GMapControl),
            new FrameworkPropertyMetadata(null,
                  FrameworkPropertyMetadataOptions.AffectsRender |
                  FrameworkPropertyMetadataOptions.AffectsParentMeasure));*/
        
        public GMapPro()
        {
                        
        }

        public Transform getVisualTransform()
        {
            GMapPro p = this;
            GPoint p2 = p.FromLatLngToLocal(new PointLatLng(30, 30));
            return VisualTransform;
        }

        /*/// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The transform.</value>
        public Transform transform
        {
            get { return VisualTransform; }            
        }*/
    }
}
