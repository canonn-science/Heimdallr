using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Heimdallr.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Diagnostics;

namespace Heimdallr.Controllers
{
    public class LookupController : Controller
    {
        private readonly IOptions<UnknownSiteSettings> _unknownSiteSettings;
        private readonly IOptions<GuardianRuinSettings> _guardianRuinSettings;
        

        public LookupController(IOptions<UnknownSiteSettings> unknownSiteSettings, IOptions<GuardianRuinSettings> guardianRuinSettings)
        {
            _unknownSiteSettings = unknownSiteSettings;
            _guardianRuinSettings = guardianRuinSettings;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Test")]
        public IActionResult Test()
        {
            return View();
        }

        [Route("/Lookup/GS{siteNum:int}")]
        [Route("/Lookup/GR{siteNum:int}")]
        public IActionResult Guardian(int siteNum)
        {
            ViewData["Message"] = _guardianRuinSettings.Value.resourceLocation ;

            return View("Guardian");
        }


        [Route("/Lookup/US{siteNum:int}")]
        public IActionResult UnknownSite(int siteNum)
        {
            ViewData["Message"] = "Your application description page.";

            return View("UnknownSite");
        }

        [Route("/Lookup/{query}")]
        public IActionResult Codex(string query)
        {
            ViewData["Message"] = "Query for " + query;
            return View("Codex");
        }

        [Route("/Lookup/Error")]
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            return View("Error",feature.Error);
        }
    }
}
