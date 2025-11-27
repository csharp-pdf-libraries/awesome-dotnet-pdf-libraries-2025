// NuGet: Install-Package Fonet
using Fonet;
using Fonet.Render.Pdf;
using System.IO;

class Program
{
    static void Main()
    {
        // FoNet settings are configured in XSL-FO markup
        string xslFo = @"<?xml version='1.0' encoding='utf-8'?>
            <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
                <fo:layout-master-set>
                    <fo:simple-page-master master-name='A4' 
                        page-height='297mm' page-width='210mm'
                        margin-top='20mm' margin-bottom='20mm'
                        margin-left='25mm' margin-right='25mm'>
                        <fo:region-body/>
                    </fo:simple-page-master>
                </fo:layout-master-set>
                <fo:page-sequence master-reference='A4'>
                    <fo:flow flow-name='xsl-region-body'>
                        <fo:block font-size='14pt'>Custom PDF</fo:block>
                    </fo:flow>
                </fo:page-sequence>
            </fo:root>";
        
        FonetDriver driver = FonetDriver.Make();
        driver.Render(new StringReader(xslFo), 
            new FileStream("custom.pdf", FileMode.Create));
    }
}