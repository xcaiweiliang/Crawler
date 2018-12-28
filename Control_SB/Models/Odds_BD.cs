using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 波胆
    /// </summary>
    public class OddsBD
    {
        /// <summary>
        /// 1:全场，2：上半场，3：下半场
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 主队得分
        /// </summary>
        public int Text_H { get; set; }
        /// <summary>
        /// 客队得分
        /// </summary>
        public int Text_V { get; set; }
        /// <summary>
        /// 赔率
        /// </summary>
        public string Odds_BD { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable { get; set; }
    }
}
