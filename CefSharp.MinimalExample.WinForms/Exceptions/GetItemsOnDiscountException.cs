using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Exceptions
{
    class GetItemsOnDiscountException : Exception
    {
        public GetItemsOnDiscountException() { }

        public GetItemsOnDiscountException(string message)
           : base(String.Format("Error during getting items {0} ", message))
        {

        }
    }
}
