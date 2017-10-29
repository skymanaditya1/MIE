namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserSchoolZone")]
    public partial class UserSchoolZone
    {
        [Key]
        public string UserId { get; set; }

        public int School { get; set; }

        public int Zone { get; set; }

        [StringLength(255)]
        public string categoryid { get; set; }

        [StringLength(255)]
        public string userroleid { get; set; }
    }
}
