using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class ResourceType
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

   public class ResourceTypeDBContext : DbContext
    {
         public ResourceTypeDBContext() : base("DBConnectionString") { }

        public DbSet<ResourceType> ResourceTypes { get; set; }
    }

}