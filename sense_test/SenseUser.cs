using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sense_test
{
    class SenseUser
    {
        public SenseUser()
        {
            id = "00000000-0000-0000-0000-000000000000";
        }
        public string id { get; set; }
        public string userId { get; set; }
        public string userDirectory { get; set; }
        public string name { get; set; }
    }
}
