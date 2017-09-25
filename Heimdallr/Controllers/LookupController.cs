﻿using System;
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
using System.Text;
using System.Net;

namespace Heimdallr.Controllers
{
    public class LookupController : Controller
    {
        private readonly IOptions<ThargoidSiteSettings> _thargoidSiteSettings;
        private readonly IOptions<GuardianRuinSettings> _guardianRuinSettings;
        private readonly IOptions<CanonnWebAPISettings> _canonnWebAPISettings;



        public LookupController(IOptions<ThargoidSiteSettings> thargoidSiteSettings, IOptions<GuardianRuinSettings> guardianRuinSettings, IOptions<CanonnWebAPISettings> canonnWebAPISettings)
        {
            _thargoidSiteSettings = thargoidSiteSettings;
            _guardianRuinSettings = guardianRuinSettings;
            _canonnWebAPISettings = canonnWebAPISettings;
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


        private IActionResult CodexPostByPath(string postSlug)
        {
            
            ViewData["Message"] = "Some Codex Post with Slug " + postSlug;
            return View("Codex");
        }

        private IActionResult CodexPageByPath(string pageRef)
        {

            ViewData["Message"] = "Some Codex Page by URL " + pageRef;
            return View("Codex");
        }

        [Route("/Lookup/{*query}")]
        public IActionResult Codex(string query)
        {
            StringBuilder apiCall = new StringBuilder(_canonnWebAPISettings.Value.resourceLocation);
            

            //Ruin map direct link [https://ruins.canonn.technology/#GR25 OR https://ruins.canonn.technology/#GS25]
            MatchCollection thargoidCheck = Regex.Matches(query, ".*ruins.canonn.technology/#(GR|GS)([0-9]+)");
            if (thargoidCheck.Count == 1)
            {
                //Query param is the site number.  
                query = thargoidCheck[0].Groups[2].Value;
                return ThargoidSite(Int32.Parse(query));
            }

            //Codex entry? [https://canonn.science/codex/unknown-probe/]
            MatchCollection codexCheck = Regex.Matches(query, ".*canonn.science/codex/(.*)");

            //Lore entry? [https://canonn.science/lore/]
            MatchCollection loreCheck = Regex.Matches(query, ".*canonn.science/(.*)");


            if (codexCheck.Count == 1)
            {
                //Query param is the codex page (removing any trailing paths)
                query = codexCheck[0].Groups[1].Value.Replace("/", "");

                apiCall.Append("posts?slug=");
                apiCall.Append(WebUtility.UrlEncode(query));
            }else if (loreCheck.Count == 1)
            {
                //Query param is the page path (removing any trailing paths)
                query = loreCheck[0].Groups[1].Value.Replace("/","");

                apiCall.Append("pages?slug=");
                apiCall.Append(WebUtility.UrlEncode(query));

            }else
            {
                //Query param is the search term
                apiCall.Append("posts?search=");
                apiCall.Append(WebUtility.UrlEncode(query));

            }

            string apiJSON = "";
            try
            {
                apiJSON = Task.Run(() =>
                {
                    return GetUrlContents(apiCall.ToString());
                }).Result;
            }
            catch (Exception ex)
            {
                Exception asyncEx = ex.GetBaseException();
                //Error thrown from the backend api such as a 500 when we send an invalid ID?
                if (asyncEx is HttpRequestException)
                {
                    //TODO: Add direct search handling https://canonn.science/?s=example
                    throw new Exception("Unable to find the specified entry on the Canonn website.");
                }

                //No.  Log it
                //TODO: Log exception here
                throw new Exception("Unexpected error when trying to retrieve the entry from the Canonn website");
            }

            CanonnEntry[] canonnEntries = JsonConvert.DeserializeObject<CanonnEntry[]>(apiJSON);

            if(canonnEntries.Length < 1)
            {
                throw new Exception("Unable to find the specified entry on the Canonn website.");
            }

            CanonnEntry entryModel = canonnEntries[0];
            entryModel.mediaLink = "";

            
            //Media item?
            if(entryModel.featured_media > 0)
            {
                string mediaJSON = "";
                StringBuilder mediaCall = new StringBuilder(_canonnWebAPISettings.Value.resourceLocation);
                mediaCall.Append("media/");
                mediaCall.Append(entryModel.featured_media);

                try
                {
                    mediaJSON = Task.Run(() =>
                    {
                        return GetUrlContents(mediaCall.ToString());
                    }).Result;

                    CanonnMedia canonnMedia = JsonConvert.DeserializeObject<CanonnMedia>(mediaJSON);

                    entryModel.mediaLink = canonnMedia.source_url;


                }
                catch (Exception ex)
                {
                    Exception asyncEx = ex.GetBaseException();
                    //Error thrown from the backend api such as a 500 when we send an invalid ID?
                    if (asyncEx is HttpRequestException)
                    {
                        //TODO: Log exception here
                        //Log the error but still render the item
                    }
                }
            }
            


            return View("Codex", entryModel);
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
