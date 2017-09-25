using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr.Models
{
    public class CanonnLink
    {
        public string embeddable { get; set; }

        public string href { get; set; }
        public string name { get; set; }
        public string taxonomy { get; set; }
        public string templated { get; set; }

        public override string ToString()
        {
            return href;
        }
    }
}
