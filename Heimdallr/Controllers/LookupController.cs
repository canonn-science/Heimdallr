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
using System.IO;
using System.Text.RegularExpressions;

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
        [Route("/Lookup/TS{siteNum:int}")]
        public IActionResult ThargoidSite(int siteNum)
        {
            ThargoidSite matchingSite = null;
            string thargoidCSV = "";
            try
            {
                thargoidCSV = Task.Run(() =>
                {
                    return GetUrlContents(_thargoidSiteSettings.Value.resourceLocation);
                }).Result;
            }
            catch (Exception ex)
            {
                Exception asyncEx = ex.GetBaseException();

                //TODO: Log exception here 
                //Rethrow it
                throw;
            }

            using (StringReader reader = new StringReader(thargoidCSV))
            {
                string line;
                decimal Lat = 0;
                decimal Lng = 0;
                bool isValid = true;
                

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("us_name,"))
                    {
                        //Header
                    }
                    else
                    {
                        isValid = true;
                        //Data.  Get the pieces and if valid add a line item 
                        string[] data = line.Split(',');
                        if (data.Length == 12)
                        {
                            ThargoidSite newSite = new ThargoidSite();

                            //us_name,system,x,y,z,planet,lat,lng,active,msg1,msg2,msg3
                            newSite.SiteID = Int32.Parse(Regex.Replace(data[0], "[^0-9]", ""));

                            //us_name,system,x,y,z,planet,lat,lng,active,msg1,msg2,msg3
                            newSite.SystemName = data[1];
                            isValid = isValid & (newSite.SystemName.Length > 0);


                            newSite.BodyName = data[5];
                            isValid = isValid & decimal.TryParse(data[6], out Lat);
                            isValid = isValid & decimal.TryParse(data[7], out Lng);

                            newSite.Coordinates = new decimal[] { 0, 0 };
                            newSite.Coordinates[0] = Lat;
                            newSite.Coordinates[1] = Lng;
                            

                            newSite.Active = (data[8] == "Y");
                            newSite.MessageSite1 = Int32.Parse(Regex.Replace("0" + data[9], "[^0-9]", ""));
                            newSite.MessageSite2 = Int32.Parse(Regex.Replace("0" + data[10], "[^0-9]", ""));
                            newSite.MessageSite3 = Int32.Parse(Regex.Replace("0" + data[11], "[^0-9]", ""));

                            if (isValid)
                            {
                                if(newSite.SiteID == siteNum)
                                {
                                    matchingSite = newSite;
                                }
                            }
                        }
                    }

                }
            }

            if(matchingSite == null)
            {
                throw new Exception("Unable to find a matching Thargoid Site");
            }
            

            return View("ThargoidSite", matchingSite);
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
