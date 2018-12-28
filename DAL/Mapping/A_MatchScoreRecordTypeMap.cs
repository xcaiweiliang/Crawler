namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class A_MatchScoreRecordTypeMap : EntityTypeConfiguration<A_MatchScoreRecord>
    {
        public A_MatchScoreRecordTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("A_MatchScoreRecord");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.MatchID).HasColumnName("MatchID");
            this.Property(t => t.MatchType).HasColumnName("MatchType");
            this.Property(t => t.HomeTeamScore).HasColumnName("HomeTeamScore");
            this.Property(t => t.HomeTeamInning).HasColumnName("HomeTeamInning");
            this.Property(t => t.HomeTeamSet).HasColumnName("HomeTeamSet");
            this.Property(t => t.VisitingTeamScore).HasColumnName("VisitingTeamScore");
            this.Property(t => t.VisitingTeamInning).HasColumnName("VisitingTeamInning");
            this.Property(t => t.VisitingTeamSet).HasColumnName("VisitingTeamSet");
            this.Property(t => t.Timing).HasColumnName("Timing");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
        }
    }
}
