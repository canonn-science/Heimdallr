using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Heimdallr.Settings;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace Heimdallr
{
    public class ThargoidSiteData
    {
        private Dictionary<Int32, ThargoidSite> _siteData  = new Dictionary<Int32, ThargoidSite>();

        public ThargoidSiteData(IOptions<ThargoidSiteSettings> thargoidSiteSettings)
        {
            string thargoidCSV = "";
            try
            {
                thargoidCSV = Task.Run(() =>
                {
                    return GetUrlContents(thargoidSiteSettings.Value.resourceLocation);
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
                float Lat = 0;
                float Lng = 0;
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
                        if(data.Length == 12)
                        {
                            ThargoidSite newSite = new ThargoidSite();

                            //us_name,system,x,y,z,planet,lat,lng,active,msg1,msg2,msg3
                            newSite.SiteID = Int32.Parse(Regex.Replace(data[0], "[^0-9]", ""));
                            
                            //us_name,system,x,y,z,planet,lat,lng,active,msg1,msg2,msg3
                            newSite.SystemName = data[1];
                            isValid = isValid & (newSite.SystemName.Length > 0);


                            newSite.BodyName = data[5];
                            isValid = isValid & float.TryParse(data[6], out Lat);
                            isValid = isValid & float.TryParse(data[7], out Lng);

                            newSite.Active = (data[7] == "Y");
                            newSite.MessageSite1 = Int32.Parse(Regex.Replace("0" + data[8], "[^0-9]", ""));
                            newSite.MessageSite2 = Int32.Parse(Regex.Replace("0" + data[9], "[^0-9]", ""));
                            newSite.MessageSite3 = Int32.Parse(Regex.Replace("0" + data[10], "[^0-9]", ""));

                            if (isValid)
                            {
                                if (!_siteData.ContainsKey(newSite.SiteID))
                                {
                                    _siteData.Add(newSite.SiteID, newSite);
                                }
                            }
                        }
                    }
                    
                }
            }

            

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
