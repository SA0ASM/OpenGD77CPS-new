using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGD77CPS.Models
{
    public enum ContactType
    {
        TalkGroup,
        PrivateCall,
        AllCall

    }

    public class Contact
    {
        public String Name { get; set; } = "";

        public uint Number { get; set; }

        public ContactType Type { get; set; }

        public TimeSlot? OverrideTS { get; set; }
        
        public Contact(String name, uint number)
        {
            Name = name;
            Number = number;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
