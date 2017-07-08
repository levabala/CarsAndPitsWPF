using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarsAndPitsWPF
{
    class ValuesNetManager
    {
        public static void fillValuesNet(
            ValuesNet emptyValuesMap,
            string folder,
            ProgressChangedEventHandler processHandler,
            RunWorkerCompletedEventHandler completeHandler)
        {
            List<string> directories = new List<string>();
            List<string> files = new List<string>();
            foreach (string path in Directory.GetDirectories(folder))
            {
                if (Directory.Exists(path)) directories.Add(path);
                else if (File.Exists(path)) files.Add(path);
            }

            List<CPDataGeo> data = new List<CPDataGeo>();

            Dictionary<SensorType, CPData> CPdata;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, e) =>
            {
                double i = 0;
                foreach (string path in directories)
                {
                    CPdata = CPData.fromDirectory(path);
                    if (CPdata.ContainsKey(SensorType.ACCELEROMETER) && CPdata.ContainsKey(SensorType.GPS))
                        data.Add(new CPDataGeo(CPdata[SensorType.ACCELEROMETER], CPdata[SensorType.GPS]));
                    i++;

                    bw.ReportProgress((int)(i / directories.Count * 100));
                }

                CPdata = CPData.fromDirectory(folder);
                if (CPdata.ContainsKey(SensorType.ACCELEROMETER) && CPdata.ContainsKey(SensorType.GPS))
                    data.Add(new CPDataGeo(CPdata[SensorType.ACCELEROMETER], CPdata[SensorType.GPS]));
            };
            bw.ProgressChanged += processHandler;
            bw.RunWorkerCompleted += delegate
            {
                fillValuesNet(emptyValuesMap, data, processHandler, completeHandler);
            };
            bw.RunWorkerAsync();
        }

        public static void fillValuesNet(
            ValuesNet emptyValuesMap,
            List<CPDataGeo> CPdata,
            ProgressChangedEventHandler processHandler,
            RunWorkerCompletedEventHandler completeHandler)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, e) =>
            {
                double counter = 0;
                double totalCount = 0;
                foreach (CPDataGeo data in CPdata)
                    totalCount += data.geoData.Length;

                Parallel.ForEach(CPdata, (data) =>
                {
                    int part = 0;
                    int triggerPart = data.geoData.Length / 100;
                    foreach (DataTuplyaGeo tuplya in data.geoData)
                    {
                        double value = 0;
                        foreach (double v in tuplya.values)
                            value += Math.Abs(v);
                        emptyValuesMap.putValue(tuplya.coordinate.Latitude, tuplya.coordinate.Longitude, value);
                        counter++;
                        part++;

                        if (part >= triggerPart)
                        {
                            part = 0;
                            bw.ReportProgress((int)(counter / totalCount * 100));
                        }
                    }
                });
            };
            bw.ProgressChanged += processHandler;
            bw.RunWorkerCompleted += completeHandler;
            bw.RunWorkerAsync();
        }

        public static void fillValuesNet(
            ValuesNet emptyValuesMap,
            Random rnd, long valuesCount,
            ProgressChangedEventHandler processHandler,
            RunWorkerCompletedEventHandler completeHandler)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (sender, e) =>
            {
                for (int i = 0; i < valuesCount; i++)
                {
                    Point p = new Point(rnd.NextDouble() * 360 - 180, rnd.NextDouble() * 180 - 90);
                    emptyValuesMap.putValue(p.Y, p.X, 3);
                }
            };
            bw.ProgressChanged += processHandler;
            bw.RunWorkerCompleted += completeHandler;
            bw.RunWorkerAsync();
        }

        public static CPDataSerializable[] getFromCache(string cacheFolder)
        {
            if (!(Directory.Exists(cacheFolder))) return new CPDataSerializable[0];

            string[] files = Directory.GetFiles(cacheFolder);
            CPDataSerializable[] output = new CPDataSerializable[files.Length];

            for (int i = 0; i < files.Length; i++)
                output[i] = CPDataSerializable.deserializeFrom(files[i]);

            return output;
        }

        public static void saveToCache(string cacheFolder, CPDataSerializable[] data)
        {
            //clean
            DirectoryInfo di = new DirectoryInfo(cacheFolder);
            foreach (FileInfo file in di.GetFiles())            
                file.Delete();            

            //fill
            CPDataSerializable.serializeToFolder(cacheFolder, data);
        }
    }    
}
