using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr.Models
{
    public class GuardianRuin
    {
        public int RuinId { get; set; }
        public string SystemName { get; set; }
        public string BodyName { get; set; }

        public string RuinTypeName { get; set; }
        public string EdsmSystemLink { get; set; }
        public string EdsmBodyLink { get; set; }

        public decimal[] Coordinates { get; set; }
    }
}
