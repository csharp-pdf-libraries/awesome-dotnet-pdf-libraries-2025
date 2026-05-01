// NuGet: Install-Package Fonet      (legacy .NET Framework 2.0, last released 2011)
//   - or: Install-Package Fonet.Standard  (.NET Standard 2.0 fork, last released 2020)
// FO.NET is an unmaintained C# port of an early Apache FOP. It is an XSL-FO
// renderer ONLY — it does not parse HTML.
using Fonet;
using Fonet.Render.Pdf;
using System.IO;

class Program
{
    static void Main()
    {
        // FoNet requires XSL-FO format, not HTML.
        // To render HTML you must first transform it to XSL-FO yourself
        // (typically via XSLT or a custom converter — not built in).
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