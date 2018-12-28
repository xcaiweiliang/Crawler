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
    public partial class A_Team
    {
        /// <summary>
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// </summary>
        public string LeagueMatchID { get; set; }
        /// <summary>
        ///×ãÇò¡¢À¶Çò
        /// </summary>
        public string SportsType { get; set; }
        /// <summary>
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// </summary>
        public string SourcePlatform { get; set; }
        /// <summary>
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }
}
