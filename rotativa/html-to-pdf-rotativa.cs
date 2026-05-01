// NuGet: Install-Package Rotativa.AspNetCore  (v1.4.0, last released 2024-11-06; webgio fork)
// Requires wkhtmltopdf.exe in a "Rotativa" folder at the application root.
// Underlying wkhtmltopdf binary was archived 2023-01-02 with unpatched CVE-2022-35583.
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