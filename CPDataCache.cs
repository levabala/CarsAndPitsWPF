using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarsAndPitsWPF
{
    class CPDataCache
    {
        StringCollection cachedData;
        Dictionary<string, Dictionary<long, SensorType>> cacheTree = new Dictionary<string, Dictionary<long, SensorType>>();

        public bool checkIfCached()
        {
            return false;
        }         
    }
}
