using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CarsAndPitsWPF
{
    [Serializable]
    class CPData : CPDataSerializable
    {
        public long startTime;
        public string deviceId;
        public DataTuplya[] data;
        public SensorType sensor;

        public CPData(long startTime, string deviceId, DataTuplya[] data, SensorType sensor)
        {
            this.startTime = startTime;
            this.deviceId = deviceId;
            this.data = data;
            this.sensor = sensor;
        }

        public CPData(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            SensorType type = SensorType.fromString(Path.GetFileNameWithoutExtension(path));
            if (type == SensorType.UNKNOWN)
                throw new FileFormatException("Filename isn't valid!");
            sensor = type;

            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 3)
            {
                this.data = new DataTuplya[0];
                return;
            }

            DataTuplya[] data = new DataTuplya[lines.Length - 3];
            startTime = long.Parse(lines[0].Split(' ')[2]);
            deviceId = lines[1].Split(' ')[2];

            Parallel.For(2, lines.Length-1, (i) =>
            {
                String[] str = lines[i].Split('\t');
                int time = int.Parse(str[0]);
                List<double> values = new List<double>();
                for (int ii = 1; ii < str.Length; ii++)
                {
                    str[ii] = str[ii].Replace("\t", "").Replace(",", ".");
                    if (str[ii].Length > 1) values.Add(double.Parse(str[ii]));
                }
                data[i - 2] = new DataTuplya(time, values.ToArray());
            });                        
            
            this.data = data;
        }    
        
        public static Dictionary<SensorType, CPData> fromDirectory(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("No such directory!");

            Dictionary<SensorType, CPData> data = new Dictionary<SensorType, CPData>();
            string[] files = Directory.GetFiles(path);
            foreach (string p in files)
            {
                CPData cpd = new CPData(p);
                data.Add(cpd.sensor, cpd);
            }

            return data;
        }            
    }

    struct DataTuplya
    {
        public int timeOffset;
        public double[] values;

        public DataTuplya(int timeOffset, double[] values)
        {
            this.timeOffset = timeOffset;
            this.values = values;
        }
    }

    public sealed class SensorType
    {
        private readonly string name;

        public static readonly SensorType ACCELEROMETER = new SensorType("ACCELEROMETER");
        public static readonly SensorType MAGNETIC_FIELD = new SensorType("MAGNETIC_FIELD");
        public static readonly SensorType GYROSCOPE = new SensorType("GYROSCOPE");
        public static readonly SensorType GRAVITY = new SensorType("GRAVITY");
        public static readonly SensorType AMBIENT_TEMPERATURE = new SensorType("AMBIENT_TEMPERATURE");
        public static readonly SensorType LIGHT = new SensorType("LIGHT");
        public static readonly SensorType LINEAR_ACCELERATION = new SensorType("LINEAR_ACCELERATION");
        public static readonly SensorType ORIENTATION = new SensorType("ORIENTATION");
        public static readonly SensorType PRESSURE = new SensorType("PRESSURE");
        public static readonly SensorType PROXIMITY = new SensorType("PROXIMITY");
        public static readonly SensorType RELATIVE_HUMIDITY = new SensorType("RELATIVE_HUMIDITY");
        public static readonly SensorType ROTATION_VECTOR = new SensorType("ROTATION_VECTOR");
        public static readonly SensorType TEMPERATURE = new SensorType("TEMPERATURE");
        public static readonly SensorType GAME_ROTATION_VECTOR = new SensorType("GAME_ROTATION_VECTOR");
        public static readonly SensorType GEOMAGNETIC_ROTATION_VECTOR = new SensorType("GEOMAGNETIC_ROTATION_VECTOR");
        public static readonly SensorType HEART_BEAT = new SensorType("HEART_BEAT");
        public static readonly SensorType HEART_RATE = new SensorType("HEART_RATE");
        public static readonly SensorType SIGNIFICANT_MOTION = new SensorType("SIGNIFICANT_MOTION");
        public static readonly SensorType STATIONARY_DETECT = new SensorType("STATIONARY_DETECT");
        public static readonly SensorType STEP_COUNTER = new SensorType("STEP_COUNTER");
        public static readonly SensorType STEP_DETECTOR = new SensorType("STEP_DETECTOR");
        public static readonly SensorType GPS = new SensorType("GPS");
        public static readonly SensorType UNKNOWN = new SensorType("UNKNOWN");
        ///YYYEEAH        

        private static SensorType[] array = new SensorType[]
        {
            ACCELEROMETER,MAGNETIC_FIELD,GYROSCOPE,GRAVITY,AMBIENT_TEMPERATURE,LIGHT,LINEAR_ACCELERATION,ORIENTATION,PRESSURE,
            PROXIMITY,RELATIVE_HUMIDITY,ROTATION_VECTOR,TEMPERATURE,GAME_ROTATION_VECTOR,GEOMAGNETIC_ROTATION_VECTOR,HEART_BEAT,
            HEART_RATE,SIGNIFICANT_MOTION,STATIONARY_DETECT,STEP_COUNTER,STEP_DETECTOR,GPS,UNKNOWN
        };

        private SensorType(string name)
        {
            this.name = name;
        }

        public static SensorType fromString(string str)
        {
            foreach (SensorType s in array)
                if (str == s.name) return s;
            return UNKNOWN;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
