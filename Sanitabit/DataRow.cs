using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanitabit
{
    public class DataRow
    {
        public DateTime Today { get; set; }
        public DateTime[] TimeList;

        public void setTimeList(int times)
        {
            TimeList=new DateTime[times];
        }
            
        
        

    }
}
