using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class DigitalResource
    {
        public string Name { get; set; }
        public string Education { get; set; }
        public string Grade { get; set; }
        public string Subjects { get; set; }
        public string Resource_Type { get; set; }
        public Uri Url { get; set; }
        public string Description { get; set; }
        public string UploaderId { get; set; }
        public string Title { get; set; }
        public Uri Thumbnail_Url { get; set; }

    }
}