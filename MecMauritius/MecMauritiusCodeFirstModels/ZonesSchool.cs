namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ZonesSchool
    {
        public int ID { get; set; }

        public int? Zone_ID { get; set; }

        public int? School_ID { get; set; }

        public virtual School School { get; set; }

        public virtual ZONE ZONE { get; set; }
    }
}
