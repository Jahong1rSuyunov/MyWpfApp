using Microsoft.Web.WebView2.Core;
using MyWpfApp.Models;
using MyWpfApp.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyWpfApp
{
    /// <summary>
    /// Interaction logic for WebDialog.xaml
    /// </summary>
    public partial class WebDialog : Window
    {
        private Storages storage = new Storages();  
        public WebDialog()
        {
            InitializeComponent();
            webView.Source = new Uri("https://localhost:7139");
            webView.NavigationStarting += WebView2_NavigationStarting;
            webView.WebMessageReceived += WebView2Control_NavigationCompleted;
        }
        public async void SendData()
        {
            
            await webView.EnsureCoreWebView2Async();  // never completes
        }
        private async void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var driveList = storage.GetDriveFiles();
            await webView.ExecuteScriptAsync($"localStorage.setItem(\"newObj\", JSON.stringify({JsonConvert.SerializeObject(driveList)}));");
            await webView.ExecuteScriptAsync($"console.log('dsadsadasdasdasdasdasdas');");
        }
        private async void WebView2Control_NavigationCompleted(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var result = args.TryGetWebMessageAsString();
            if (!string.IsNullOrEmpty(result))
            {
                var file = JsonConvert.DeserializeObject<DriveFile>(result);
                this.Close();
                storage.DownloadFile(file);
                MainWindow mainWindow= new MainWindow();
                mainWindow.RefreshDatagrid(file.Name);
                mainWindow.Show();
            }
        }



    }
}
