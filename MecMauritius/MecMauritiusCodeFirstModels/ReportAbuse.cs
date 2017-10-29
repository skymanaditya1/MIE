namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ReportAbuse")]
    public partial class ReportAbuse
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        [StringLength(255)]
        public string SenderMail { get; set; }

        [Required]
        [StringLength(255)]
        public string ReportName { get; set; }

        [Required]
        [StringLength(255)]
        public string ReportDescription { get; set; }

        [StringLength(255)]
        public string ReportFeedback { get; set; }
    }
}
