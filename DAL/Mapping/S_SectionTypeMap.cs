namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class S_SectionTypeMap : EntityTypeConfiguration<S_Section>
    {
        public S_SectionTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("S_Section");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.Code).HasColumnName("Code");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Sort).HasColumnName("Sort");

        }
    }
}
