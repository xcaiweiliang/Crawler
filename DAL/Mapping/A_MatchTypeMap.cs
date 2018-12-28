namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class A_MatchTypeMap : EntityTypeConfiguration<A_Match>
    {
        public A_MatchTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("A_Match");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.LeagueMatchID).HasColumnName("LeagueMatchID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.HomeTeamID).HasColumnName("HomeTeamID");
            this.Property(t => t.VisitingTeamID).HasColumnName("VisitingTeamID");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            this.Property(t => t.LastMenuType).HasColumnName("LastMenuType");
            this.Property(t => t.SP_GameStartTime).HasColumnName("SP_GameStartTime");
            this.Property(t => t.GameStartTime).HasColumnName("GameStartTime");
            this.Property(t => t.GameEndTime).HasColumnName("GameEndTime");
            this.Property(t => t.HomeTeamScore).HasColumnName("HomeTeamScore");
            this.Property(t => t.HomeTeamInning).HasColumnName("HomeTeamInning");
            this.Property(t => t.HomeTeamSet).HasColumnName("HomeTeamSet");
            this.Property(t => t.VisitingTeamScore).HasColumnName("VisitingTeamScore");
            this.Property(t => t.VisitingTeamInning).HasColumnName("VisitingTeamInning");
            this.Property(t => t.VisitingTeamSet).HasColumnName("VisitingTeamSet");
            this.Property(t => t.IsStart).HasColumnName("IsStart");
            this.Property(t => t.MatchType).HasColumnName("MatchType");
            this.Property(t => t.Timing).HasColumnName("Timing");
            this.Property(t => t.StatusText).HasColumnName("StatusText");
            this.Property(t => t.IsEnd).HasColumnName("IsEnd");
            this.Property(t => t.ExistLive).HasColumnName("ExistLive");
            this.Property(t => t.ModifyTime).HasColumnName("ModifyTime");
            this.Property(t => t.IsLock).HasColumnName("IsLock");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");

        }
    }
}
