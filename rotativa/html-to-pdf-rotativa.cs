// NuGet: Install-Package Rotativa.Core
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class PdfController : Controller
    {
        public async Task<IActionResult> GeneratePdf()
        {
            var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
            
            // Rotativa requires returning a ViewAsPdf result from MVC controller
            return new ViewAsPdf()
            {
                ViewName = "PdfView",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}