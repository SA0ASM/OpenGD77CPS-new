using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGD77CPS.Models
{
    internal class GeneralSettings
    {

        public String Callsign { get; set; } = "";

        public int DMRId { get; set; } = 0;

        public int cpVersion { get; set; } = 0;


        public bool CustomBootScreen { get; set; }

        public String BootTextLine1 { get; set; } = "";
        public String BootTextLine2 { get; set; } = "";


        public GeneralSettings() { }
    }
}
