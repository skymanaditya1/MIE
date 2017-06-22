using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class SiteAdmins
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public SiteAdmins() { }

        public SiteAdmins(string id, string email, string fname, string lname)
        {
            ID = id;
            Email = email;
            FirstName = fname;
            LastName = lname;
        }
    }
}