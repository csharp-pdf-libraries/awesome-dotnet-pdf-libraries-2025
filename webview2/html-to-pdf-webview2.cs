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
        
        webView.CoreWebView2.NavigateToString("<html><body><h1>Hello World</h1></body></html>");
        await Task.Delay(2000);
        
        await webView.CoreWebView2.CallDevToolsProtocolMethodAsync(
            "Page.printToPDF",
            "{}"
        );
    }
}