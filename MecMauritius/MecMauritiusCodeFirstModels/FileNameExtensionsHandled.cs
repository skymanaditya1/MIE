namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FileNameExtensionsHandled")]
    public partial class FileNameExtensionsHandled
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string FileExtension { get; set; }
    }
}
