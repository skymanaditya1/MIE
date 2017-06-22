using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class UserRolesModel
    {
        public Collection<UserRoles> roles { get; set; }
        public Int16 Selected { get; set; }
    }

    public class UserRoles
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
}