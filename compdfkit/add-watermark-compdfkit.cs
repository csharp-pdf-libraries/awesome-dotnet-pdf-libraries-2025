// NuGet: Install-Package ComPDFKit.NetCore
// Real ComPDFKit watermark API uses CPDFDocument.InitWatermark() returning a
// CPDFWatermark object. Verified against
// https://www.compdf.com/guides/pdf-sdk/windows/watermark/add-image-watermark
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;
using System;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("input.pdf");

        // Create a text watermark
        CPDFWatermark watermark = document.InitWatermark(C_Watermark_Type.WATERMARK_TYPE_TEXT);
        watermark.SetText("CONFIDENTIAL");
        watermark.SetFontName("Helvetica");
        watermark.SetFontSize(48);
        watermark.SetTextRGBColor(255, 0, 0);
        watermark.SetScale(1);
        watermark.SetRotation(45);
        watermark.SetOpacity(76);              // 0..255 (76 ≈ 30%)
        watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER);
        watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER);
        watermark.SetVertOffset(0);
        watermark.SetHorizOffset(0);
        watermark.SetPages($"0-{document.PageCount - 1}");
        watermark.SetFront(true);
        watermark.CreateWatermark();

        document.WriteToFilePath("watermarked.pdf");
        document.Release();
    }
}
