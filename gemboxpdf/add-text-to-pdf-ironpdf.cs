// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<p>Original Content</p>");

        var stamper = new TextStamper()
        {
            Text = "Hello World",
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalOffset = new Length(100, MeasurementUnit.Pixels),
            VerticalOffset = new Length(700, MeasurementUnit.Pixels)
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("output.pdf");
    }
}