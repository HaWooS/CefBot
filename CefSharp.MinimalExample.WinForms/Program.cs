// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using CefSharp.MinimalExample.WinForms.Services;
using CefSharp.MinimalExample.WinForms.Utilities;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using static CefSharp.MinimalExample.WinForms.BrowserForm;

namespace CefSharp.MinimalExample.WinForms
{
    public class Program
    {
        public delegate void Del();
        public static BrowserForm browser = null;

        public static void EndFunction()
        {
            Application.ExitThread();
            Application.Exit();
            //Cef.Shutdown();
        }


        [STAThread]
        public static int Main(string[] args)
        {
            //For Windows 7 and above, best to include relevant app.manifest entries as well
            Cef.EnableHighDPISupport();

#if NETCOREAPP
            //We are using our current exe as the BrowserSubProcess
            //Multiple instances will be spawned to handle all the 
            //Chromium proceses, render, gpu, network, plugin, etc.
            var subProcessExe = new CefSharp.BrowserSubprocess.BrowserSubprocessExecutable();
            var result = subProcessExe.Main(args);
            if (result > 0)
            {
                return result;
            }
#endif

            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

#if NETCOREAPP
            //We use our Applications exe as the BrowserSubProcess, multiple copies
            //will be spawned
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            settings.BrowserSubprocessPath = exePath;
#endif
            settings.CefCommandLineArgs.Add("enable-media-stream");
            var mngr = Cef.GetGlobalCookieManager();
            Cookie Ac = new Cookie();
            Ac.Name = "SameSite";
            Ac.Value = "None; Secure";

            Del endFunctionDelegate = new Del(EndFunction);
            var website = CredentialProvider.GetSiteUrl();
        
            if (website != null)
            {
                if (website.Equals("https://suplementy.pl/"))
                {
                    browser = new BrowserForm(endFunctionDelegate, settings, new SuplementyService());
                }
                else if (website.Equals("https://pl.aliexpress.com/"))
                {
                    browser = new BrowserForm(endFunctionDelegate, settings, new AliexpressService());
                }
                else
                {
                    throw new Exception("Website is not supported");
                }
            }
            else
            {
                throw new Exception("Url of website not found");
            }
           

            int firstRunHour = 10;
            int firstRunMinute = 10;
            Scheduler.IntervalInDays(firstRunHour, firstRunMinute, 1, () => {
                Application.Run(browser);
            });
            Console.ReadLine();

          

            return 0;
        }
    }
    
}
