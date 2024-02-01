using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eryph.Packer
{
    internal class EryphPackerUserException : Exception
    {
        public EryphPackerUserException(string message) : base(message)
        {
        }

        public EryphPackerUserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
