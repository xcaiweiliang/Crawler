using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Model
{
    /// <summary>
    /// 用户
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 平台代码
        /// </summary>
        [Display(Name = "平台代码")]
        //[Required]
        public string PlatCode { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [Display(Name = "账号")]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Display(Name = "密码")]
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        public string Name { get; set; }

        /// <summary>
        /// 是否有效(1正常，0冻结)
        /// </summary>
        public string Enable { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsEnable { get { return this.Enable == "1"; } set { this.Enable = value ? "1" : "0"; } }

    

        /// <summary>
        /// 备注
        /// </summary>
        [Display(Name = "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
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
        /// 用户角色
        /// </summary>
        public string Roles { get; set; }


    }
}
