namespace DAL
{
    using Model;
    using System.Data.Entity.ModelConfiguration;
    internal partial class A_MatchResultTypeMap : EntityTypeConfiguration<A_MatchResult>
    {
        public A_MatchResultTypeMap()
        {
            this.HasKey(t => t.ID);
            this.ToTable("A_MatchResult");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.LeagueMatchID).HasColumnName("LeagueMatchID");
            this.Property(t => t.MatchID).HasColumnName("MatchID");
            this.Property(t => t.HomeTeamID).HasColumnName("HomeTeamID");
            this.Property(t => t.VisitingTeamID).HasColumnName("VisitingTeamID");
            this.Property(t => t.SportsType).HasColumnName("SportsType");
            this.Property(t => t.SourcePlatform).HasColumnName("SourcePlatform");
            this.Property(t => t.GameStartTime).HasColumnName("GameStartTime");
            this.Property(t => t.HomeTeamScore1H).HasColumnName("HomeTeamScore1H");
            this.Property(t => t.HomeTeamScore1Q).HasColumnName("HomeTeamScore1Q");
            this.Property(t => t.HomeTeamScore2Q).HasColumnName("HomeTeamScore2Q");
            this.Property(t => t.HomeTeamScore3Q).HasColumnName("HomeTeamScore3Q");
            this.Property(t => t.HomeTeamScore4Q).HasColumnName("HomeTeamScore4Q");
            this.Property(t => t.HomeTeamScore5Q).HasColumnName("HomeTeamScore5Q");
            this.Property(t => t.HomeTeamScore6Q).HasColumnName("HomeTeamScore6Q");
            this.Property(t => t.HomeTeamScore7Q).HasColumnName("HomeTeamScore7Q");
            this.Property(t => t.HomeTeamScore8Q).HasColumnName("HomeTeamScore8Q");
            this.Property(t => t.HomeTeamScore9Q).HasColumnName("HomeTeamScore9Q");
            this.Property(t => t.HomeTeamScoreEX).HasColumnName("HomeTeamScoreEX");
            this.Property(t => t.VisitingTeamScore1H).HasColumnName("VisitingTeamScore1H");
            this.Property(t => t.VisitingTeamScore1Q).HasColumnName("VisitingTeamScore1Q");
            this.Property(t => t.VisitingTeamScore2Q).HasColumnName("VisitingTeamScore2Q");
            this.Property(t => t.VisitingTeamScore3Q).HasColumnName("VisitingTeamScore3Q");
            this.Property(t => t.VisitingTeamScore4Q).HasColumnName("VisitingTeamScore4Q");
            this.Property(t => t.VisitingTeamScore5Q).HasColumnName("VisitingTeamScore5Q");
            this.Property(t => t.VisitingTeamScore6Q).HasColumnName("VisitingTeamScore6Q");
            this.Property(t => t.VisitingTeamScore7Q).HasColumnName("VisitingTeamScore7Q");
            this.Property(t => t.VisitingTeamScore8Q).HasColumnName("VisitingTeamScore8Q");
            this.Property(t => t.VisitingTeamScore9Q).HasColumnName("VisitingTeamScore9Q");
            this.Property(t => t.VisitingTeamScoreEX).HasColumnName("VisitingTeamScoreEX");
            this.Property(t => t.HomeTeamScore).HasColumnName("HomeTeamScore");
            this.Property(t => t.VisitingTeamScore).HasColumnName("VisitingTeamScore");
            this.Property(t => t.Status).HasColumnName("Status");
            this.Property(t => t.GoalOrder).HasColumnName("GoalOrder");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
        }
    }
}
