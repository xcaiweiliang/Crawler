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
        /// ����
        /// </summary>
        [Display(Name = "����")]

        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "����")]
        public string Name { get; set; }

    }
}
