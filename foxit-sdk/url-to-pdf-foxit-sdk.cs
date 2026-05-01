// NuGet: Install-Package Foxit.SDK.Dotnet
// HTML/URL-to-PDF requires the separate Foxit HTML2PDF engine (engine_path)
// obtainable from Foxit support/sales — not in the NuGet package.
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

            // Convert.FromHTML accepts a URL or a literal HTML string in the
            // first argument; the URL form is what makes this URL-to-PDF.
            Convert.FromHTML(
                "https://www.example.com",
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
