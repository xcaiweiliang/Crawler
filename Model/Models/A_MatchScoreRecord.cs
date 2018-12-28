using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//=========================By£º
namespace Model
{
    /// <summary>
    ///
    /// </summary>
    public partial class A_MatchScoreRecord
    {
        /// <summary>
        /// </summary>
        public string ID { get; set; }        
        /// <summary>
        /// </summary>
        public string MatchID { get; set; }
        /// <summary>
        /// </summary>
        public string MatchType { get; set; }
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
        ///Ãë
        /// </summary>
        public Nullable<int> Timing { get; set; }
        /// <summary>
        ///×ãÇò¡¢À¶Çò
        /// </summary>
        public string SportsType { get; set; }
        /// <summary>
        /// </summary>
        public string SourcePlatform { get; set; }
        /// <summary>
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
