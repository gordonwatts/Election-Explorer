using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionDriver
{
    /// <summary>
    /// Thrown when an election fails for some reason.
    /// </summary>
    public class ElectionFailureException : Exception
    {
        public ElectionFailureException(string message)
            : base (message)
        {
        }

        public ElectionFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
