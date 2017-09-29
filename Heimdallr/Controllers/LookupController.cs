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


        [Route("/QuickTest")]
        public IActionResult QuickTest()
        {
            Console.WriteLine("QuickTest View");
            return View();
        }

        [Route("/Test")]
        public IActionResult Test()
        {
            Console.WriteLine("Test View");
            return View();
        }

        [Route("/Lookup/GS{siteNum:int}")]
        [Route("/Lookup/GR{siteNum:int}")]
        public IActionResult Guardian(int siteNum)
        {
            Console.WriteLine("guardiansite:" + siteNum.ToString());
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
                    throw new ResourceException("Sorry but we're unable to retrieve the specified Guardian Ruin.  Please check that the Ruin ID is correct.","GR" + siteNum);
                }

                //No.  Log it
                //TODO: Log exception here
                throw new ResourceException("There was an unexpected error when trying to retrieve Guardian Ruin", "GR" + siteNum);
            }

            GuardianRuin ruinInfo = JsonConvert.DeserializeObject<GuardianRuin>(guardianJSON);

            return View("Guardian", ruinInfo);
        }


        [Route("/Lookup/US{siteNum:int}")]
        [Route("/Lookup/TS{siteNum:int}")]
        public IActionResult ThargoidSite(int siteNum)
        {
            Console.WriteLine("thargoidsite:" + siteNum.ToString());

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
                throw new ResourceException("Sorry but we're unable to find a matching Thargoid Site", "TS" + siteNum);
            }
            

            return View("ThargoidSite", matchingSite);
        }

        private string WhiteListString(string input,string whitelist = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            StringBuilder sanitizedString = new StringBuilder();
            foreach (char c in input)
            {
                if (whitelist.IndexOf(c) > -1)
                {
                    sanitizedString.Append(c);
                }
            }
            return sanitizedString.ToString();
        }

        [Route("/Lookup/{*query}")]
        public IActionResult Codex(string query)
        {
            if(query == null)
            {
                return View("Index");
            }

            StringBuilder apiCall = new StringBuilder(_canonnWebAPISettings.Value.resourceLocation);

            Console.WriteLine("codex:" + query);

            //Ruin map direct link [https://ruins.canonn.technology/#GR25 OR https://ruins.canonn.technology/#GS25]
            MatchCollection ruinCheck = Regex.Matches(query, ".*ruins.canonn.technology/#(GR|GS)([0-9]+)");
            if (ruinCheck.Count == 1)
            {
                //Query param is the site number.  
                query = ruinCheck[0].Groups[2].Value;

                Int32 guardianId = 0;

                if(!Int32.TryParse(query, out guardianId))
                {
                    throw new ResourceException("Unable to retrieve the Guardian ID due to an invalid ID", query);
                }

                return Guardian(guardianId);
            }      

            //Codex entry? [https://canonn.science/codex/unknown-probe/]
            MatchCollection codexCheck = Regex.Matches(query, ".*canonn.science/codex/(.*)");

            if (codexCheck.Count == 1)
            {
                
                //Query param is the codex page 
                query = codexCheck[0].Groups[1].Value;
                //removing any trailing paths
                query = query.TrimEnd('/');

                //Parse out any paths such as [https://canonn.science/codex/xenobiology/cmdr-panpiper-a-brief-history-of-brain-trees/]
                query = query.Split('/').Last();

                //Cleanup using slug whitelist
                query = WhiteListString(query, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_");

                apiCall.Append("posts?slug=");
                apiCall.Append(WebUtility.UrlEncode(query));
            }
            else
            {
                //Query param is the search term
                
                //Cleanup using search whitelist
                query = WhiteListString(query, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#&()+,-/: _;!@");

                apiCall.Append("posts?categories=2&search=");
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
                    throw new ResourceException("Sorry but we're unable to find the specified entry on the Canonn website.", query);
                }

                //No.  Log it
                //TODO: Log exception here
                throw new ResourceException("There was an unexpected error when trying to retrieve the entry from the Canonn website",query);
            }

            CanonnEntry[] canonnEntries = JsonConvert.DeserializeObject<CanonnEntry[]>(apiJSON);

            if(canonnEntries.Length < 1)
            {
                throw new ResourceException("Sorry but we're unable to find the specified entry on the Canonn website.", query);
            }

            CanonnEntry entryModel = canonnEntries[0];
            entryModel.mediaLink = "";

            
            //Media item?
            if(entryModel.featured_media > 0 )
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
