// NuGet: Install-Package GrabzIt
using GrabzIt;
using GrabzIt.Parameters;
using System;

class Program
{
    static void Main()
    {
        var grabzIt = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");
        var options = new ImageOptions();
        options.Format = ImageFormat.png;
        options.Width = 800;
        options.Height = 600;
        
        grabzIt.HTMLToImage("<html><body><h1>Hello World</h1></body></html>", options);
        grabzIt.SaveTo("output.png");
    }
}