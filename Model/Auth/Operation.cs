using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Model
{
    public class Operation
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ±àÂë
        /// </summary>
        [Display(Name = "±àÂë")]

        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Ãû³Æ")]
        public string Name { get; set; }

    }
}
