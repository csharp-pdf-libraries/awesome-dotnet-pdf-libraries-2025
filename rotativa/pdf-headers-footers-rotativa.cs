// NuGet: Install-Package Rotativa.Core
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
                CustomSwitches = "--header-center \"Page Header\" --footer-center \"Page [page] of [toPage]\""
            };
        }
    }
}