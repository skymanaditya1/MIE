namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ZONES")]
    public partial class ZONE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ZONE()
        {
            ZonesSchools = new HashSet<ZonesSchool>();
        }

        [Key]
        public int Zone_ID { get; set; }

        [Column("Zone")]
        [Required]
        [StringLength(50)]
        public string Zone1 { get; set; }

        [Required]
        [StringLength(255)]
        public string Education_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ZonesSchool> ZonesSchools { get; set; }
    }
}
