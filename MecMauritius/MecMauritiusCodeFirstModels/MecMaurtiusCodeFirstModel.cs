namespace MecMauritius.MecMauritiusCodeFirstModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class MecMauDb : MecMaurtiusCodeFirstModel
    {
        public MecMauDb() : base()
        {
            
        }
    }

    public partial class MecMaurtiusCodeFirstModel : DbContext
    {
        public MecMaurtiusCodeFirstModel()
            : base("name=MecMaurtiusCodeFirstModel")
        {
        }

        public virtual DbSet<Cours> Courses { get; set; }
        public virtual DbSet<EducatorConfirmation> EducatorConfirmations { get; set; }
        public virtual DbSet<EducatorSector> EducatorSectors { get; set; }
        public virtual DbSet<FileNameExtensionsHandled> FileNameExtensionsHandleds { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<Query> Queries { get; set; }
        public virtual DbSet<ReportAbuse> ReportAbuses { get; set; }
        public virtual DbSet<ResourceReview> ResourceReviews { get; set; }
        public virtual DbSet<ResourceType> ResourceTypes { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<tblEmployee> tblEmployees { get; set; }
        public virtual DbSet<UserSchoolZone> UserSchoolZones { get; set; }
        public virtual DbSet<ZONE> ZONES { get; set; }
        public virtual DbSet<ZonesSchool> ZonesSchools { get; set; }
        public virtual DbSet<DigitalResource> DigitalResources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EducatorSector>()
                .Property(e => e.Sector)
                .IsUnicode(false);

            modelBuilder.Entity<Query>()
                .Property(e => e.query1)
                .IsUnicode(false);

            modelBuilder.Entity<ReportAbuse>()
                .Property(e => e.SenderMail)
                .IsUnicode(false);

            modelBuilder.Entity<ReportAbuse>()
                .Property(e => e.ReportName)
                .IsUnicode(false);

            modelBuilder.Entity<ReportAbuse>()
                .Property(e => e.ReportDescription)
                .IsUnicode(false);

            modelBuilder.Entity<ReportAbuse>()
                .Property(e => e.ReportFeedback)
                .IsUnicode(false);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.User_ID)
                .IsUnicode(false);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Resource_ID)
                .IsUnicode(false);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Resource_Description)
                .IsUnicode(false);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Relevance)
                .HasPrecision(2, 1);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Adaptability)
                .HasPrecision(2, 1);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Language)
                .HasPrecision(2, 1);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Pedagogy)
                .HasPrecision(2, 1);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Design)
                .HasPrecision(2, 1);

            modelBuilder.Entity<ResourceReview>()
                .Property(e => e.Total_Rating)
                .HasPrecision(4, 1);

            modelBuilder.Entity<School>()
                .Property(e => e.School1)
                .IsUnicode(false);

            modelBuilder.Entity<Subject>()
                .Property(e => e.education_subject)
                .IsUnicode(false);

            modelBuilder.Entity<UserSchoolZone>()
                .Property(e => e.categoryid)
                .IsUnicode(false);

            modelBuilder.Entity<UserSchoolZone>()
                .Property(e => e.userroleid)
                .IsUnicode(false);

            modelBuilder.Entity<ZONE>()
                .Property(e => e.Zone1)
                .IsUnicode(false);

            modelBuilder.Entity<ZONE>()
                .Property(e => e.Education_Sector)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalResource>()
                .Property(e => e.Grade)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalResource>()
                .Property(e => e.URL)
                .IsUnicode(false);

            modelBuilder.Entity<DigitalResource>()
                .Property(e => e.Thumbnail_URL)
                .IsUnicode(false);
        }
    }
}
