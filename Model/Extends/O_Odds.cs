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
        /// Íæ·¨Ãû³Æ
        /// </summary>
        [NotMapped]
        public string BetName { get; set; }
    }
}
