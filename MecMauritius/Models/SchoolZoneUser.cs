using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class SchoolZoneUser
    {
        public string userid { get; set; }
        public int zone_id { get; set; }
        public int school_id { get; set; }
        public string zone { get; set; }
        public string school { get; set; }
    }
}