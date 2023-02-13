using Microsoft.Web.WebView2.Core;
using MyWpfApp.Models;
using MyWpfApp.Storage;
using Newtonsoft.Json;
using System.Windows;

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
            await webView.ExecuteScriptAsync($"console.log('WPFdan Salom Blazorga');");
        }
        private async void WebView2Control_NavigationCompleted(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var result = args.TryGetWebMessageAsString();
            if (!string.IsNullOrEmpty(result))
            {
                var file = JsonConvert.DeserializeObject<DriveFile>(result);
                this.Close();
                if (storage.DownloadFile(file))
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.RefreshDatagrid(file.Name);
                    mainWindow.Show();
                }
                else
                    MessageBox.Show("Error ???");

            }
        }

    }
}
