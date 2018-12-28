namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class O_OddsTypeMap : EntityTypeConfiguration<O_Odds>
    {
        public O_OddsTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("O_Odds");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.LeagueMatchID).HasColumnName("LeagueMatchID");
            this.Property(t => t.MatchID).HasColumnName("MatchID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.BetCode).HasColumnName("BetCode");
            this.Property(t => t.BetExplain).HasColumnName("BetExplain");
            this.Property(t => t.OddsSort).HasColumnName("OddsSort");
            this.Property(t => t.MainSort).HasColumnName("MainSort");
            this.Property(t => t.ReadSort).HasColumnName("ReadSort");
            this.Property(t => t.LastOdds).HasColumnName("LastOdds").HasPrecision(18, 2);
            this.Property(t => t.Odds).HasColumnName("Odds").HasPrecision(18, 2);
            this.Property(t => t.IsLive).HasColumnName("IsLive");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.ModifyTime).HasColumnName("ModifyTime");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            //this.Property(t => t.RowVersion).HasColumnName("RowVersion").IsRowVersion();
        }
    }
}
