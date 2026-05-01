// NuGet: Install-Package Foxit.SDK.Dotnet
// HTML-to-PDF requires the separate Foxit HTML2PDF engine (engine_path)
// available from Foxit support/sales — not shipped in the NuGet package.
using foxit;
using foxit.common;
using foxit.addon.conversion;
using System;

class Program
{
    static void Main()
    {
        ErrorCode err = Library.Initialize("sn", "key");
        if (err != ErrorCode.e_ErrSuccess) return;

        try
        {
            HTML2PDFSettingData settingData = new HTML2PDFSettingData();
            settingData.page_width = 612.0f;
            settingData.page_height = 792.0f;
            settingData.page_mode = HTML2PDFPageMode.e_HTML2PDFPageModeSinglePage;

            // Convert.FromHTML is the real API — engine_path points to the
            // Foxit HTML2PDF engine binaries shipped separately by Foxit.
            Convert.FromHTML(
                "<html><body><h1>Hello World</h1></body></html>",
                "C:\\Foxit\\html2pdf_engine",   // engine_path
                "",                              // cookies path
                settingData,
                "output.pdf",
                30);                             // timeout (seconds)
        }
        finally
        {
            Library.Release();
        }
    }
}
