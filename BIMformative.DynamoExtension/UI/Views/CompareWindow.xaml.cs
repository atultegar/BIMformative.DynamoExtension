using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows;

namespace BIMformative.DynamoExtension.UI.Views
{
    /// <summary>
    /// Interaction logic for CompareWindow.xaml
    /// </summary>
    public partial class CompareWindow : Window
    {
        private readonly string _compareUrl;
        private readonly object _payload;
        public  CompareWindow(string compareUrl, object payload)
        {
            InitializeComponent();

            _compareUrl = compareUrl;
            _payload = payload;

            Loaded += CompareWindow_Loaded;
        }

        private async void CompareWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async();

            string json =JsonConvert.SerializeObject(_payload);

            // Escape safely for JS injection
            string escapedJson = json
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"");

            string script = $@"
                window.addEventListener('DOMContentLoaded', function() {{
                    sessionStorage.setItem('payload', '{escapedJson}');
                }});
            ";

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

            webView.CoreWebView2.Navigate(_compareUrl);
        }

        

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
