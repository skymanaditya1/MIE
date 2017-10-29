namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DigitalResource
    {
        [Key]
        [Column(Order = 0)]
        public int ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(255)]
        public string Education { get; set; }

        [StringLength(255)]
        public string Grade { get; set; }

        [StringLength(255)]
        public string Subjects { get; set; }

        [StringLength(255)]
        public string Resource_Type { get; set; }

        [StringLength(255)]
        public string URL { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(255)]
        public string Description { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(255)]
        public string UploaderId { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Thumbnail_URL { get; set; }

        public float? Ratings { get; set; }

        [Key]
        [Column(Order = 6)]
        public float NoOfRatings { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Downloads { get; set; }

        [StringLength(255)]
        public string Author { get; set; }
    }
}
