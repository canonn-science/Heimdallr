using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr
{
    public class ThargoidSite
    {
        public int SiteID { get; set; }
        public string SystemName { get; set; }
        public string BodyName { get; set; }

        public bool Active { get; set; }

        public int MessageSite1 { get; set; }
        public int MessageSite2 { get; set; }
        public int MessageSite3 { get; set; }
        
        public decimal[] Coordinates { get; set; }
    }
}
