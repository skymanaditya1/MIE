using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class ResourceReviews
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime TimePosted { get; set; }
        public float Rating { get; set; }
        public string User_ID { get; set; }
    }
}