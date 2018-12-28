using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

//=========================By£º
namespace Model
{
    /// <summary>
    ///
    /// </summary>
    public partial class O_Odds
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
        ///×ãÇò¡¢À¶Çò
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
        public Nullable<int> ReadSort { get; set; }
        /// <summary>
        /// </summary>
        public Nullable<decimal> LastOdds { get; set; }
        /// <summary>
        /// </summary>
        public decimal Odds { get; set; }
        /// <summary>
        ///1£ºÊÇ   0£º·ñ
        /// </summary>
        public string IsLive { get; set; }
        /// <summary>
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// </summary>
        public DateTime ModifyTime { get; set; }
        /// <summary>
        /// </summary>
        public string SourcePlatform { get; set; }
        /// <summary>
        /// </summary>
        [NotMapped]
        public byte[] RowVersion { get; set; }
    }
}
