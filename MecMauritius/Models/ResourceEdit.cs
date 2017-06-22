using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    /**
     * Displays information about a resource 
     * Allows educators / Admins to modify details of resources uploaded by them
     */
    public class ResourceEdit
    {
        public string ResourceID { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
    }
}