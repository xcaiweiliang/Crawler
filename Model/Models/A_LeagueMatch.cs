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
    public partial class A_LeagueMatch
    {
        /// <summary>
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// </summary>
        public string Season { get; set; }
        /// <summary>
        ///��������
        /// </summary>
        public string SportsType { get; set; }
        /// <summary>
        /// </summary>
        public Nullable<DateTime> BeginDate { get; set; }
        /// <summary>
        /// </summary>
        public Nullable<DateTime> EndDate { get; set; }
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
