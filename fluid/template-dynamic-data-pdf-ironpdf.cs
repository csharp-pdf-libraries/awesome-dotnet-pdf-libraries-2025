// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var title = "My List";
        var items = new[] { "Item 1", "Item 2", "Item 3" };
        
        var html = $@"
            <html><body>
                <h1>{title}</h1>
                <ul>";
        
        foreach (var item in items)
        {
            html += $"<li>{item}</li>";
        }
        
        html += "</ul></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("template-output.pdf");
    }
}