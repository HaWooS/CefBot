using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Exceptions
{
    class SaveToCsvException : Exception
    {

        public SaveToCsvException() { }

        public SaveToCsvException(string message)
           : base(String.Format("Error while saving to CSV {0} ", message))
        {

        }
    }
}
