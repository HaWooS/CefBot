using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Utilities
{
    public static class CredentialProvider
    {
        //username,password, login url declared in app.config
        private static string username = ConfigurationManager.AppSettings["username"];
        private static string password = ConfigurationManager.AppSettings["password"];
        private static string siteUrl = ConfigurationManager.AppSettings["url"];
        //private static string siteUrl = ConfigurationManager.AppSettings["url1"];
        public static bool IsContentToDownloadAvailable { get; set; }

        public static StringBuilder stringBuilder = new StringBuilder();

        public static string GetUsername() { return username; }
        public static string GetPassword() { return password; }
        public static string GetSiteUrl() { return siteUrl; }

        //public static string GetSite1Url() { return siteUrl; }
    }
}
