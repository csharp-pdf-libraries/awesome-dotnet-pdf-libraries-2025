// NuGet: Install-Package Rotativa.Core
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class UrlPdfController : Controller
    {
        public async Task<IActionResult> ConvertUrlToPdf()
        {
            // Rotativa works within MVC framework and returns ActionResult
            return new UrlAsPdf("https://www.example.com")
            {
                FileName = "webpage.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
    }
}