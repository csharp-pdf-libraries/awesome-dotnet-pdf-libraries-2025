// NuGet: Install-Package ComPDFKit.NetCore
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("input.pdf");
        
        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.PageAtIndex(i);
            var editor = page.GetEditor();
            editor.BeginEdit(CPDFEditType.EditText);
            
            var textArea = editor.CreateTextArea();
            textArea.SetText("CONFIDENTIAL");
            textArea.SetFontSize(48);
            textArea.SetTransparency(128);
            
            editor.EndEdit();
            page.Release();
        }
        
        document.WriteToFilePath("watermarked.pdf");
        document.Release();
    }
}