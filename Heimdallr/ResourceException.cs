using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heimdallr
{
    public class ResourceException : Exception
    {
        public string queryContext { get; set; }
        public ResourceException() : base() { }
        public ResourceException(string message, string queryContext) : base(message) { this.queryContext = queryContext; }
        public ResourceException(string message, string queryContext, System.Exception inner) : base(message, inner) { this.queryContext = queryContext; }

    }
}
