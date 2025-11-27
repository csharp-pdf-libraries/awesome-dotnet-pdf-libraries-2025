// NuGet: Install-Package GdPicture.NET
using GdPicture14;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            pdf.LoadFromFile("input.pdf", false);
            
            for (int i = 1; i <= pdf.GetPageCount(); i++)
            {
                pdf.SelectPage(i);
                pdf.SetTextColor(Color.Red);
                pdf.SetTextSize(48);
                pdf.DrawText("CONFIDENTIAL", 200, 400);
            }
            
            pdf.SaveToFile("watermarked.pdf");
        }
    }
}