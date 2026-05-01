// NuGet: Install-Package Rotativa.AspNetCore  (v1.4.0, last released 2024-11-06; webgio fork)
// Margins constructor order is (top, right, bottom, left).
// wkhtmltopdf placeholders are lowercase: [page], [topage], [date], [title].
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class HeaderFooterController : Controller
    {
        public async Task<IActionResult> GeneratePdfWithHeaderFooter()
        {
            return new ViewAsPdf("Report")
            {
                PageSize = Size.A4,
                PageMargins = new Margins(20, 10, 20, 10),
                CustomSwitches = "--header-center \"Page Header\" --footer-center \"Page [page] of [topage]\""
            };
        }
    }
}