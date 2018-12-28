using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_SB
{
    /// <summary>
    /// 总入球
    /// </summary>
    public class OddsZRQ
    {
        /// <summary>
        /// 1:全场，2：上半场，3：下半场
        /// </summary>
        public int type { get; set; }      
        /// <summary>
        /// 总入球数
        /// </summary>
        public string Text_Goals { get; set; }
        /// <summary>
        /// 赔率
        /// </summary>
        public string Odds_ZRQ { get; set; }
        /// <summary>
        /// disable
        /// </summary>
        public string IsDisable { get; set; }
    }
}
