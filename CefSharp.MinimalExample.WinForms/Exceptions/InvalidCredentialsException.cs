using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Errors
{
    class InvalidCredentialsException : Exception
    {
        public string Name { get; set; }
        public InvalidCredentialsException()
        {
            this.Name = "Invalid Credentials Exception";
        }

        public InvalidCredentialsException(string username, string password) 
            : base(String.Format("Invalid username {0} or password {1}", username, password))
            {
            this.Name = "Invalid Credentials Exception";
        }

        
    }
}
