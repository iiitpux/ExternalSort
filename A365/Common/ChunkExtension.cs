using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A365.Common
{
    public static class ChunkExtension
    {
        public static IEnumerable<IEnumerable<TValue>> Chunk<TValue>(
this IEnumerable<TValue> values,
int chunkSize)
        {
            return values
                   .Select((v, i) => new { v, groupIndex = i / chunkSize })
                   .GroupBy(x => x.groupIndex)
                   .Select(g => g.Select(x => x.v));
        }
    }
}
