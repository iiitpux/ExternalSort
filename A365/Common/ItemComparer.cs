using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A365.Common
{
    public class ItemComparer : IComparer<string>//название получше
    {
        public int Compare(string x, string y)
        {
            var splitX = x.Split(". ");
            var splitY = y.Split(". ");
            var compareResult = string.Compare(splitX[1], splitY[1]);
            if (compareResult == 0)
            {
                return int.Parse(splitX[0]).CompareTo(int.Parse(splitY[0]));
            }
            return compareResult;
        }
    }
}
