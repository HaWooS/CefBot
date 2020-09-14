using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Exceptions
{
    class UiChangedException : Exception
    {
        public UiChangedException() { }

        public UiChangedException(string message)
           : base(String.Format("UI changed exception {0} ", message))
        {

        }
    }
}
