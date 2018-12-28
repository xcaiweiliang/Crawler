using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//=========================By：
namespace Model
{
    /// <summary>
    ///
    /// </summary>
    public partial class A_Match
    {
        /// <summary>
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// </summary>
        public string LeagueMatchID { get; set; }
        /// <summary>
        ///足球、蓝球
        /// </summary>
        public string SportsType { get; set; }
        /// <summary>
        /// </summary>
        public string HomeTeamID { get; set; }
        /// <summary>
        /// </summary>
        public string VisitingTeamID { get; set; }
        /// <summary>
        /// </summary>
        public string SourcePlatform { get; set; }
        /// <summary>
        ///1：早盘 2：今日赛事 3：滚球
        /// </summary>
        public string LastMenuType { get; set; }
        /// <summary>
        ///平台时区
        /// </summary>
        public Nullable<DateTime> SP_GameStartTime { get; set; }
        /// <summary>
        ///北京时间
        /// </summary>
        public Nullable<DateTime> GameStartTime { get; set; }
        /// <summary>
        /// </summary>
        public Nullable<DateTime> GameEndTime { get; set; }
        /// <summary>
        /// </summary>
        public string HomeTeamScore { get; set; }
        /// <summary>
        /// </summary>
        public string HomeTeamInning { get; set; }
        /// <summary>
        /// </summary>
        public string HomeTeamSet { get; set; }
        /// <summary>
        /// </summary>
        public string VisitingTeamScore { get; set; }
        /// <summary>
        /// </summary>
        public string VisitingTeamInning { get; set; }
        /// <summary>
        /// </summary>
        public string VisitingTeamSet { get; set; }
        /// <summary>
        ///1：是   0：否
        /// </summary>
        public string IsStart { get; set; }
        /// <summary>
        /// </summary>
        public string MatchType { get; set; }
        /// <summary>
        ///秒
        /// </summary>
        public Nullable<int> Timing { get; set; }
        /// <summary>
        /// </summary>
        public string StatusText { get; set; }
        /// <summary>
        ///1：是   0：否
        /// </summary>
        public string IsEnd { get; set; }
        /// <summary>
        ///1：是   0：否
        /// </summary>
        public string ExistLive { get; set; }
        /// <summary>
        /// </summary>
        public DateTime ModifyTime { get; set; }
        /// <summary>
        ///1：是   0：否
        /// </summary>
        public string IsLock { get; set; }
        /// <summary>
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
