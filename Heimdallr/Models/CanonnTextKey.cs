using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr.Models
{
    public class CanonnTextKey
    {
        public string rendered { get; set; }
        [JsonProperty("protected")]
        public bool isProtected { get; set; }

        public override string ToString()
        {
            return rendered;
        }

    }
}