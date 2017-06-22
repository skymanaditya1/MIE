using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class DigitalResources
    {
        public string ResourceTitle { get; set; }
        public string ResourceDescription { get; set; }
        public string ResourceEducation { get; set; }
        public string ResourceGrades { get; set; }
        public string ResourceSubject { get; set; }
        public Uri ResourceUrl { get; set; }
        public string ResourceThumbnail { get; set; }
        public string Downloads { get; set; }
        public string Ratings { get; set; }
        public string ResourceID { get; set; }
    }
}