using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MecMauritius.Models
{
    public class SchoolModel
    {
        public Collection<School> Schoollist { get; set; }
        public Int16 Selected { get; set; }
    }

    public class School
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

}