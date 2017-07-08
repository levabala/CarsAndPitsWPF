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
    class CPDataSerializable
    {
        public void serializeTo(string path, string name)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path + name + ".bin",
                                     FileMode.Create,
                                     FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static CPDataSerializable deserializeFrom(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("MyFile.bin",
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.Read);
            CPDataSerializable obj = (CPDataSerializable)formatter.Deserialize(stream);
            stream.Close();

            return obj;
        }

        public static void serializeToFolder(string folder, CPDataSerializable[] array)
        {
            for (int i = 0; i < array.Length; i++)
                array[i].serializeTo(folder, i.ToString());
        }
    }
}
