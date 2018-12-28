using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 联赛
    /// </summary>
    public class LeagueMatch
    {
        public LeagueMatch()
        {
            this.MatchList = new List<Match>();
            this.TeamList = new List<Team>();
        }
        /// <summary>
        /// 联赛名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 比赛列表
        /// </summary>
        public List<Match> MatchList { get; set; }
        /// <summary>
        /// 队伍列表
        /// </summary>
        public List<Team> TeamList { get; set; }
    }
}
