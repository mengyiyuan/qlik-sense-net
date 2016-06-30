using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sense_test
{
    class ResourceCustomProperty
    {
        public string id { get; set; }
        public DateTime modifiedDate { get; set; }
        public string value { get; set; }
        public CustomPropertyDefinition definition { get; set; }
    }
}
