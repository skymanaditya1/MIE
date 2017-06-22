using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MecMauritius.Models
{
    public class ModifyModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string Firstname { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; }

        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Birthdate { get; set; }

        [Required]
        [Display(Name = "ZoneList")]
        public int Zone { get; set; }

        [Required]
        [Display(Name = "SchoolList")]
        public int School { get; set; }

        [Required]
        [Display(Name = "CategoriesList")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "UserRoles")]
        public string UserRoles { get; set; }
    }
}