using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Id { get; set; }
        public int Permission { get; set; }
        public string Name { get; set; }
        public string Zone { get; set; }
        public string School { get; set; }

        public User() { }

        public User(string email, string id, int permission, string name, string zone, string school)
        {
            Email = email;
            Id = id;
            Permission = permission;
            Name = name;
            Zone = zone;
            School = school;
        }
    }
}