// NuGet: Install-Package ceTe.DynamicPDF.CoreSuite.NET
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Merger;

class Program
{
    static void Main()
    {
        MergeDocument document = new MergeDocument("document1.pdf");
        document.Append("document2.pdf");
        document.Draw("merged.pdf");
    }
}