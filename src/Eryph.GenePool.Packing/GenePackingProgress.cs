using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eryph.GenePool.Packing
{
    public readonly struct GenePackingProgress
    {
        public (long ProcessedBytes, long TotalBytes)? Compression { get; init; }
        public (long ProcessedBytes, long TotalBytes)? Splitting { get; init; }
    }
}
