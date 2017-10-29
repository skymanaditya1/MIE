namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Subject")]
    public partial class Subject
    {
        [Key]
        public int subject_id { get; set; }

        [Required]
        [StringLength(255)]
        public string education_subject { get; set; }
    }
}
