using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// �淨����
        /// </summary>
        [NotMapped]
        public string BetName { get; set; }
    }
}
