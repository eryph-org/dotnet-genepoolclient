using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eryph.GenePool.Packing
{
    public readonly record struct GenePackerProgress(long ProcessedBytes, long TotalBytes);
}
