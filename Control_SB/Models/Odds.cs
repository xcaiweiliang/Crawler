using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 玩法赔率
    /// </summary>
    public class Odds
    {
        /// <summary>
        /// 1:全场，2：上半场，3：下半场，4：第一节，5：第二节，6：第三节，7：第四节
        /// </summary>
        public int type { get; set; }
        //=====================================独赢
        /// <summary>
        /// 主赢赔率
        /// </summary>
        public string Odds_ZY { get; set; }
        /// <summary>
        /// 客赢赔率
        /// </summary>
        public string Odds_KY { get; set; }
        /// <summary>
        /// 和局赔率
        /// </summary>
        public string Odds_HJ { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable_DY { get; set; }
        //=====================================让球
        /// <summary>
        /// 主让客球数
        /// </summary>
        public string Text_ZRKQ { get; set; }
        /// <summary>
        /// 客让主球数
        /// </summary>
        public string Text_KRZQ { get; set; }
        /// <summary>
        /// 让球主赢赔率
        /// </summary>
        public string Odds_RQZY { get; set; }
        /// <summary>
        /// 让球客赢赔率
        /// </summary>
        public string Odds_RQKY { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable_RQ { get; set; }
        //=====================================大小
        /// <summary>
        /// 大球数
        /// </summary>
        public string Text_DQ { get; set; }
        /// <summary>
        /// 小球数
        /// </summary>
        public string Text_XQ { get; set; }
        /// <summary>
        /// 大球赔率
        /// </summary>
        public string Odds_DQ { get; set; }
        /// <summary>
        /// 小球赔率
        /// </summary>
        public string Odds_XQ { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable_DX { get; set; }
        //=====================================单双
        /// <summary>
        /// 单赔率
        /// </summary>
        public string Odds_D { get; set; }
        /// <summary>
        /// 双赔率
        /// </summary>
        public string Odds_S { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable_DS { get; set; }


        //=====================================篮球
        //球队得分大小
        /// <summary>
        /// 主队大得分
        /// </summary>
        public string Text_DQZ { get; set; }
        /// <summary>
        /// 主队小得分
        /// </summary>
        public string Text_XQZ { get; set; }
        /// <summary>
        /// 主队大得分赔率
        /// </summary>
        public string Odds_DQZ { get; set; }
        /// <summary>
        /// 主队小得分赔率
        /// </summary>
        public string Odds_XQZ { get; set; }
        /// <summary>
        /// 客队大得分
        /// </summary>
        public string Text_DQK { get; set; }
        /// <summary>
        /// 客队小得分
        /// </summary>
        public string Text_XQK { get; set; }
        /// <summary>
        /// 客队大得分赔率
        /// </summary>
        public string Odds_DQK { get; set; }
        /// <summary>
        /// 客队小得分赔率
        /// </summary>
        public string Odds_XQK { get; set; }

        //=====================================网球 让局
        /// <summary>
        /// 主让客局数
        /// </summary>
        public string Text_ZRKJ { get; set; }
        /// <summary>
        /// 客让主局数
        /// </summary>
        public string Text_KRZJ { get; set; }
        /// <summary>
        /// 让局主赢赔率
        /// </summary>
        public string Odds_RJZY { get; set; }
        /// <summary>
        /// 让局客赢赔率
        /// </summary>
        public string Odds_RJKY { get; set; }
    }
}
