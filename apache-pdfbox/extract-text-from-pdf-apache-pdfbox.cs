// Apache PDFBox is a Java library — there is no official .NET port.
// Community options on nuget.org:
//   - Pdfbox-IKVM 1.8.9 (last published Mar 2017, IKVM-based, abandoned)
//   - PdfBox_DotNet_Version 2.0.15 (last published Jul 2019, abandoned)
//   - MASES.NetPDF 3.0.x (active, JCOBridge — requires a JVM at runtime)
// All ports mirror the Java package hierarchy, so namespaces match
// Java exactly (e.g. org.apache.pdfbox.pdmodel.PDDocument).
//
// Example using the IKVM-style port (Pdfbox-IKVM / Pdfbox NuGet):
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.text;
using java.io;
using System;

class Program
{
    static void Main()
    {
        // Java-style API surfaces directly through IKVM
        PDDocument document = PDDocument.load(new File("document.pdf"));
        try
        {
            PDFTextStripper stripper = new PDFTextStripper();
            string text = stripper.getText(document);
            Console.WriteLine(text);
        }
        finally
        {
            document.close();
        }
    }
}
