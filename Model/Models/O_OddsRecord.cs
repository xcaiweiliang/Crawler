using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//=========================By��
namespace Model
{
    /// <summary>
    ///
    /// </summary>
    public partial class O_OddsRecord
    {
        /// <summary>
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// </summary>
        public string LeagueMatchID { get; set; }
        /// <summary>
        /// </summary>
        public string MatchID { get; set; }
        /// <summary>
        ///��������
        /// </summary>
        public string SportsType { get; set; }
        /// <summary>
        /// </summary>
        public string BetCode { get; set; }
        /// <summary>
        /// </summary>
        public string BetExplain { get; set; }
        /// <summary>
        /// </summary>
        public string OddsSort { get; set; }
        /// <summary>
        /// </summary>
        public Nullable<int> MainSort { get; set; }
        /// <summary>
        /// </summary>
        public decimal Odds { get; set; }
        /// <summary>
        ///1����   0����
        /// </summary>
        public string IsLive { get; set; }
        /// <summary>
        /// </summary>
        public string SourcePlatform { get; set; }
        /// <summary>
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
