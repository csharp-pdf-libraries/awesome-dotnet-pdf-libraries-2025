// Apache PDFBox via a .NET port (e.g. Pdfbox-IKVM on nuget.org).
// The Java class org.apache.pdfbox.multipdf.PDFMergerUtility is exposed
// directly through IKVM, so method names stay Java-style (camelCase).
using org.apache.pdfbox.multipdf;
using org.apache.pdfbox.io;
using System;

class Program
{
    static void Main()
    {
        PDFMergerUtility merger = new PDFMergerUtility();
        merger.addSource("document1.pdf");
        merger.addSource("document2.pdf");
        merger.setDestinationFileName("merged.pdf");
        // MemoryUsageSetting governs heap vs temp-file buffering
        merger.mergeDocuments(MemoryUsageSetting.setupMainMemoryOnly());
        Console.WriteLine("PDFs merged");
    }
}
