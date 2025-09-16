using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Http
{
    /// <summary>
    /// 系统功能类型
    /// </summary>
    //{ label: '抽卡活动', type: 1 },
    //{ label: '首冲活动', type: 2 },
    //{ label: '宝箱活动', type: 3 },
    //{ label: '礼包活动', type: 4 },
    //{ label: '节日活动', type: 5 },
    //{ label: '签到活动', type: 6 },
    //{ label: '积分活动', type: 7 },
    //{ label: '道具活动', type: 8 },
    //{ label: '通行证', type: 9 },
    public enum SystemFunctionType:int
    {
        none = 0,
        /// <summary>
        /// 抽卡活动
        /// </summary>
        drawcard,
        /// <summary>
        /// 首冲活动
        /// </summary>
        rechargeFirst ,
        /// <summary>
        /// 宝箱活动
        /// </summary>
        Chest ,
        /// <summary>
        /// 礼包活动
        /// </summary>
        gif ,
        /// <summary>
        /// 节日活动
        /// </summary>
        festival ,
        /// <summary>
        /// 签到活动
        /// </summary>
        sign ,
        /// <summary>
        /// 积分活动
        /// </summary>
        integral ,
        /// <summary>
        /// 道具活动
        /// </summary>
        item ,
        /// <summary>
        /// 通行证活动
        /// </summary>
        pass ,

        /// <summary>
        /// 元世界
        /// </summary>
        metaverse 
    }
}
