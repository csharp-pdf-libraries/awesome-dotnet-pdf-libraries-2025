// NuGet: Install-Package Microsoft.Web.WebView2.WinForms
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

class Program
{
    static async Task Main()
    {
        var webView = new WebView2();
        await webView.EnsureCoreWebView2Async();
        
        string htmlFile = Path.Combine(Directory.GetCurrentDirectory(), "input.html");
        webView.CoreWebView2.Navigate(htmlFile);
        
        await Task.Delay(3000);
        
        var printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
        printSettings.Orientation = CoreWebView2PrintOrientation.Landscape;
        printSettings.MarginTop = 0.5;
        printSettings.MarginBottom = 0.5;
        
        using (var stream = await webView.CoreWebView2.PrintToPdfAsync("custom.pdf", printSettings))
        {
            Console.WriteLine("Custom PDF created");
        }
    }
}