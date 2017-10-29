namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EducatorConfirmation
    {
        [Key]
        public string UserId { get; set; }

        public bool? Denied { get; set; }
    }
}
