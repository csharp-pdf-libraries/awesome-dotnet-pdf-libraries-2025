// NuGet: Install-Package GdPicture
// Nutrient .NET SDK (GdPicture.NET) renders text watermarks via low-level PDF drawing
// primitives on GdPicturePDF (DrawTextBox + SetFillAlpha for transparency). There is no
// "TextAnnotation" object — watermarks are drawn as content, optionally inside an OCG
// (Optional Content Group) layer.
using GdPicture14;

class Program
{
    static void Main()
    {
        using var pdf = new GdPicturePDF();
        pdf.LoadFromFile("document.pdf");
        pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);

        var fontResName = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);
        int pageCount = pdf.GetPageCount();

        for (int i = 1; i <= pageCount; i++)
        {
            pdf.SelectPage(i);
            pdf.SetFillAlpha(128); // ~50% opacity (0=transparent, 255=opaque)
            pdf.SetTextSize(48);
            pdf.SetOriginRotationInDegrees(45);
            pdf.DrawTextBox(fontResName,
                100, 300, 500, 400,
                "CONFIDENTIAL",
                PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
                PdfVerticalAlignment.PdfVerticalAlignmentMiddle);
        }

        pdf.SaveToFile("watermarked.pdf");
    }
}
