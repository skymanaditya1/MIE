namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ResourceReview
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(255)]
        public string User_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string Resource_ID { get; set; }

        [StringLength(255)]
        public string Resource_Description { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime Resource_Posted { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Relevance { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Adaptability { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Language { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Pedagogy { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Design { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? Total_Rating { get; set; }
    }
}
