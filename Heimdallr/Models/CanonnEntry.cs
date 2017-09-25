using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr.Models
{
    public class CanonnEntry
    {
        public int id { get; set; }
        public CanonnTextKey title { get; set; }
        public string link { get; set; }
        public CanonnTextKey content { get; set; }
        public CanonnTextKey excerpt { get; set; }
        public Dictionary<string, CanonnLink[]> _links { get; set; }
        public int featured_media { get; set; }
        public string mediaLink { get; set; }

        public string ItemLink()
        {
            if (link.Length > 0)
            {
                return link;

            }

            if(_links.ContainsKey("self")){
                return _links["self"][0].href;
            }

            return "";
        }
    }
}
