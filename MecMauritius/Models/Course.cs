using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class Course
    {

[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class CourseDBContext : DbContext
    {
        public CourseDBContext() : base("DBConnectionString") { }

        public DbSet<Course> Courses { get; set; }
    }

}