namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class A_LeagueMatchTypeMap : EntityTypeConfiguration<A_LeagueMatch>
    {
        public A_LeagueMatchTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("A_LeagueMatch");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Season).HasColumnName("Season");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.BeginDate).HasColumnName("BeginDate");
            this.Property(t => t.EndDate).HasColumnName("EndDate");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            this.Property(t => t.Remark).HasColumnName("Remark");
            this.Property(t => t.ModifyTime).HasColumnName("ModifyTime");
        }
    }
}
