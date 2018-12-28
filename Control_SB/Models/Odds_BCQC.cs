using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 半场/全场
    /// </summary>
    public class OddsBCQC
    {
        /// <summary>
        /// 主赢/主赢
        /// </summary>
        public string Odds_HH { get; set; }
        /// <summary>
        /// 和局/主赢
        /// </summary>
        public string Odds_DH { get; set; }
        /// <summary>
        /// 客赢/主赢
        /// </summary>
        public string Odds_VH { get; set; }

        /// <summary>
        /// 主赢/和局
        /// </summary>
        public string Odds_HD { get; set; }
        /// <summary>
        /// 和局/和局
        /// </summary>
        public string Odds_DD { get; set; }
        /// <summary>
        /// 客赢/和局
        /// </summary>
        public string Odds_VD { get; set; }

        /// <summary>
        /// 主赢/客赢
        /// </summary>
        public string Odds_HV { get; set; }
        /// <summary>
        /// 和局/客赢
        /// </summary>
        public string Odds_DV { get; set; }
        /// <summary>
        /// 客赢/客赢
        /// </summary>
        public string Odds_VV { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable { get; set; }
    }
}
