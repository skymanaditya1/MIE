using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class CategoriesModel
    {
        public Collection<Category> categories { get; set; }
        public Int16 Selected { get; set; }
    }

    public class Category
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
}