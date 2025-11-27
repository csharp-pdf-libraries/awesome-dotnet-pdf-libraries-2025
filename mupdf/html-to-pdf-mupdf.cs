// NuGet: Install-Package MuPDF.NET
using MuPDFCore;
using System.IO;

class Program
{
    static void Main()
    {
        // MuPDF doesn't support HTML to PDF conversion directly
        // You would need to use another library to convert HTML to a supported format first
        // This is a limitation - MuPDF is primarily a PDF renderer/viewer
        
        // Alternative: Use a browser engine or intermediate conversion
        string html = "<html><body><h1>Hello World</h1></body></html>";
        
        // Not natively supported in MuPDF
        throw new NotSupportedException("MuPDF does not support direct HTML to PDF conversion");
    }
}