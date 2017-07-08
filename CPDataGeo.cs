using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarsAndPitsWPF
{
    class CPDataGeo : CPDataSerializable
    {
        public long startTime;
        public string deviceId;        
        public SensorType sensor;
        public DataTuplyaGeo[] geoData;

        public CPDataGeo(CPData sensorCPData, CPData geoCPData)            
        {
            startTime = sensorCPData.startTime;
            deviceId = sensorCPData.deviceId;

            List<DataTuplyaGeo> geoData = new List<DataTuplyaGeo>();            
            int sensorIndex = 0;
            for (int i = 1; i < geoCPData.data.Length; i++)
            {
                while(sensorCPData.data[sensorIndex].timeOffset <= geoCPData.data[i].timeOffset)
                {
                    double sLat = geoCPData.data[i - 1].values[0];
                    int sTime = geoCPData.data[i - 1].timeOffset;
                    double eLat = geoCPData.data[i].values[0];
                    int eTime = geoCPData.data[i].timeOffset;                
                    int nowTime = sensorCPData.data[sensorIndex].timeOffset;
                    double nowLat = lineIntr(sTime, eTime, sLat, eLat, nowTime);

                    double sLng = geoCPData.data[i - 1].values[1];                    
                    double eLng = geoCPData.data[i].values[1];                    
                    double nowLng = lineIntr(sTime, eTime, sLng, eLng, nowTime);

                    geoData.Add(new DataTuplyaGeo(nowTime, sensorCPData.data[sensorIndex].values, new GeoCoordinate(nowLat, nowLng)));

                    sensorIndex++;
                }
            }

            this.geoData = geoData.ToArray();
        }

        private double lineIntr(double x1, double x2, double y1, double y2, double x)
        {
            return y1 + (x - x1) * (y2 - y1) / (x2 - x1);
        }
    }

    struct DataTuplyaGeo
    {
        public int timeOffset;
        public double[] values;
        public GeoCoordinate coordinate;

        public DataTuplyaGeo(int timeOffset, double[] values, GeoCoordinate coordinate)
        {
            this.timeOffset = timeOffset;
            this.values = values;
            this.coordinate = coordinate;
        }
    }
}
