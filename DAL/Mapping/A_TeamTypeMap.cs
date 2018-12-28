namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class A_TeamTypeMap : EntityTypeConfiguration<A_Team>
    {
        public A_TeamTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("A_Team");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.LeagueMatchID).HasColumnName("LeagueMatchID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            this.Property(t => t.Remark).HasColumnName("Remark");
            this.Property(t => t.ModifyTime).HasColumnName("ModifyTime");
        }
    }
}
