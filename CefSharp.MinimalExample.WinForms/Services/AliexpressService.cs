using CefSharp.MinimalExample.WinForms.Exceptions;
using CefSharp.MinimalExample.WinForms.Interfaces;
using CefSharp.MinimalExample.WinForms.Utilities;
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
    class AliexpressService : IService
    {
        private readonly string siteUrl = "https://pl.aliexpress.com/";

        private readonly string errorLoginUrl = "https://suplementy.pl/return.php?status=account_badlogin";
        private readonly string loginConfirmationUrl = "https://suplementy.pl/login.php";
        private readonly string logoutConfirmationUrl = "https://suplementy.pl/login.php?operation=logout";

        private Stack<object> actions;
        private List<string> listOfItemsOnDiscount { get; set; }

        public AliexpressService()
        {
            this.actions = new Stack<object>();
            //actions.Push(GetLogOutScript());
            actions.Push(GetDownloadDataScript());
            actions.Push(GetLoggingInScript());
            actions.Push(GetRedirectToLoginPageScript());
        }

        public string GetSiteUrl() { return siteUrl; }
        public string GetErrorLoginUrl() { return errorLoginUrl; }
        public string GetLoginConfirmationUrl() { return loginConfirmationUrl; }
        public string GetLogoutConfirmationUrl() { return logoutConfirmationUrl; }
        public object GetActionStack() { return actions; }
        public object GetRedirectToLoginPageScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('rax - image ')[1].click();");
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('sign - btn')[0].click();");
            var redirectScript = CredentialProvider.stringBuilder.ToString();
            return redirectScript;
        }
        public object GetLoggingInScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('rax - image ')[1].click();");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('fm - login - id').value='" + CredentialProvider.GetUsername() + "';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('fm - login - password').value='" + CredentialProvider.GetPassword() + "';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('fm - button fm - submit password - login')[0].click();");
            var loggingInScript = CredentialProvider.stringBuilder.ToString();
            return loggingInScript;
        }

        public object GetLogOutScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("https://login.aliexpress.com/xman/xlogout.htm");
            var redirectScript = CredentialProvider.stringBuilder.ToString();
            return redirectScript;
        }

        public object GetDownloadDataScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('rax - image ')[1].click();");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('search - key').value='Xiaomi redmi 8T'");
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('search - button')[0].click();");
            var downloadDataScript = CredentialProvider.stringBuilder.ToString();
            return downloadDataScript;
        }

        public object GetRedirectScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('rax - image ')[1].click();");
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', '"  + "');");
            var logoutScript = CredentialProvider.stringBuilder.ToString();
            return logoutScript;
        }

        public object GetParsedResponse(string response)
        {
            HtmlAgilityPack.HtmlDocument htmldocument = new HtmlAgilityPack.HtmlDocument();
            htmldocument.LoadHtml(response);
            {
                if (htmldocument.GetElementbyId("root") != null)
                {
                    //get whole html where the id is a search id on the site
                    string[] nodes = Regex.Split(htmldocument.GetElementbyId("search").InnerText, @"\n# $&:\n\t");
                    Console.Write(nodes);
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

        public string GetSiteLoginUrl()
        {
            throw new NotImplementedException();
        }

        public string GetDiscountUrl()
        {
            throw new NotImplementedException();
        }

        public void CheckForConditionOfBrowser(object e)
        {
            throw new NotImplementedException();
        }
    }
}
