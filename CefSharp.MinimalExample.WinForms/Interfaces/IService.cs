using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Interfaces
{
    public interface IService
    {
        object GetLoggingInScript();
        object GetRedirectScript();
        object GetLogOutScript();
        object GetRedirectToLoginPageScript();
        object GetActionStack();
        string GetSiteLoginUrl();
        string GetDiscountUrl();
        string GetErrorLoginUrl();
        string GetLoginConfirmationUrl();
        string GetLogoutConfirmationUrl();
        object GetParsedResponse(string html);
        void SaveItemsToCsvFile(object items);
        void CheckForConditionOfBrowser(object e);


    }
}
