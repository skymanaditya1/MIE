using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    // class which gets invoked when resource details are required
    public class ResourceDisplay
    {
        public string Resource_Thumbnail { get; set; }
        public string Resource_Title { get; set; }
        public string Resource_Grade { get; set; }
        public string Resource_Subject { get; set; }
        public string Ratings { get; set; }
        public string Downloads { get; set; }
        public string Download_URL { get; set; }
        public string Description { get; set; }
        public string Resource_ID { get; set; }
        // fields for the feedback / reviews associated with each resource to be added
    }
}