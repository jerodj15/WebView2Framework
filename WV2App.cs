using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace WV2Framework
{
    public partial class WV2App : UserControl
    {
        public WebView2 webView2;
        public delegate Task GetTwoInputStringHandler(string inOne, string inTwo);
        public delegate String GetTwoInputStringHandlerOUT(string inOne, string inTwo);
        public delegate Task<string> GetZeroInputStringHandlerOUT();
        public delegate Task<string> GetOneInputStringHandlerOUT(string inOne);
        public delegate Task GetFourInputStringHandler(string inOne, string inTwo, string inThree, string inFour);
        public delegate Task GetThreeInputStringHandler(string inOne, string inTwo, string inThree);
        public delegate Task GetOneStringAndTwoInputIntHandler(string inOne, int inTwo, int inThree);
        public delegate Task GetTwoStringOneIntInputHandler(string inOne, string inTwo, int inThree);
        public delegate Task GetOneInputStringHandler(string inOne);
        public delegate Task GetOneStringAndOneIntHandler(string inOne, int inTwo);
        public delegate Task GetThreeStringOneIntInputHandler(string inOne, string inTwo, string inThree, int inFour);
        public delegate Task GetZeroInputStringHandler();
        public delegate Task<bool> GetZeroReturnOneBoolHandler();
        CoreWebView2CreationProperties creationProperties = null;
        CoreWebView2Environment environment;
        CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();

        private string initialUrl;
        private string applicationName;
        private string applicationFolder;
        public string returnValue = String.Empty;

        private bool applicationReady = false;
        private bool isNavigating = false;
        private bool clickableFound;
        private bool isGlobalLogon = false;
        private bool isLoggedIn = false;
        private bool elementExists = false;

        private int windowWidth;
        private int windowHeight;
        private int taskCounter = 0;
        private int checkWV2Counter = 0;
        private int populateDelay = 500;
        private int clickDelay = 500;
        private int extractDelay = 1000;
        private int taskRetries = 20;

        private List<Task> listOfTasks = new List<Task>();
        public List<String> resultsList = new List<string>();
        string runtimePath = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Microsoft.WebView2.FixedVersionRuntime.87.0.664.55.x64";


        public WV2App(string applicationName, string initialUrl)
        {
            InitializeComponent();
            this.initialUrl = initialUrl;
            this.applicationName = applicationName;
            applicationFolder = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\WebView2Configurations\\" + applicationName;
            Prerequisities();
        }

        #region Prerequisites
        async private Task Prerequisities()
        {
            //await CheckForEdgeWebView2();
            await CreateApplicationData();
            await SetupApplication();
        }

        async private Task CheckForEdgeWebView2()
        {
            if (Directory.Exists(runtimePath))
            {
                Console.WriteLine("WV2 is ready");
            }
            else
            {
                Console.WriteLine("WV2 is missing");
            }
            //HKEY_LOCAL_MACHINE\\
            /*string regKey = "SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(regKey);
            if (key != null)
            {
                Console.WriteLine("Webview2 is installed");
            }
            else
            {
                Console.WriteLine("Webview2 is not present");
                String installerFile = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Installer\\MicrosoftEdgeWebview2Setup.exe";
                // Begin install
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = installerFile;
                info.Arguments = "/install";
                Console.WriteLine("Starting installer: " + installerFile);
                Process startInstaller = Process.Start(info);
                startInstaller.WaitForExit();
                CheckForEdgeWebView2();
            }*/
        }

        async private Task CreateApplicationData()
        {
            Console.WriteLine("Creating the folder: " + applicationFolder);
            if (!Directory.Exists(Path.GetDirectoryName(applicationFolder)))
            {
                Directory.CreateDirectory(applicationFolder);
            }
        }
        #endregion
        #region Setup Application
        async private Task SetupApplication()
        {
            environment = CoreWebView2Environment.CreateAsync(null, applicationFolder, options).GetAwaiter().GetResult();
            await SetupWebPage();
        }
        async private Task SetupWebPage()
        {
            webView2 = new WebView2();
            await webView2.EnsureCoreWebView2Async(environment);
            this.webView2.Dock = DockStyle.Fill;
            this.webView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            this.Controls.Add(webView2);
            webView2.NavigationCompleted += WebView2_NavigationCompleted;
            webView2.NavigationStarting += WebView2_NavigationStarting;
            webView2.SizeChanged += WebView2_SizeChanged;
            webView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView2.CoreWebView2.FrameNavigationCompleted += CoreWebView2_FrameNavigationCompleted;
            await InitialNavigation();
        }

        async private void CoreWebView2_FrameNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                await Task.Delay(1000);
                await ExecuteJavascript("document.getElementById('cwc_2').getElementsByTagName('div')[3].setAttribute('style','width=0;height=0');");

            }
            catch (Exception Ex)
            {
                Console.WriteLine("Exception ExecuteExceptionManagerRefresh: " + Ex);
            }
        }

        async private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            /*            e.Handled = true;
                        await Task.Delay(2000);
                        await NavigateToNew(e.Uri);
                        await Task.Delay(1000);
                        await e.NewWindow.ExecuteScriptAsync("window.close();");
                        await e.NewWindow.CallDevToolsProtocolMethodAsync("Browser.close", "{}");*/
        }

        async private Task InitialNavigation()
        {
            if (InvokeRequired)
            {
                Invoke(new GetOneInputStringHandler(NavigateToNewAsync), new object[] { this.initialUrl });
            }
            else
            {
                await NavigateToNewAsync(this.initialUrl);
            }

        }




        #endregion
        #region Event Handlers
        private void WebView2_SizeChanged(object sender, EventArgs e)
        {
            this.windowWidth = webView2.Size.Width;
            this.windowHeight = webView2.Size.Height;
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            this.isNavigating = true;
        }

        async public void ExecuteExceptionManagerRefresh()
        {
            await Task.Delay(1500);
            this.ExecuteJavascript("document.getElementById('cwc_2').getElementsByTagName('div')[3].setAttribute('style','width=0;height=0');");
        }

        private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.isNavigating = false;
            if (this.applicationName.Equals("ExceptionManager"))
            {
                ExecuteExceptionManagerRefresh();
            }
        }
        #endregion
        #region Setters and Getters
        async public Task SetReturnValue(string returnValue)
        {
            this.returnValue = returnValue;
            resultsList.Add(returnValue);
            Console.WriteLine("SetReturnValue");
        }
        async public Task<string> GetReturnValue()
        {
            return this.returnValue;
        }
        public void SetIsGlobalLogon(bool isGlo)
        {
            if (isGlo)
            {
                this.isGlobalLogon = true;
            }
        }
        public WebView2 GetWebView2()
        {
            return this.webView2;
        }
        public CoreWebView2 GetCoreWebView2()
        {
            return this.webView2.CoreWebView2;
        }
        #endregion
        #region Public Async Methods
        async public Task NavigateToNew(string url)
        {
            await Task.Delay(2000);
            Console.WriteLine("Nav2New: " + isNavigating + " " + this.taskCounter);
            while (true)
            {
                if (this.isNavigating && this.taskCounter <= 10)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Page is still navigating, trying again");
                    taskCounter++;
                    if (taskCounter >= 10)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of NavigateToNew: " + url);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    Task task = Task.Run(() => NavigateToNewAsync(url));
                    await Task.WhenAll(task);
                    break;
                }
            }
        }
        async public Task PopulateTextAreaByID(string elementID, string value)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByID(elementID);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in PopulateTextAreaByIDAsync");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of PopulateTextAreaByIDAsync: " + elementID);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await PopulateTextAreaByIDAsync(elementID, value);
                    break;
                }
            }
        }
        async public Task PopulateTextAreaByName(string elementName, string value, int elementNumber = 0)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByName(elementName, elementNumber);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in PopulateTextAreaByNameAsync");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of PopulateTextAreaByNameAsync: " + elementName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await PopulateTextAreaByNameAsyncPrivate(elementName, value, elementNumber);
                    break;
                }
            }
        }
        async public Task SetValueofElementByName(string elementName, string value, int elementNumber = 0)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByName(elementName, elementNumber);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in SetValueofElementByName");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of SetValueofElementByName: " + elementName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await SetValueofElementByNamePrivate(elementName, value, elementNumber);
                    break;
                }
            }
        }
        async public Task SetValueInIFRAMEByName(string iframeId, string elementName, string value, int elementNumber = 0)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByID(iframeId);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in SetValueofElementByName");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of SetValueofElementByName: " + elementName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await SetValueInIFRAMEByNameAsync(iframeId, elementName, value, elementNumber);
                    break;
                }
            }
        }
        async public Task PopulateTextAreaInIFRAMEByName(string iframeId, string elementName, string value)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByID(iframeId);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in PopulateTextAreaInIFRAMEByNameAsync");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of PopulateTextAreaInIFRAMEByNameAsync: " + elementName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await PopulateTextAreaInIFRAMEByNameAsyncPrivate(iframeId, elementName, value);
                    break;
                }
            }
        }
        async public Task PopulateTextAreaByClassName(string className, string value, int itemNumber = 0)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByClassName(className, itemNumber);
                if ((this.isNavigating && taskCounter <= 10 || !this.applicationReady) && !isIframeFound)
                {
                    await Task.Delay(populateDelay);
                    Console.WriteLine("Trying again in PopulateTextAreaByClassName");
                    taskCounter++;
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of PopulateTextAreaByClassName: " + className);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await PopulateTextAreaByClassNameAsync(className, value, itemNumber);
                    break;
                }
            }
        }
        async public Task ClickItemByID(string elementID)
        {
            clickableFound = false;
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(clickDelay);
                    Console.WriteLine("Trying again in ClickItemByID: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;
                    await GetItemByIdAsync(elementID);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of ClickItemByID: " + elementID);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await ClickItemByIDAsync(elementID);
                    clickableFound = false;
                    await Task.Delay(500);
                    break;
                }
            }
        }
        async public Task ClickItemByName(string elementName, int elementNumber = 0)
        {
            clickableFound = false;
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(clickDelay);
                    Console.WriteLine("Trying again in ClickItemByName: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;
                    await GetItemByNameAsync(elementName);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of ClickItemByName: " + elementName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await ClickItemByNameAsync(elementName, elementNumber);
                    clickableFound = false;
                    await Task.Delay(500);
                    break;
                }
            }
        }
        async public Task ClickItemByClassName(string elementClassName, int elementNumber = 0)
        {
            clickableFound = false;
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(clickDelay);
                    Console.WriteLine("Trying again in ClickItemByClassName: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;
                    await GetItemByClassNameAsync(elementClassName, elementNumber);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of ClickItemByClassName: " + elementClassName);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await ClickItemByClassNameAsync(elementClassName, elementNumber);
                    clickableFound = false;
                    await Task.Delay(500);
                    break;
                }
            }
        }
        async public Task ClickItemInIFRAMEByName(string iFrameId, string elementName)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByID(iFrameId);
                if ((this.isNavigating && taskCounter <= 10) && !isIframeFound)
                {
                    await Task.Delay(clickDelay);
                    Console.WriteLine("Trying again in ClickItemInIFRAMEByName: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;

                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of ClickItemInIFRAMEByName: " + iFrameId);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await ClickItemInIFRAMEByNamePrivate(iFrameId, elementName);
                    await Task.Delay(500);
                    break;
                }
            }
        }
        async public Task ClickItemByInputTypeAndID(string inputType, string elementID)
        {
            bool isIframeFound;
            while (true)
            {
                isIframeFound = await CheckIfElementExistsByID(elementID);
                if ((this.isNavigating && taskCounter <= 10) && !isIframeFound)
                {
                    await Task.Delay(2000);
                    Console.WriteLine("Trying again in ClickItemByInputTypeAndID: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;

                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of ClickItemByInputTypeAndID: " + elementID);
                        break;
                    }
                }
                else
                {
                    await Task.Delay(1000);
                    taskCounter = 0;
                    await Task.Run(() => ClickItemByInputTypeAndIDAsync(inputType, elementID));
                    await Task.Delay(1000);
                    break;
                }
            }
        }
        async public Task<string> GetInfoByID(string elementId, string attribute)
        {
            string result = String.Empty;
            clickableFound = false;
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Trying again in GetInfoByID: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;
                    await GetItemByIdAsync(elementId);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of GetInfoByID: " + elementId);
                        result = "Item Not Found";
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    result = await GetInfoByIDAsync(elementId, attribute);
                    await Task.Delay(1000);
                    clickableFound = false;
                    break;
                }
            }
            return result;
        }
        async public Task<string> GetInfoByName(string elementName, string attribute)
        {
            clickableFound = false;
            string result = String.Empty;
            // await Task.Delay(1000);
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Trying again in GetInfoByName: isNAV=" + isNavigating + ",taskCounter=" + taskCounter);
                    taskCounter++;
                    await GetItemByNameAsync(elementName);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of GetInfoByName: " + elementName);
                        result = "Item Not Found";
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    result = await GetInfoByNameAsync(elementName, attribute);
                    await Task.Delay(1000);
                    clickableFound = false;
                    break;
                }
            }
            return result;
        }
        async public Task GetInformationInIFRAMEByName(string iFrameId, string elementName, string attribute)
        {
            clickableFound = false;
            // await Task.Delay(1000);
            while (true)
            {
                if ((this.isNavigating || clickableFound == false) && taskCounter <= 10)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Trying again in GetInformationInIFRAMEByName: isNAV=" + isNavigating + ",taskCounter=" + taskCounter + ", clickableFound=" + clickableFound);
                    taskCounter++;
                    await GetIFRAMEItemByNameASync(iFrameId, elementName);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of GetInformationInIFRAMEByName: " + elementName);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Executing GetInformationInIFRAMEByName: " + "isNAV=" + isNavigating + ",taskCounter=" + taskCounter + ", clickableFound=" + clickableFound);
                    taskCounter = 0;
                    await Task.Run(() => GetInformationInIFRAMEByNameAsync(iFrameId, elementName, attribute));
                    await Task.Delay(1000);
                    clickableFound = false;
                    break;
                }
            }
        }
        async public Task GetInformationInIFRAMEByID(string iFrameId, string elementId, string attribute)
        {
            clickableFound = false;
            // await Task.Delay(1000);
            while (true)
            {
                if (this.isNavigating && taskCounter <= 10 || clickableFound == false)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Trying again in GetInformationInIFRAMEByID: isNAV=" + isNavigating + ",taskCounter=" + taskCounter + ", clickableFound=" + clickableFound);
                    taskCounter++;
                    await GetIFRAMEItemByIDASync(iFrameId, elementId);
                    if (taskCounter >= taskRetries)
                    {
                        taskCounter = 0;
                        Console.WriteLine("Broke out of GetInformationInIFRAMEByID: " + elementId);
                        break;
                    }
                }
                else
                {
                    taskCounter = 0;
                    await Task.Run(() => GetInformationInIFRAMEByIDAsync(iFrameId, elementId, attribute));
                    await Task.Delay(1000);
                    break;
                }
            }
        }
        async public Task<bool> CheckLoginStatus()
        {
            this.isLoggedIn = false;
            if (isGlobalLogon)
            {
                try
                {
                    string checkString = "Log on";
                    await this.GetInfoByID("GloPasswordSubmit", "value");
                    foreach (String res in this.resultsList)
                    {
                        if (res.Contains(checkString))
                        {

                            this.isLoggedIn = true;
                            this.resultsList.Clear();
                            break;
                        }
                        else
                        {
                            this.isLoggedIn = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception CheckLoginStatus: " + ex);
                }
                return isLoggedIn;
            }
            else
            {
                return false;
            }
        }
        async public Task<bool> CheckLoginStatus(string existingElementID)
        {
            try
            {
                await CheckIfElementExistsByID(existingElementID);
                if (this.elementExists)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckLoginStatus(" + existingElementID + "): " + ex);
                return false;
            }
        }
        async public Task<string> ExecuteJavascript(string scriptIn)
        {
            string result = string.Empty;
            await Task.Delay(1000);
            if (InvokeRequired)
            {
                Invoke(new GetOneInputStringHandler(ExecuteJavascript), new object[] { scriptIn });
            }
            else
            {
                result = await webView2.ExecuteScriptAsync(scriptIn);

            }
            return result;
        }
        async public Task<bool> IsPageNull()
        {
            bool elementExists = false;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetZeroReturnOneBoolHandler(IsPageNull), new object[] { });
                }
                else
                {
                    string preScript = "document.body.outerHTML";
                    Console.WriteLine("Prescript GetInfoByID: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        elementExists = true;

                    }
                    else if (x.Equals("null"))
                    {
                        elementExists = true;
                    }
                    else
                    {
                        elementExists = false;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
                return elementExists;
                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByID: " + ex);
                return false;
            }
        }
        async public Task<string> GetPageBody()
        {
            await Task.Delay(1000);
            string pageBody = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetZeroInputStringHandlerOUT(GetPageBody), new object[] { });
                }
                else
                {
                    string preScript = "document.body";
                    Console.WriteLine("Prescript GetInfoByID: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        pageBody = null;

                    }
                    else if (x.Equals("null"))
                    {
                        pageBody = null;
                    }
                    else
                    {
                        pageBody = x;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
                return pageBody;
                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByID: " + ex);
                return "null";
            }
        }
        async public Task<string> GetPageBodyInnerHTML()
        {
            await Task.Delay(1000);
            string pageBody = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetZeroInputStringHandlerOUT(GetPageBodyInnerHTML), new object[] { });
                }
                else
                {
                    string preScript = "document.body.innerHTML";
                    Console.WriteLine("Prescript GetInfoByID: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        pageBody = null;

                    }
                    else if (x.Equals("null"))
                    {
                        pageBody = null;
                    }
                    else
                    {
                        pageBody = x;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
                return pageBody;
                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByID: " + ex);
                return "null";
            }
        }
        async public Task<string> GetPageBodyInnerText()
        {
            await Task.Delay(1000);
            string pageBody = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetZeroInputStringHandlerOUT(GetPageBodyInnerText), new object[] { });
                }
                else
                {
                    string preScript = "document.body.innerText";
                    Console.WriteLine("Prescript GetPageBodyInnerText: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        pageBody = null;

                    }
                    else if (x.Equals("null"))
                    {
                        pageBody = null;
                    }
                    else
                    {
                        pageBody = x;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
                return pageBody;
                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetPageBodyInnerText: " + ex);
                return "null";
            }
        }
        async public Task<string> GetPageBodyClass()
        {
            await Task.Delay(1000);
            string pageBody = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetZeroInputStringHandlerOUT(GetPageBodyClass), new object[] { });
                }
                else
                {
                    string preScript = "document.body.getAttribute('class');";
                    Console.WriteLine("Prescript GetPageBodyClass: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        pageBody = null;

                    }
                    else if (x.Equals("null"))
                    {
                        pageBody = null;
                    }
                    else
                    {
                        pageBody = x;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
                return pageBody;
                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetPageBodyInnerText: " + ex);
                return "null";
            }
        }
        #region Public Node Methods
        async public Task<string> ExtractNodeByElementID(string nodeElement, string nodePath)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementIDAsync(nodeElement, nodePath);
                    if (String.IsNullOrEmpty(nodeOuterHTML))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in ExtractNodeByElementID");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of ExtractNodeByElementID: " + nodeElement);
                            return null;
                        }
                    }
                    else
                    {
                        return nodeOuterHTML;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ExtractNodeByElementID: " + ex);
                return null;
            }
        }
        async public Task<string> ExtractNodeByElementName(string nodeElement, string nodePath, int elementNumber = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementNameAsync(nodeElement, nodePath, elementNumber);
                    Console.WriteLine("NodeOuterHTML : " + nodeOuterHTML);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in ExtractNodeByElementName");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of ExtractNodeByElementName: " + nodeElement);
                            return null;
                        }
                    }
                    else
                    {
                        return nodeOuterHTML;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ExtractNodeByElementName: " + ex);
                return null;
            }
        }
        async public Task<string> ExtractNodeByClassName(string nodeClass, string nodePath, int classNodeLocation = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByClassNameAsync(nodeClass, nodePath, classNodeLocation);
                    Console.WriteLine("NodeOuterHTML : " + nodeOuterHTML);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in PopulateNodeTextByClass");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of PopulateNodeTextByClass: " + nodeClass);
                            return "Not found";
                        }
                    }
                    else
                    {
                        return nodeOuterHTML;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByClass: " + ex);
                return "Not found";
            }
        }
        async public Task ClickNodeElementByID(string nodeElement, string nodePath)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementIDAsync(nodeElement, nodePath);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in ClickNodeElementByID");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of ClickNodeElementByID: " + nodeElement);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await ClickNodeElementByIDAsync(nodeElement, nodePath);
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickNodeElementByID: " + ex);
            }
        }
        async public Task ClickNodeElementByClass(string nodeClass, string nodePath, int classNodeLocation = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByClassNameAsync(nodeClass, nodePath, classNodeLocation);
                    Console.WriteLine("NodeOuterHTML : " + nodeOuterHTML);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in PopulateNodeTextByClass");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of PopulateNodeTextByClass: " + nodeClass);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await ClickNodeElementByClassAsync(nodeClass, nodePath, classNodeLocation);
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByClass: " + ex);
            }
        }
        async public Task ClickNodeElementByName(string nodeElement, string nodePath, int elementNumber = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementNameAsync(nodeElement, nodePath);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in ClickNodeElementByName");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of ClickNodeElementByName: " + nodeElement);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await Task.Run(() => ClickNodeElementByNameAsync(nodeElement, nodePath, elementNumber));
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickNodeElementByName: " + ex);
            }

        }
        async public Task PopulateNodeTextByID(string nodeElement, string nodePath, string value)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementIDAsync(nodeElement, nodePath);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in PopulateNodeTextByID");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of PopulateNodeTextByID: " + nodeElement);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await Task.Run(() => PopulateNodeTextByIDAsync(nodeElement, nodePath, value));
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByID: " + ex);
            }

        }
        async public Task PopulateNodeTextByClass(string nodeClass, string nodePath, string value, int classNodeLocation = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByClassNameAsync(nodeClass, nodePath, classNodeLocation);
                    Console.WriteLine("NodeOuterHTML : " + nodeOuterHTML);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in PopulateNodeTextByClass");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of PopulateNodeTextByClass: " + nodeClass);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await PopulateNodeTextByClassAsync(nodeClass, nodePath, value, classNodeLocation);
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByClass: " + ex);
            }
        }
        async public Task PopulateNodeTextByName(string nodeElement, string nodePath, string value, int elementNumber = 0)
        {
            try
            {
                while (true)
                {
                    string nodeOuterHTML = await ExtractNodeByElementNameAsync(nodeElement, nodePath, elementNumber);
                    Console.WriteLine("NodeOuterHTML : " + nodeOuterHTML);
                    if (String.IsNullOrEmpty(nodeOuterHTML) || nodeOuterHTML.Contains("null"))
                    {
                        await Task.Delay(500);
                        Console.WriteLine("Trying again in PopulateNodeTextByClass");
                        taskCounter++;
                        if (taskCounter >= taskRetries)
                        {
                            taskCounter = 0;
                            Console.WriteLine("Broke out of PopulateNodeTextByName: " + nodeElement);
                            break;
                        }
                    }
                    else
                    {
                        taskCounter = 0;
                        await PopulateNodeTextByNameAsync(nodeElement, nodePath, value, elementNumber);
                        await Task.Delay(500);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByClass: " + ex);
            }
        }
        #endregion
        async public Task ExecuteDevToolProtocol(string method, string parametersJSONasString)
        {
            await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync(method, parametersJSONasString);
        }
        #endregion
        #region Private Async Methods
        async private Task PauseForHalfSecond()
        {
            await Task.Delay(500);
            Console.WriteLine("Paused for Half a second");
        }
        async private Task NavigateToNewAsync(string url)
        {
            if (InvokeRequired)
            {

                Invoke(new GetOneInputStringHandler(NavigateToNewAsync), new object[] { url });
            }
            else
            {
                webView2.Source = new Uri(url);
                await Task.Delay(3000);
                applicationReady = true;
            }
        }
        async private Task PopulateTextAreaByIDAsync(string elementID, string value)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(PopulateTextAreaByIDAsync), new object[] { elementID, value });
                }
                else
                {
                    await Task.Delay(populateDelay);
                    String prescript1 = "document.getElementById(\"" + elementID + "\").focus();";
                    Console.WriteLine("Prescript PopulateTextAreaByIDAsync: " + prescript1);
                    await webView2.ExecuteScriptAsync(prescript1);
                    await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateTextAreaAsync: " + ex);
            }

        }
        async private Task PopulateTextAreaByNameAsyncPrivate(string elementName, string value, int inputNum)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoStringOneIntInputHandler(PopulateTextAreaByNameAsyncPrivate), new object[] { elementName, value, inputNum });
                }
                else
                {
                    await Task.Delay(populateDelay);
                    string preScript = "document.getElementsByName(\"" + elementName + "\")[" + inputNum + "].focus();";
                    Console.WriteLine("Prescript PopulateTextAreaByNameAsync: " + preScript);
                    await webView2.ExecuteScriptAsync(preScript);
                    await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateTextAreaByNameAsyncPrivate: " + ex);
            }
        }
        async private Task SetValueofElementByNamePrivate(string elementName, string value, int inputNum)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoStringOneIntInputHandler(SetValueofElementByNamePrivate), new object[] { elementName, value, inputNum });
                }
                else
                {
                    await Task.Delay(populateDelay);
                    string preScript1 = "document.getElementsByName(\"" + elementName + "\")[" + inputNum + "].focus();";
                    string preScript2 = "document.getElementsByName(\"" + elementName + "\")[" + inputNum + "].value =  '" + value + "';";
                    Console.WriteLine("Prescript PopulateTextAreaByNameAsync: " + preScript1);
                    await webView2.ExecuteScriptAsync(preScript1);
                    await webView2.ExecuteScriptAsync(preScript2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO PopulateTextAreaAsync: " + ex);
            }
        }
        async private Task SetValueInIFRAMEByNameAsync(string iframeId, string elementName, string value, int elementNumber)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeStringOneIntInputHandler(SetValueInIFRAMEByNameAsync), new object[] { iframeId, elementName, value, elementNumber });
                }
                else
                {
                    string preScript = "document.getElementById(\"" + iframeId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].value = '" + value + "';";
                    await webView2.ExecuteScriptAsync(preScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception SetValueInIFRAMEByNameAsync: " + ex);
            }
        }
        async private Task PopulateTextAreaInIFRAMEByNameAsyncPrivate(string iframeId, string elementName, string value)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeInputStringHandler(PopulateTextAreaInIFRAMEByNameAsyncPrivate), new object[] { iframeId, elementName, value });
                }
                else
                {
                    await Task.Delay(populateDelay);
                    string preScript = "document.getElementById(\"" + iframeId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].focus();";
                    Console.WriteLine("Prescript PopulateTextAreaByNameAsync: " + preScript);
                    await webView2.ExecuteScriptAsync(preScript);
                    await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO PopulateTextAreaAsync: " + ex);
            }
        }
        async private Task PopulateTextAreaByClassNameAsync(string className, string value, int itemNumber)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoStringOneIntInputHandler(PopulateTextAreaByClassNameAsync), new object[] { className, value, itemNumber });
                }
                else
                {
                    await Task.Delay(populateDelay);
                    string preScript = "document.getElementsByClassName(\"" + className + "\")[" + itemNumber + "].focus();";
                    Console.WriteLine("Prescript PopulateTextAreaByClassName: " + preScript);
                    await webView2.ExecuteScriptAsync(preScript);
                    await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO PopulateTextAreaAsync: " + ex);
            }
        }
        async private Task ClickItemByIDAsync(string elementID)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandler(ClickItemByIDAsync), new object[] { elementID });
                }
                else
                {
                    await Task.Delay(500);
                    string focusScript = "document.getElementById('" + elementID + "').focus();";
                    string preScript = "document.getElementById('" + elementID + "').click();";
                    string checkEnabledScript = "document.getElementById(\"" + elementID + "\").disabled";
                    var isEnabled = await webView2.ExecuteScriptAsync(checkEnabledScript);
                    if (isEnabled.ToString().Equals("true"))
                    {
                        String enableScript = "document.getElementById(\"" + elementID + "\").disabled = false;";
                        await webView2.ExecuteScriptAsync(enableScript);
                    }
                    Console.WriteLine("Prescript ClickItemByIDAsync: " + preScript);
                    await webView2.ExecuteScriptAsync(focusScript);
                    await webView2.ExecuteScriptAsync(preScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO ClickItemByIDAsync: " + ex);
            }
        }
        async private Task ClickItemByNameAsync(string elementName, int elementNum)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneStringAndOneIntHandler(ClickItemByNameAsync), new object[] { elementName, elementNum });
                }
                else
                {
                    await Task.Delay(500);
                    string preScript = "document.getElementsByName(\"" + elementName + "\")[" + elementNum + "].focus();";
                    await webView2.ExecuteScriptAsync(preScript);
                    string clickScript = "document.getElementsByName(\"" + elementName + "\")[" + elementNum + "].click();";
                    Console.WriteLine("Prescript ClickItemByNameAsync: " + preScript);
                    await webView2.ExecuteScriptAsync(clickScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickItemByNameAsync: " + ex);
            }
        }

        async private Task ClickItemByClassNameAsync(string elementName, int elementNum)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneStringAndOneIntHandler(ClickItemByClassNameAsync), new object[] { elementName, elementNum });
                }
                else
                {
                    await Task.Delay(500);
/*                    string preScript = "document.getElementsByClassName(\"" + elementName + "\")[" + elementNum + "].focus();";
                    await webView2.ExecuteScriptAsync(preScript);*/
                    string clickScript = "document.getElementsByClassName(\"" + elementName + "\")[" + elementNum + "].click();";
                    //Console.WriteLine("Prescript ClickItemByNameAsync: " + preScript);
                    Console.WriteLine("Prescript ClickItemByClassNameAsync: " + clickScript);
                    await webView2.ExecuteScriptAsync(clickScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickItemByNameAsync: " + ex);
            }
        }
        async private Task ClickItemInIFRAMEByNamePrivate(string iFrameId, string elementName)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(ClickItemInIFRAMEByNamePrivate), new object[] { iFrameId, elementName });
                }
                else
                {
                    string preScript = "document.getElementById(\"" + iFrameId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].focus();";
                    string clickScript = "document.getElementById(\"" + iFrameId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].click();";
                    Console.WriteLine("Prescript ClickItemInIFRAMEByNamePrivate: " + preScript);
                    await webView2.ExecuteScriptAsync(preScript);
                    await webView2.ExecuteScriptAsync(clickScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickItemInIFRAMEByNamePrivate: " + ex);
            }
        }
        async private Task ClickItemByInputTypeAndIDAsync(string inputType, string elementID)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandler(ClickItemByIDAsync), new object[] { elementID });
                }
                else
                {
                    await Task.Delay(500);
                    string preScript = "document.getElementById(\"" + elementID + "\").focus();";
                    string clickScript = "document.getElementById(\"" + elementID + "\").click();";
                    Console.WriteLine("Prescript ClickItemByIDAsync: " + preScript);
                    await webView2.ExecuteScriptAsync(preScript);
                    await webView2.ExecuteScriptAsync(clickScript);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO ClickItemByIDAsync: " + ex);
            }
        }
        async private Task<string> GetInfoByIDAsync(string elementId, string attribute)
        {
            string result = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(GetInfoByIDAsync), new object[] { elementId, attribute });
                }
                else
                {
                    if (attribute.ToUpper().Equals("OUTERHTML"))
                    {
                        string preScript = "document.getElementById(\"" + elementId + "\").outerHTML;";
                        Console.WriteLine("Prescript GetInfoByID: " + preScript);
                        var x = await webView2.ExecuteScriptAsync(preScript);
                        Console.WriteLine("Var x = " + x);
                        await SetReturnValue(x);
                        result = x;
                    }
                    else
                    {
                        await Task.Delay(500);
                        string preScript = "document.getElementById(\"" + elementId + "\").getAttribute(\"" + attribute + "\");";
                        Console.WriteLine("Prescript GetInfoByID: " + preScript);
                        var x = await webView2.ExecuteScriptAsync(preScript);
                        Console.WriteLine("Var x = " + x);
                        await SetReturnValue(x);
                        result = x;
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetInfoByID: " + ex);
                result = "Item Not Found";
            }
            return result;
        }
        async private Task<string> GetInfoByNameAsync(string elementName, string attribute)
        {
            string result = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(GetInfoByNameAsync), new object[] { elementName, attribute });
                }
                else
                {
                    if (attribute.ToUpper().Equals("OUTERHTML"))
                    {
                        await Task.Delay(500);
                        string preScript = "document.getElementsByName(\"" + elementName + "\")[0].outerHTML;";
                        Console.WriteLine("Prescript GetInfoByNameAsync: " + preScript);
                        var x = await webView2.ExecuteScriptAsync(preScript);
                        Console.WriteLine("Var x = " + x);
                        await SetReturnValue(x);
                        result = x;
                    }
                    else
                    {
                        await Task.Delay(500);
                        string preScript = "document.getElementsByName(\"" + elementName + "\")[0].getAttribute(\'" + attribute + "\');";
                        Console.WriteLine("Prescript GetInfoByNameAsync: " + preScript);
                        var x = await webView2.ExecuteScriptAsync(preScript);
                        Console.WriteLine("Var x = " + x);
                        await SetReturnValue(x);
                        result = x;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetInfoByID: " + ex);
                result = "Item Not Found";
            }
            return result;
        }
        async private Task GetInformationInIFRAMEByNameAsync(string iFrameId, string elementName, string attribute)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeInputStringHandler(GetInformationInIFRAMEByNameAsync), new object[] { iFrameId, elementName, attribute });
                }
                else
                {
                    await Task.Delay(500);
                    string preScript = "document.getElementById(\'" + iFrameId + "\').contentWindow.document.getElementsByName(\'" + elementName + "\')[0].getAttribute(\'" + attribute + "\');";
                    Console.WriteLine("Prescript GetInformationInIFRAMEByNameAsync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    await SetReturnValue(x);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetInformationInIFRAMEByNameAsync: " + ex);
            }
        }
        async private Task GetInformationInIFRAMEByIDAsync(string iFrameId, string elementID, string attribute)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeInputStringHandler(GetInformationInIFRAMEByIDAsync), new object[] { iFrameId, elementID, attribute });
                }
                else
                {
                    await Task.Delay(500);
                    string preScript = "document.getElementById(\'" + iFrameId + "\').contentWindow.document.getElementById(\'" + elementID + "\').getAttribute(\'" + attribute + "\');";
                    Console.WriteLine("Prescript GetInformationInIFRAMEByIDAsync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    await SetReturnValue(x);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetInformationInIFRAMEByIDAsync: " + ex);
            }
        }
        async private Task<string> FindControl(string name)
        {
            string result = String.Empty;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandlerOUT(FindControl), new object[] { name });
                }
                else
                {
                    await Task.Delay(500);
                    // Find by id first
                    string preScript = "document.getElementById(\"" + name + "\").outerHTML;";
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    if (!String.IsNullOrEmpty(x) || x != null)
                    {
                        result = "document.getElementById(\"" + name + "\")";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
                return null;
            }
            return result;
        }
        #region Private Node Methods
        async private Task SetAttributeOfNodeByElementIDAsync(string nodeElement, string nodePath, string attribute, string value)
        {
            string initialNodeQuery = "document.getElementById('" + nodeElement + "')";
            string finalQuery = initialNodeQuery;
            try
            {
                Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception SetAttributeOfNodeByElemendIDAsync: " + ex);
            }
            finalQuery = finalQuery + ".setAttribute('" + attribute + "','" + value + "');";
            Console.WriteLine("Final query = " + finalQuery);
            await webView2.ExecuteScriptAsync(finalQuery);
        }
        async private Task<string> ExtractNodeByElementIDAsync(string nodeElement, string nodePath)
        {
            string initialNodeQuery = "document.getElementById('" + nodeElement + "')";
            string finalQuery = initialNodeQuery;
            try
            {
                Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            await Task.Delay(500);
            finalQuery = finalQuery + ".outerHTML;";
            Console.WriteLine("Final query = " + finalQuery);
            var x = await webView2.ExecuteScriptAsync(finalQuery);
            Console.WriteLine("Var x = : " + x);
            return x;
        }
        async private Task<string> ExtractNodeByClassNameAsync(string nodeClass, string nodePath, int classNodeLocation = 0)
        {
            string initialNodeQuery = "document.getElementsByClassName('" + nodeClass + "')[" + classNodeLocation + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeClass))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            await Task.Delay(500);
            finalQuery = finalQuery + ".outerHTML;";
            Console.WriteLine("Final query = " + finalQuery);
            var x = await webView2.ExecuteScriptAsync(finalQuery);
            return x;
        }
        async private Task<string> ExtractNodeByElementNameAsync(string nodeElement, string nodePath, int elementNumber = 0)
        {
            string initialNodeQuery = "document.getElementsByName('" + nodeElement + "')[" + elementNumber + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                return null;
            }
            await Task.Delay(500);
            finalQuery = finalQuery + ";";
            var x = await webView2.ExecuteScriptAsync(finalQuery);
            return x;
        }
        async public Task ClickNodeElementByIDAsync(string nodeElement, string nodePath)
        {
            string initialNodeQuery = "document.getElementById('" + nodeElement + "')";
            string finalQuery = initialNodeQuery;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(ClickNodeElementByIDAsync), new object[] { nodeElement, nodePath });
                }
                else
                {
                    Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                    for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                    {
                        if (String.IsNullOrEmpty(nodeElement))
                        {
                            break;
                        }
                        switch (nodePath[nodeCount])
                        {
                            case 'n'://next sibling
                                finalQuery = finalQuery + ".nextSibling";
                                break;
                            case 'f'://first child
                                finalQuery = finalQuery + ".firstChild";
                                break;
                            case 'p': //parent node
                                finalQuery = finalQuery + ".parentNode";
                                break;
                            case 'l': //lastChild
                                finalQuery = finalQuery + ".lastChild";
                                break;
                            case 'r': //previous sibling
                                finalQuery = finalQuery + ".previousSibling";
                                break;
                            default:
                                finalQuery = finalQuery + ".nextSibling";
                                break;
                        }
                    }
                    await Task.Delay(500);
                    string focusScript = finalQuery + ".focus();";
                    string preScript = finalQuery + ".click();";
                    Console.WriteLine("Final query = " + finalQuery);
                    await webView2.ExecuteScriptAsync(focusScript);
                    await webView2.ExecuteScriptAsync(preScript);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickNodeByElementID: " + ex);
            }

        }
        async private Task ClickNodeElementByClassAsync(string nodeClass, string nodePath, int classNodeLocation = 0)
        {
            string initialNodeQuery = "document.getElementsByClassName('" + nodeClass + "')[" + classNodeLocation + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoStringOneIntInputHandler(ClickNodeElementByClassAsync), new object[] { nodeClass, nodePath, classNodeLocation });
                }
                else
                {
                    Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                    for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                    {
                        if (String.IsNullOrEmpty(nodeClass))
                        {
                            break;
                        }
                        switch (nodePath[nodeCount])
                        {
                            case 'n'://next sibling
                                finalQuery = finalQuery + ".nextSibling";
                                break;
                            case 'f'://first child
                                finalQuery = finalQuery + ".firstChild";
                                break;
                            case 'p': //parent node
                                finalQuery = finalQuery + ".parentNode";
                                break;
                            case 'l': //lastChild
                                finalQuery = finalQuery + ".lastChild";
                                break;
                            case 'r': //previous sibling
                                finalQuery = finalQuery + ".previousSibling";
                                break;
                            default:
                                finalQuery = finalQuery + ".nextSibling";
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickNodeElementByClass: " + ex);
            }
            await Task.Delay(500);
            string focusScript = finalQuery + ".focus();";
            string preScript = finalQuery + ".click();";
            Console.WriteLine("Final query = " + finalQuery);
            await webView2.ExecuteScriptAsync(focusScript);
            await webView2.ExecuteScriptAsync(preScript);
        }
        async private Task ClickNodeElementByNameAsync(string nodeElement, string nodePath, int elementNumber = 0)
        {
            string initialNodeQuery = "document.getElementsByName('" + nodeElement + "')[" + elementNumber + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception ClickNodeElementByName: " + ex);
            }
            await Task.Delay(500);
            string focusScript = finalQuery + ".focus();";
            string preScript = finalQuery + ".click();";
            Console.WriteLine("Final query = " + finalQuery);
            await webView2.ExecuteScriptAsync(focusScript);
            await webView2.ExecuteScriptAsync(preScript);
        }
        async private Task PopulateNodeTextByIDAsync(string nodeElement, string nodePath, string value)
        {
            string initialNodeQuery = "document.getElementById('" + nodeElement + "')";
            string finalQuery = initialNodeQuery;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeInputStringHandler(PopulateNodeTextByIDAsync), new object[] { nodeElement, nodePath, value });
                }
                Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByID: " + ex);
            }
            await Task.Delay(populateDelay);
            string focusScript = finalQuery + ".focus();";
            Console.WriteLine("Prescript PopulateNodeTextByID: " + focusScript);
            await webView2.ExecuteScriptAsync(focusScript);
            await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
        }
        async private Task PopulateNodeTextByNameAsync(string nodeElement, string nodePath, string value, int elementNumber = 0)
        {
            string initialNodeQuery = "document.getElementsByName('" + nodeElement + "')[" + elementNumber + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeStringOneIntInputHandler(PopulateNodeTextByNameAsync), new object[] { nodeElement, nodePath, value, elementNumber });
                }
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeElement))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByName: " + ex);
            }
            await Task.Delay(populateDelay);
            string focusScript = finalQuery + ".focus();";
            Console.WriteLine("Prescript PopulateNodeTextByName: " + focusScript);
            await webView2.ExecuteScriptAsync(focusScript);
            await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
        }
        async private Task PopulateNodeTextByClassAsync(string nodeClass, string nodePath, string value, int classNodeLocation = 0)
        {
            string initialNodeQuery = "document.getElementsByClassName('" + nodeClass + "')[" + classNodeLocation + "]";
            string finalQuery = initialNodeQuery;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeStringOneIntInputHandler(PopulateNodeTextByClassAsync), new object[] { nodeClass, nodePath, value, classNodeLocation });
                }
                Console.WriteLine("Extract node by ID: Length = " + nodePath.Length);
                for (int nodeCount = 0; nodeCount < nodePath.Length; nodeCount++)
                {
                    if (String.IsNullOrEmpty(nodeClass))
                    {
                        break;
                    }
                    switch (nodePath[nodeCount])
                    {
                        case 'n'://next sibling
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                        case 'f'://first child
                            finalQuery = finalQuery + ".firstChild";
                            break;
                        case 'p': //parent node
                            finalQuery = finalQuery + ".parentNode";
                            break;
                        case 'l': //lastChild
                            finalQuery = finalQuery + ".lastChild";
                            break;
                        case 'r': //previous sibling
                            finalQuery = finalQuery + ".previousSibling";
                            break;
                        default:
                            finalQuery = finalQuery + ".nextSibling";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception PopulateNodeTextByClass: " + ex);
            }
            await Task.Delay(populateDelay);
            string focusScript = finalQuery + ".focus();";
            Console.WriteLine("Prescript PopulateNodeTextByClass: " + focusScript);
            await webView2.ExecuteScriptAsync(focusScript);
            await webView2.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText", "{ \"text\": \"" + value + "\"}");
        }
        #endregion
        #endregion


        #region Experimental Methods
        async public Task SetValueOfElementinIFRAMEByName(string iframeId, string elementName, string value)
        {
            await Task.Delay(1000);
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetThreeInputStringHandler(SetValueOfElementinIFRAMEByName), new object[] { iframeId, elementName, value });
                }
                else
                {
                    await Task.Delay(500);
                    //string preScript1 = "document.getElementById(\"" + iframeId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].focus();";
                    string preScript2 = "document.getElementById(\"" + iframeId + "\").contentWindow.document.getElementsByName(\"" + elementName + "\")[0].value = '" + value + "';";
                    Console.WriteLine("Prescript PopulateTextAreaByNameAsync: " + preScript2);
                    // await webView2.ExecuteScriptAsync(preScript1);
                    // await Task.Delay(5000);
                    await webView2.ExecuteScriptAsync(preScript2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception EDGEAUTO PopulateTextAreaAsync: " + ex);
            }
        }
        #endregion
        #region Item Checks
        async private Task GetItemByIdAsync(string elementID)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandler(GetItemByIdAsync), new object[] { elementID });
                }
                else
                {
                    string preScript = "document.getElementById('" + elementID + "').outerHTML;";
                    Console.WriteLine("Prescript GetInfoByID: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;

                    }
                    else if (x.Equals("null"))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;
                    }
                    else
                    {
                        Console.WriteLine("Click found set to true");
                        clickableFound = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetInfoByID: " + ex);
            }
        }
        async private Task GetItemByNameAsync(string elementName)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandler(GetItemByIdAsync), new object[] { elementName });
                }
                else
                {
                    string preScript = "document.getElementsByName(\"" + elementName + "\")[0].outerHTML;";
                    Console.WriteLine("Prescript GetItemByNameAsync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;

                    }
                    else if (x.Equals("null"))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;
                    }
                    else
                    {
                        Console.WriteLine("Click found set to true");
                        clickableFound = true;
                    }
                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetItemByNameAsync: " + ex);
            }
        }
        async private Task GetItemByClassNameAsync(string elementClassName, int elementNum = 0)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneStringAndOneIntHandler(GetItemByClassNameAsync), new object[] { elementClassName,  elementNum});
                }
                else
                {
                    string preScript = "document.getElementsByClassName(\"" + elementClassName + "\")[" + elementNum + "].outerHTML;";
                    Console.WriteLine("Prescript GetItemByClassNameAsync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;

                    }
                    else if (x.Equals("null"))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;
                    }
                    else
                    {
                        Console.WriteLine("Click found set to true");
                        clickableFound = true;
                    }
                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetItemByNameAsync: " + ex);
            }
        }
        async private Task GetIFRAMEItemByNameASync(string iFrameId, string elementName)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(GetIFRAMEItemByNameASync), new object[] { iFrameId, elementName });
                }
                else
                {
                    string preScript = "document.getElementById(\'" + iFrameId + "\').contentWindow.document.getElementsByName(\'" + elementName + "\')[0].outerHTML;";
                    Console.WriteLine("Prescript GetIFRAMEItemByNameASync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("GetIFRAMEItemByNameASync: Var x = " + x.ToString());
                    if (String.IsNullOrEmpty(x))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;

                    }
                    else if (x.Equals("null"))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;
                    }
                    else
                    {
                        Console.WriteLine("Click found set to true");
                        clickableFound = true;
                    }
                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetItemByNameAsync: " + ex);
            }
        }
        async private Task GetIFRAMEItemByIDASync(string iFrameId, string elementId)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetTwoInputStringHandler(GetIFRAMEItemByIDASync), new object[] { iFrameId, elementId });
                }
                else
                {
                    string preScript = "document.getElementById(\'" + iFrameId + "\').contentWindow.document.getElementById(\'" + elementId + "\').outerHTML;";
                    Console.WriteLine("Prescript GetIFRAMEItemByIDASync: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;

                    }
                    else if (x.Equals("null"))
                    {
                        Console.WriteLine("Click found set to false");
                        clickableFound = false;
                    }
                    else
                    {
                        Console.WriteLine("Click found set to true");
                        clickableFound = true;
                    }
                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception GetIFRAMEItemByIDASync: " + ex);
            }
        }
        async private Task<bool> CheckIfElementExistsByID(string elementID)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneInputStringHandler(CheckIfElementExistsByID), new object[] { elementID });
                }
                else
                {
                    string preScript = "document.getElementById(\"" + elementID + "\").outerHTML;";
                    Console.WriteLine("Prescript GetInfoByID: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        this.elementExists = false;

                    }
                    else if (x.Equals("null"))
                    {
                        this.elementExists = false;
                    }
                    else
                    {
                        this.elementExists = true;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }

                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByID: " + ex);
            }
            return this.elementExists;
        }
        async private Task<bool> CheckIfElementExistsByName(string elementName, int elementNumber = 0)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneStringAndOneIntHandler(CheckIfElementExistsByName), new object[] { elementName, elementNumber });
                }
                else
                {
                    string preScript = "document.getElementsByName(\"" + elementName + "\")[" + elementNumber + "].outerHTML;";
                    Console.WriteLine("Prescript CheckIfElementExistsByName: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        this.elementExists = false;

                    }
                    else if (x.Equals("null"))
                    {
                        this.elementExists = false;
                    }
                    else
                    {
                        this.elementExists = true;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }

                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByID: " + ex);
            }
            return this.elementExists;
        }
        async private Task<bool> CheckIfElementExistsByClassName(string className, int elementNumber = 0)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new GetOneStringAndOneIntHandler(CheckIfElementExistsByClassName), new object[] { className, elementNumber });
                }
                else
                {
                    string preScript = "document.getElementsByClassName(\"" + className + "\")[" + elementNumber + "].outerHTML;";
                    Console.WriteLine("Prescript CheckIfElementExistsByClassName: " + preScript);
                    var x = await webView2.ExecuteScriptAsync(preScript);
                    Console.WriteLine("Var x = " + x);
                    if (String.IsNullOrEmpty(x))
                    {
                        this.elementExists = false;

                    }
                    else if (x.Equals("null"))
                    {
                        this.elementExists = false;
                    }
                    else
                    {
                        this.elementExists = true;
                    }

                    //  existingElement = webView2.ExecuteScriptAsync("document.getElementsByName(\"" + elementName + "\")[0]." + "value");

                }

                //this.resultsList.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception CheckIfElementExistsByClassName: " + ex);
            }
            return this.elementExists;
        }
        #endregion

    }


}
