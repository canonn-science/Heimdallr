using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Heimdallr.Controllers
{
    public class LookupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Lookup/Test")]
        public IActionResult Test()
        {
            return View();
        }

        [Route("/Lookup/GS{siteNum:int}")]
        public IActionResult Guardian(int siteNum)
        {
            ViewData["Message"] = "Your application description page.";

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

        public IActionResult Error()
        {
            return View();
        }
    }
}
