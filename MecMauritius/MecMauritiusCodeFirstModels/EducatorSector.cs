namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EducatorSector")]
    public partial class EducatorSector
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Sector { get; set; }
    }
}
