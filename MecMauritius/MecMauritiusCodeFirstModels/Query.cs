namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Query
    {
        public int id { get; set; }

        [Column("query", TypeName = "text")]
        [Required]
        public string query1 { get; set; }
    }
}
