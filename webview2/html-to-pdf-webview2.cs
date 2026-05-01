// NuGet: Install-Package Microsoft.Web.WebView2
// (the WinForms host lives in the same package; no separate .WinForms package)
// Requires the Edge WebView2 Runtime installed on the target machine. Windows-only.
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

        // PrintToPdfAsync(path, printSettings) returns Task<bool>; null settings = defaults
        bool ok = await webView.CoreWebView2.PrintToPdfAsync("output.pdf", null);
    }
}