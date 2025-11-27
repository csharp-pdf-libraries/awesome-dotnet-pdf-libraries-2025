// NuGet: Install-Package ComPDFKit.NetCore
using ComPDFKit.PDFDocument;
using System;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.CreateDocument();
        var page = document.InsertPage(0, 595, 842, "");
        
        // ComPDFKit requires manual HTML rendering
        // Native HTML to PDF not directly supported
        var editor = page.GetEditor();
        editor.BeginEdit(CPDFEditType.EditText);
        editor.CreateTextWidget(new System.Drawing.RectangleF(50, 50, 500, 700), "HTML content here");
        editor.EndEdit();
        
        document.WriteToFilePath("output.pdf");
        document.Release();
    }
}