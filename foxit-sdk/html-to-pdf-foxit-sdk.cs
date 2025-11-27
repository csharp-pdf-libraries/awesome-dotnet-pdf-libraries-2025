// NuGet: Install-Package Foxit.SDK
using Foxit.SDK;
using Foxit.SDK.Common;
using Foxit.SDK.PDFConversion;
using System;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");
        
        HTML2PDFSettingData settingData = new HTML2PDFSettingData();
        settingData.page_width = 612.0f;
        settingData.page_height = 792.0f;
        settingData.page_mode = HTML2PDFPageMode.e_HTML2PDFPageModeSinglePage;
        
        using (HTML2PDF html2pdf = new HTML2PDF(settingData))
        {
            html2pdf.Convert("<html><body><h1>Hello World</h1></body></html>", "output.pdf");
        }
        
        Library.Release();
    }
}