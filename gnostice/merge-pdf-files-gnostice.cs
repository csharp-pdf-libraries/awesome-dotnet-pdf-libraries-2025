// NuGet: Install-Package Gnostice.PDFOne.DLL
using Gnostice.PDFOne;
using Gnostice.PDFOne.Document;
using System;

class Program
{
    static void Main()
    {
        PDFDocument doc1 = new PDFDocument();
        doc1.Load("document1.pdf");
        
        PDFDocument doc2 = new PDFDocument();
        doc2.Load("document2.pdf");
        
        PDFDocument mergedDoc = new PDFDocument();
        mergedDoc.Open();
        
        mergedDoc.Append(doc1);
        mergedDoc.Append(doc2);
        
        mergedDoc.Save("merged.pdf");
        
        doc1.Close();
        doc2.Close();
        mergedDoc.Close();
    }
}