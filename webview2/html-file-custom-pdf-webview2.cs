// NuGet: Install-Package Microsoft.Web.WebView2
// CreatePrintSettings() lives on CoreWebView2Environment.
// Margin* / PageWidth / PageHeight on CoreWebView2PrintSettings are in INCHES.
// PrintToPdfAsync(path, settings) returns Task<bool> (true on success) — not a stream.
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

        CoreWebView2PrintSettings printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
        printSettings.Orientation = CoreWebView2PrintOrientation.Landscape;
        printSettings.MarginTop = 0.5;     // inches
        printSettings.MarginBottom = 0.5;  // inches
        printSettings.ShouldPrintBackgrounds = true;

        bool ok = await webView.CoreWebView2.PrintToPdfAsync("custom.pdf", printSettings);
        Console.WriteLine(ok ? "Custom PDF created" : "PrintToPdfAsync returned false");
    }
}