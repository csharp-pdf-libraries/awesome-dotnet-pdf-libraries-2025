// NuGet: Install-Package Foxit.SDK.Dotnet
using foxit;
using foxit.common;
using foxit.pdf;
using System;

class Program
{
    static void Main()
    {
        ErrorCode err = Library.Initialize("sn", "key");
        if (err != ErrorCode.e_ErrSuccess) return;

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.Load("");

                WatermarkSettings settings = new WatermarkSettings();
                settings.flags = (int)Watermark.Flags.e_FlagASPageContents;
                settings.position = Position.e_PosCenter;
                settings.offset_x = 0;
                settings.offset_y = 0;
                settings.scale_x = 1.0f;
                settings.scale_y = 1.0f;
                settings.rotation = -45.0f;
                settings.opacity = 50;     // 0-100 in newer SDKs

                WatermarkTextProperties props = new WatermarkTextProperties();
                props.font = new Font(Font.StandardID.e_StdIDHelvetica);
                props.font_size = 48.0f;
                props.color = 0xFF0000;    // RGB
                props.font_style = WatermarkTextProperties.FontStyle.e_FontStyleNormal;
                props.line_space = 1.0f;
                props.alignment = Alignment.e_AlignmentCenter;

                Watermark watermark = new Watermark(doc, "Confidential", props, settings);

                // No InsertToAllPages — loop pages and call InsertToPage.
                for (int i = 0; i < doc.GetPageCount(); i++)
                {
                    using (PDFPage page = doc.GetPage(i))
                    {
                        watermark.InsertToPage(page);
                    }
                }

                doc.SaveAs("output.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
    }
}
