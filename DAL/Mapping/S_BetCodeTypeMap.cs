namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class S_BetCodeTypeMap : EntityTypeConfiguration<S_BetCode>
    {
        public S_BetCodeTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("S_BetCode");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.Code).HasColumnName("Code");
            this.Property(t => t.CodeName).HasColumnName("CodeName");
            this.Property(t => t.Remark).HasColumnName("Remark");
            this.Property(t => t.SectionCode).HasColumnName("SectionCode");
            this.Property(t => t.Sort).HasColumnName("Sort");
        }
    }
}
