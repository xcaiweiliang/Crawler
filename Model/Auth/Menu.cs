using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Menu
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }
        [Required]
        public string Show { get; set; }
        public bool IsShow { get { return Show == "1"; } set { Show = value ? "1" : "0"; } }

    }
}
