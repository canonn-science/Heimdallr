using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Heimdallr.Settings;
using Heimdallr.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;

namespace Heimdallr.Controllers
{
    public class LookupController : Controller
    {
        private readonly IOptions<ThargoidSiteSettings> _thargoidSiteSettings;
        private readonly IOptions<GuardianRuinSettings> _guardianRuinSettings;
        

        public LookupController(IOptions<ThargoidSiteSettings> thargoidSiteSettings, IOptions<GuardianRuinSettings> guardianRuinSettings)
        {
            _thargoidSiteSettings = thargoidSiteSettings;
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

            string guardianJSON = "";
            try
            {
                guardianJSON = Task.Run(() =>
                {
                    return GetUrlContents(_guardianRuinSettings.Value.resourceLocation + "api/v1/maps/ruininfo/" + siteNum);
                }).Result;
            }
            catch (Exception ex)
            {
                Exception asyncEx = ex.GetBaseException();
                //Error thrown from the backend api such as a 500 when we send an invalid ID?
                if (asyncEx is HttpRequestException){
                    throw new Exception("Unable to retrieve the specified Guardian Ruin.  Please check that the Ruin ID is correct.");
                }

                //No.  Log it
                //TODO: Log exception here
                throw new Exception("Unexpected error when trying to retrieve Guardian Ruin");
            }

            GuardianRuin ruinInfo = JsonConvert.DeserializeObject<GuardianRuin>(guardianJSON);

            return View("Guardian", ruinInfo);
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

        private static async Task<string> GetUrlContents(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage responseMsg = await client.GetAsync(url))
                {
                    responseMsg.EnsureSuccessStatusCode();
                    return responseMsg.Content.ReadAsStringAsync().Result;
                }
                

            }
        }

    }
}
