using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Common
    {
        
    }
    /// <summary>
    /// 来源平台
    /// </summary>
    public enum SourcePlatformEnum
    {
        /// <summary>
        /// 沙巴
        /// </summary>
        [Description("沙巴")]
        SB,
        /// <summary>
        /// 皇冠
        /// </summary>
        [Description("皇冠")]
        HG,
    }
    /// <summary>
    /// 体育类型枚举
    /// </summary>
    public enum SportsTypeEnum
    {
        /// <summary>
        /// 足球 1
        /// </summary>
        [Description("足球")]
        Football,
        /// <summary>
        /// 篮球 2
        /// </summary>
        [Description("篮球")]
        Basketball,
        /// <summary>
        /// 美式足球/美式橄榄球 8
        /// </summary>
        [Description("美式足球")]
        AmericanFootball,
        /// <summary>
        /// 网球 3
        /// </summary>
        [Description("网球")]
        Tennis,
        /// <summary>
        /// 排球 4
        /// </summary>
        [Description("排球")]
        Volleyball,
        /// <summary>
        /// 棒球 5
        /// </summary>
        [Description("棒球")]
        Baseball,
        /// <summary>
        /// 羽毛球 6
        /// </summary>
        [Description("羽毛球")]
        Badminton,
        /// <summary>
        /// 乒乓球 7
        /// </summary>
        [Description("乒乓球")]
        Pingpong,
        ///// <summary>
        ///// 其他
        ///// </summary>
        //[Description("其他")]
        //Other,
    }
    /// <summary>
    /// 场次枚举
    /// </summary>
    public enum MatchTypeEnum
    {
        /// <summary>
        /// 全场
        /// </summary>
        [Description("全场")]
        Full,
        /// <summary>
        /// 上半场
        /// </summary>
        [Description("上半场")]
        Firsthalf,
        /// <summary>
        /// 下半场
        /// </summary>
        [Description("下半场")]
        Secondhalf,
        /// <summary>
        /// 第一节
        /// </summary>
        [Description("第一节")]
        FirstQuarter,
        /// <summary>
        /// 第二节
        /// </summary>
        [Description("第二节")]
        SecondQuarter,
        /// <summary>
        /// 第三节
        /// </summary>
        [Description("第三节")]
        ThirdQuarter,
        /// <summary>
        /// 第四节
        /// </summary>
        [Description("第四节")]
        FourthQuarter,
        /// <summary>
        /// 加时
        /// </summary>
        [Description("加时")]
        ExtraTime
    }

    public enum SportTabEnum
    {
        /// <summary>
        /// 滚球
        /// </summary>
        [Description("滚球")]
        GunQiu,
        /// <summary>
        /// 今日
        /// </summary>
        [Description("今日")]
        JinRi,
        /// <summary>
        /// 早盘
        /// </summary>
        [Description("早盘")]
        ZaoPan

    }



}
