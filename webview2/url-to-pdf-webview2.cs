// NuGet: Install-Package Microsoft.Web.WebView2.WinForms
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

class Program
{
    static async Task Main()
    {
        var webView = new WebView2();
        await webView.EnsureCoreWebView2Async();
        
        var tcs = new TaskCompletionSource<bool>();
        webView.CoreWebView2.NavigationCompleted += (s, e) => tcs.SetResult(true);
        
        webView.CoreWebView2.Navigate("https://example.com");
        await tcs.Task;
        await Task.Delay(1000);
        
        var result = await webView.CoreWebView2.CallDevToolsProtocolMethodAsync(
            "Page.printToPDF",
            "{\"printBackground\": true}"
        );
        
        var base64 = System.Text.Json.JsonDocument.Parse(result).RootElement.GetProperty("data").GetString();
        File.WriteAllBytes("output.pdf", Convert.FromBase64String(base64));
    }
}