using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Model
{
    /// <summary>
    /// �û�
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ƽ̨����
        /// </summary>
        [Display(Name = "ƽ̨����")]
        //[Required]
        public string PlatCode { get; set; }

        /// <summary>
        /// �˺�
        /// </summary>
        [Display(Name = "�˺�")]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Display(Name = "����")]
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [Display(Name = "����")]
        public string Name { get; set; }

        /// <summary>
        /// �Ƿ���Ч(1������0����)
        /// </summary>
        public string Enable { get; set; }

        /// <summary>
        /// �Ƿ���Ч
        /// </summary>
        public bool IsEnable { get { return this.Enable == "1"; } set { this.Enable = value ? "1" : "0"; } }

    

        /// <summary>
        /// ��ע
        /// </summary>
        [Display(Name = "��ע")]
        public string Remark { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public System.DateTime CreateTime { get; set; }

        public System.DateTime? LastLoginTime { get; set; }

        [Required]
        public int TryNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime? DisabledTime { get; set; }

        /// <summary>
        /// �û���ɫ
        /// </summary>
        public string Roles { get; set; }


    }
}
