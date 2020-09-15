using CefSharp.MinimalExample.WinForms.Errors;
using CefSharp.MinimalExample.WinForms.Exceptions;
using CefSharp.MinimalExample.WinForms.Interfaces;
using CefSharp.MinimalExample.WinForms.Utilities;
using CefSharp.WinForms;
using HtmlAgilityPack;
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
    class MpcForumService : IService
    {
        private readonly string siteUrl = "https://www.mpcforum.pl/login";
        private readonly string errorLoginUrl = "https://www.mpcforum.pl/login/";

        private Stack<object> actions;
        private List<string> listOfPostsToSave { get; set; }
        private List<string> listOfTitles { get; set; }
        private List<string> listOfLinks { get; set; }

        private Dictionary<string, Exception> forbiddenSubsites;
        public InvalidCredentialsException invalidCredentialsExceltpions { get; }
        public MpcForumService()
        {
            this.actions = new Stack<object>();
            listOfTitles = new List<string>();
            listOfLinks = new List<string>();
            listOfPostsToSave = new List<string>();

            actions.Push(GetLogOutScript());
            actions.Push(GetDownloadDataScript());
            actions.Push(GetRedirectScript());
            actions.Push(GetLoggingInScript());

            //dictionary forbidden subsites for CheckForConditionOfBrowser method that checks the condition of flow in browser
            //dictionary determines which subsites are associated with operations ended due to error and which exception is assigned to each error
            this.forbiddenSubsites = new Dictionary<string, Exception>();
            this.forbiddenSubsites.Add(errorLoginUrl, new InvalidCredentialsException(CredentialProvider.GetUsername(), CredentialProvider.GetPassword()));
        }

        public string GetSiteUrl() { return siteUrl; }

        public object GetActionStack() { return actions; }
        public object GetRedirectToLoginPageScript()
        {
            throw new NotImplementedException();
        }
        public object GetLoggingInScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('button_button--sYDKO details_save--3nDG7')[0].click();");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('auth').value='"+CredentialProvider.GetUsername()+"';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('password').value='"+ CredentialProvider.GetPassword()+"';");
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('elSignIn_submit').click();");
            var loggingInScript = CredentialProvider.stringBuilder.ToString();
            return loggingInScript;
        }

        public object GetLogOutScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementById('elUserLink').click();");
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', 'https://www.mpcforum.pl/logout/?csrfKey=612f5196785dc5edf4216b77a9369cf7')");
            var logOutScript = CredentialProvider.stringBuilder.ToString();
            return logOutScript;
        }

        public object GetDownloadDataScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("document.getElementsByClassName('ipsDataList ipsDataList_reducedSpacing')[0];");
            var downloadDataScript = CredentialProvider.stringBuilder.ToString();
            return downloadDataScript;
        }

        public object GetRedirectScript()
        {
            CredentialProvider.stringBuilder.Clear();
            CredentialProvider.stringBuilder.AppendLine("$(location).attr('href', 'https://www.mpcforum.pl');");
            var logoutScript = CredentialProvider.stringBuilder.ToString();
            return logoutScript;
        }

        public object GetParsedResponse(string response)
        {
            HtmlAgilityPack.HtmlDocument htmldocument = new HtmlAgilityPack.HtmlDocument();
            htmldocument.LoadHtml(response);
           
            {
                Console.Write(htmldocument);
                //iteratre and get every single post link inside of web response content
                foreach (HtmlNode node in htmldocument.DocumentNode.SelectNodes("//div[@class='ipsType_break ipsContained']"))
                {
                int startingIndexOfElementsToCut = node.InnerHtml.IndexOf("title");
                string link = node.InnerHtml.Substring(0, startingIndexOfElementsToCut).Replace(" ", "").Replace("<ahref=", "").Replace('"', ' ').Replace('"', ' ');
                listOfTitles.Add(link);
                }
                //iteratre and get every single title of posts inside of web response content
                foreach (HtmlNode node in htmldocument.DocumentNode.SelectNodes("//a[@class='ipsDataItem_title']"))
                {
                    string title = node.InnerHtml.Replace(" ", "");
                    listOfLinks.Add(title);
                }

                //prepare list of elements to add to csv file that contains post title and link
                for(int i=0; i< listOfLinks.Count; i++)
                {
                    listOfPostsToSave.Add(listOfTitles[i] + "   " + listOfLinks[i]);
                }
                    return listOfPostsToSave;
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
                File.AppendAllText("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/posts.csv", csvFile.ToString());

            }
            catch (Exception e)
            {
                throw new SaveToCsvException("Error during to save posts to csv file");
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

        public void CheckForConditionOfBrowser(object e, object browser)
        {
            var webBrowser = (ChromiumWebBrowser)browser;
            if(webBrowser.Address.Equals("https://www.mpcforum.pl/login/"))
            {
                foreach (KeyValuePair<string, Exception> subsiteLink in forbiddenSubsites)
                {
                    //if url address points to address in list of forbidden subsites, then check for error and throw new exception
                        var excep = (InvalidCredentialsException)subsiteLink.Value;
                        throw new Exception(excep.Name);
                }
            }

        }

        public string GetErrorLoginUrl()
        {
            throw new NotImplementedException();
        }

        public string GetLoginConfirmationUrl()
        {
            throw new NotImplementedException();
        }

        public string GetLogoutConfirmationUrl()
        {
            throw new NotImplementedException();
        }

        public bool IsNeedToReload()
        {
            return true;
        }
    }
}
