using CefSharp.MinimalExample.WinForms.Errors;
using CefSharp.MinimalExample.WinForms.Exceptions;
using CefSharp.MinimalExample.WinForms.Interfaces;
using CefSharp.MinimalExample.WinForms.Utilities;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms.Services
{
    class SuplementyService : IService
    {
        private readonly string siteLoginUrl = "https://suplementy.pl/signin.php";
        private readonly string discountUrl = "https://suplementy.pl/Wyprzedaz-przeceny-sdiscount-pol.html";
        private readonly string errorLoginUrl = "https://suplementy.pl/return.php?status=account_badlogin";
        private readonly string loginConfirmationUrl = "https://suplementy.pl/login.php";
        private readonly string logoutConfirmationUrl = "https://suplementy.pl/login.php?operation=logout";
        private Stack<object> actions;
        private List<string> listOfItemsOnDiscount;

        private Dictionary<string, Exception> forbiddenSubsites;

        public InvalidCredentialsException invalidCredentialsExceltpions { get; }

        public SuplementyService()
        {
            this.actions = new Stack<object>();
            actions.Push(GetLogOutScript());
            actions.Push(GetRedirectScript());
            actions.Push(GetLoggingInScript());
            actions.Push(GetRedirectToLoginPageScript());

            //dictionary forbidden subsites for CheckForConditionOfBrowser method that checks the condition of flow in browser
            //dictionary determines which subsites are associated with operations ended due to error and which exception is assigned to each error
            this.forbiddenSubsites = new Dictionary<string, Exception>();
            this.forbiddenSubsites.Add(errorLoginUrl, new InvalidCredentialsException(CredentialProvider.GetUsername(),CredentialProvider.GetPassword()));
        }

        public string GetSiteLoginUrl() { return siteLoginUrl; }
        public string GetDiscountUrl() { return discountUrl; }
        public string GetErrorLoginUrl() { return errorLoginUrl; }
        public string GetLoginConfirmationUrl() { return loginConfirmationUrl; }
        public string GetLogoutConfirmationUrl() { return logoutConfirmationUrl; }
        public object GetActionStack() { return actions; }
        public object GetRedirectToLoginPageScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', '" + GetSiteLoginUrl() + "');");
            var redirectScript = CredentialProvider.stringBuilder.ToString();
            return redirectScript;
        }
        public object GetLoggingInScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('user_login').value = '" + CredentialProvider.GetUsername() + "';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('user_pass').value='" + CredentialProvider.GetPassword() + "';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('btn signin_button')[0].click();");
            var loggingInScript = CredentialProvider.stringBuilder.ToString();
            return loggingInScript;
        }

        public object GetLogOutScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', '" + GetLogoutConfirmationUrl() + "');");
            
            var redirectScript = CredentialProvider.stringBuilder.ToString();
            return redirectScript;
        }

        public object GetRedirectScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', '" + GetDiscountUrl() + "');");
            var logoutScript = CredentialProvider.stringBuilder.ToString();
            return logoutScript;
        }

        public object GetParsedResponse(string response)
        {
            HtmlAgilityPack.HtmlDocument htmldocument = new HtmlAgilityPack.HtmlDocument();
            htmldocument.LoadHtml(response);
            {
                if (htmldocument.GetElementbyId("search") != null)
                {
                    //get whole html where the id is a search id on the site
                    string[] nodes = Regex.Split(htmldocument.GetElementbyId("search").InnerText, @"\n# $&:\n\t");
                    RegexOptions options = RegexOptions.None;
                    //create regex and remove all unnecesarry data from retrieved html document
                    Regex regex = new Regex("[ ]{2,}", options);
                    string nodess = regex.Replace(nodes[0], " ").Replace("Przecena", " ").Replace("Dodaj do porównania", " ").Replace("Nowość", " ").Replace("  \n  \n ", "\n").Replace(nodes[0], " ");
                    //create list which will store items downloaded from website

                    //add each element to list after signature \n
                    listOfItemsOnDiscount = nodess.Split('\n').ToList();
                    return listOfItemsOnDiscount;
                }
                else return listOfItemsOnDiscount;
            }

        }

        public void SaveItemsToCsvFile(object items)
        {
            try
            {
                //create representation of sentence which will be saved in csv file
                DateTime localDate = DateTime.Now;
                var csvFile = new System.Text.StringBuilder();
                csvFile.AppendLine(" ");
                csvFile.AppendLine("Data pobrania danych " + localDate.ToString());
                var listOfItemsToSave = (List<string>)items;

                foreach (string item in listOfItemsToSave)
                {
                    var sentence = item;
                    var newLine = string.Format("{0}", sentence.ToString());
                    csvFile.AppendLine(newLine);
                }
                //create an csv document or write to existing document representation of above data
                File.AppendAllText("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/items.csv", csvFile.ToString());

            }
            catch (Exception e)
            {
                throw new SaveToCsvException("Error during to save items to csv file");
            }
        }

        public void CheckForConditionOfBrowser(object e, object browser)
        {
            var browserEvent = (FrameLoadEndEventArgs)e;
            foreach(KeyValuePair<string,Exception> subsiteLink in forbiddenSubsites)
            {
                //if url address points to address in list of forbidden subsites, then check for error and throw new exception
                if(browserEvent.Browser.MainFrame.Url.Equals(subsiteLink.Key))
                {
                    var excep = (InvalidCredentialsException)subsiteLink.Value;
                    throw new Exception(excep.Name);
                }
            }
            
        }

        public bool IsNeedToReload()
        {
            return false;
        }
    }
}
