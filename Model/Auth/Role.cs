using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Model
{
    public class Role
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Display(Name = "����")]
        public string Name { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [Display(Name = "��ע")]
        public string Remark { get; set; }

    }
}
