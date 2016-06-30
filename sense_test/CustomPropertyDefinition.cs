using System;
using System.Collections.Generic;

namespace sense_test
{
    class CustomPropertyDefinition
    {
        public string id { get; set; }
        public DateTime modifiedDate { get; set; }
        public string name { get; set; }
        public IList<string> choiceValues { get; set; }
    }
}
