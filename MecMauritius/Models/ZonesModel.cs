using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class ZonesModel
    {
        public Collection<Zone> zones { get; set; }
        public Int16 Selected { get; set; }
    }

    public class Zone
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
}