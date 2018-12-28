using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 比赛
    /// </summary>
    public class Match
    {
        public Match()
        {
            this.FullCourtList = new List<Odds>();
            this.HalfCourtList = new List<Odds>();
            this.CorrectScoreList = new List<OddsBD>();
            this.TotalGoalList = new List<OddsZRQ>();
        }
        /// <summary>
        /// 比赛开始时间
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 比赛进行时间
        /// </summary>
        public string timing { get; set; }
        /// <summary>
        /// 滚球状态
        /// </summary>
        public string statustext { get; set; }
        /// <summary>
        /// 比赛进行场次
        /// </summary>
        public string halftype { get; set; }
        /// <summary>
        /// 滚球
        /// </summary>
        public string GQ { get; set; }
        /// <summary>
        /// 主队
        /// </summary>
        public string HomeTeam { get; set; }
        /// <summary>
        /// 主队得分
        /// </summary>
        public string HomeTeamScore { get; set; }
        /// <summary>
        /// 主局数
        /// </summary>
        public string HomeTeamInning { get; set; }
        /// <summary>
        /// 主盘数
        /// </summary>
        public string HomeTeamSet { get; set; }
        /// <summary>
        /// 客队
        /// </summary>
        public string VisitingTeam { get; set; }
        /// <summary>
        /// 客队得分
        /// </summary>
        public string VisitingTeamScore { get; set; }
        /// <summary>
        /// 客局数
        /// </summary>
        public string VisitingTeamInning { get; set; }
        /// <summary>
        /// 客盘数
        /// </summary>
        public string VisitingTeamSet { get; set; }
        /// <summary>
        /// 全场赔率
        /// </summary>
        public List<Odds> FullCourtList { get; set; }
        /// <summary>
        /// 半场赔率
        /// </summary>
        public List<Odds> HalfCourtList { get; set; }
        /// <summary>
        /// 波胆赔率
        /// </summary>
        public List<OddsBD> CorrectScoreList { get; set; }
        /// <summary>
        /// 半场/全场
        /// </summary>
        public OddsBCQC DoubleResult { get; set; }
        /// <summary>
        /// 总入球赔率
        /// </summary>
        public List<OddsZRQ> TotalGoalList { get; set; }
    }
}
