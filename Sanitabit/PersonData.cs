using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanitabit
{
    public class PersonData
    {
        public enum Sex { Male, Female };

        public string Name { get; set; }
        public string Age { get; set; }
        public Sex SetSex { get; set; }
    }
}
