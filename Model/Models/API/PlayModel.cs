using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Model.API
{
    public class PlayModel
    {


        public string Let1Id { get; set; }

        public int? Let1MainSort { get; set; }

        public string Let1OddsSort { get; set; }


        /// <summary>
        /// 队1让球数
        /// </summary>
        public string Let1Text { get; set; }
        /// <summary>
        /// 队1让球赔率
        /// </summary>
        public decimal? Let1Odds { get; set; }

        public string Let2Id { get; set; }

        public int? Let2MainSort { get; set; }

        public string Let2OddsSort { get; set; }

        /// <summary>
        /// 队2让球数
        /// </summary>
        public string Let2Text { get; set; }
        /// <summary>
        /// 队2让球赔率
        /// </summary>
        public decimal? Let2Odds { get; set; }

        public string Bs1Id { get; set; }

        public int? Bs1MainSort { get; set; }

        public string Bs1OddsSort { get; set; }

        /// <summary>
        /// 队1大小球数
        /// </summary>
        public string Bs1Text { get; set; }
        /// <summary>
        /// 队1大小球赔率
        /// </summary>
        public decimal? Bs1Odds { get; set; }


        public string Bs2Id { get; set; }

        public int? Bs2MainSort { get; set; }

        public string Bs2OddsSort { get; set; }

        /// <summary>
        /// 队2大小球数
        /// </summary>
        public string Bs2Text { get; set; }
        /// <summary>
        /// 队2大小球赔率
        /// </summary>
        public decimal? Bs2Odds { get; set; }


        public string Win1Id { get; set; }

        public int? Win1MainSort { get; set; }

        public string Win1OddsSort { get; set; }

        /// <summary>
        /// 队1赢赔率
        /// </summary>
        public decimal? Win1Odds { get; set; }

        public string Win1Text { get; set; }


        public string Win2Id { get; set; }

        public int? Win2MainSort { get; set; }

        public string Win2OddsSort { get; set; }

        /// <summary>
        /// 队2赢赔率
        /// </summary>
        public decimal? Win2Odds { get; set; }
        public string Win2Text { get; set; }

        public string TieId { get; set; }

        public int? TieMainSort { get; set; }

        public string TieOddsSort { get; set; }

        /// <summary>
        /// 打和赔率
        /// </summary>
        public decimal? TieOdds { get; set; }

        public string TieText { get; set; }


        public string Let1BetCode { get; set; }
        public string Let2BetCode { get; set; }
        public string Bs1BetCode { get; set; }
        public string Bs2BetCode { get; set; }
        public string Win1BetCode { get; set; }
        public string Win2BetCode { get; set; }
        public string TieBetCode { get; set; }
    }
}