// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 40px; }
                    h1 { color: #2c3e50; font-size: 24px; }
                    p { line-height: 1.6; color: #34495e; }
                    .highlight { background-color: yellow; font-weight: bold; }
                </style>
            </head>
            <body>
                <h1>Formatted Document</h1>
                <p>This is a <span class='highlight'>beautifully formatted</span> document with CSS styling.</p>
                <p>Complex layouts, fonts, colors, and images are fully supported.</p>
            </body>
            </html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("formatted.pdf");
        Console.WriteLine("Formatted PDF created successfully");
    }
}