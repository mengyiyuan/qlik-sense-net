using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sense_test
{
    class SenseStream
    {
        public SenseStream()
        {
            id = "00000000-0000-0000-0000-000000000000";
        }
        public string id { get; set; }        
        public string name { get; set; }
        public DateTime modifiedDate { get; set; }
        public IList<ResourceCustomProperty> customProperties { get; set; }
    }
}
