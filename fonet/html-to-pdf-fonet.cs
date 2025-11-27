// NuGet: Install-Package Fonet
using Fonet;
using Fonet.Render.Pdf;
using System.IO;
using System.Xml;

class Program
{
    static void Main()
    {
        // FoNet requires XSL-FO format, not HTML
        // First convert HTML to XSL-FO (manual process)
        string xslFo = @"<?xml version='1.0' encoding='utf-8'?>
            <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
                <fo:layout-master-set>
                    <fo:simple-page-master master-name='page'>
                        <fo:region-body/>
                    </fo:simple-page-master>
                </fo:layout-master-set>
                <fo:page-sequence master-reference='page'>
                    <fo:flow flow-name='xsl-region-body'>
                        <fo:block>Hello World</fo:block>
                    </fo:flow>
                </fo:page-sequence>
            </fo:root>";
        
        FonetDriver driver = FonetDriver.Make();
        driver.Render(new StringReader(xslFo), 
            new FileStream("output.pdf", FileMode.Create));
    }
}