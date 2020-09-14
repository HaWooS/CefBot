// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.Handler;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.MinimalExample.WinForms.Errors;
using CefSharp.MinimalExample.WinForms.Exceptions;
using CefSharp.MinimalExample.WinForms.Interfaces;
using CefSharp.MinimalExample.WinForms.Utilities;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using static CefSharp.MinimalExample.WinForms.Program;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form
    {
        private IService _service;
        public readonly ChromiumWebBrowser browser;
        public int numberOfPrieviousLoads = -1;
        public int currentNumberOfLoads = 0;
        public Stack<object> invokedActions=null;
        public bool areAllActionsInvoked = false;
       

        private EventHandler eventHandler { get; set; }
        private CefSettings settings { get; set; }
        private Del endFunction { get; set; }
        public BrowserForm(Del endFunctionDelegate, CefSettings settings, IService service)
        {
            this.settings = settings;
            this._service = service;
            CefInitialize();
            InitializeComponent();
            //Cef.GetGlobalCookieManager().DeleteCookies("", "");
            Text = "CefSharp";
            WindowState = FormWindowState.Maximized;

            browser = new ChromiumWebBrowser(CredentialProvider.GetSiteUrl());
            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;
            browser.FrameLoadEnd += OnFrameLoadEnded;
            this.eventHandler = new EventHandler(ExitMenuItemClick);
            

            this.endFunction = endFunctionDelegate;

            this.invokedActions = new Stack<object>();
            CredentialProvider.IsContentToDownloadAvailable = false;

            var version = string.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
               Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);
            //LoadUrl(System.Configuration.ConfigurationManager.AppSettings["url"]);

#if NETCOREAPP
            // .NET Core
            var environment = string.Format("Environment: {0}, Runtime: {1}",
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
                System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
#else
            // .NET Framework
            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var environment = String.Format("Environment: {0}", bitness);
            
#endif

            DisplayOutput(string.Format("{0}, {1}", version, environment));
        }


        public void CefInitialize()
        {
            
            this.InvokeOnUiThreadIfRequired(() =>
            {
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            });
      
        }


        private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
        {

            var b = ((ChromiumWebBrowser)sender);

            this.InvokeOnUiThreadIfRequired(() => b.Focus());
           
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
        

            this.InvokeOnUiThreadIfRequired(() =>
            {
                if ((invokedActions.Count==0)&&(areAllActionsInvoked==true))
                {
                    this.Invoke(endFunction);
                }
            }
            );
            

       }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }


        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);
            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
            
            if (!args.IsLoading)
            {
                //get a stack of actions to invoke in specified class
                this.invokedActions = (Stack<object>)_service.GetActionStack();
                if (invokedActions.Count != 0)
                {
                    //each time when frame reloads, pop the method from the stack and execute it
                    var script = (string)invokedActions.Pop();
                    browser.ExecuteScriptAsyncWhenPageLoaded(script);
                    if (script.Equals(_service.GetRedirectScript()))
                    {
                        //if executed script is redirecting browser to subsite with data to download, then set the flag of content avaiability to true
                        CredentialProvider.IsContentToDownloadAvailable = true;
                    }
                }
                else
                {
                    this.areAllActionsInvoked = true;
                    ConsoleMessageEventArgs arg = new ConsoleMessageEventArgs((LogSeverity)1, "1", "2", 1);
                    OnBrowserConsoleMessage(sender, arg);
                }
                
            }
        }

        private void OnFrameLoadEnded(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                _service.CheckForConditionOfBrowser(e);
                //if the flag point to site with discounts, try to download all items on discount
                if (CredentialProvider.IsContentToDownloadAvailable==true)
                {
                    try
                    {
                        browser.GetSourceAsync().ContinueWith(taskHtml =>
                        {
                            var html = taskHtml.Result;
                            var discountItems = (List<string>)_service.GetParsedResponse(html);
                            //check if web document contains some items matching discount conditions, if UI change then throw exception, if not then return list of items on discount
                            if (discountItems.Capacity != 0)
                            {
                                _service.SaveItemsToCsvFile(discountItems);
                                CredentialProvider.IsContentToDownloadAvailable = false;
                            }
                            else
                            {
                                throw new UiChangedException("Site UI changed, the data can not be downloaded");
                            }
                        });

                    }catch(Exception ex)
                    {
                       throw new GetItemsOnDiscountException("Error during downloading items on discount");
                    }

                }
               
            }
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
           
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        public void ExitMenuItemClick(object sender, EventArgs e)
        {
            browser.Dispose();
            Cef.Shutdown();
            Close();
            
            
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {

        }
    }


    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            var headers = request.Headers;

            headers["Set-Cookie"] = "HttpOnly;Secure;SameSite=Strict";
            headers["Content-Type"] = "Application/javascript";
            request.Headers = headers;

            return CefReturnValue.Continue;
        }
    }

    public class CustomRequestHandler : RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler();
        }
    }
}
