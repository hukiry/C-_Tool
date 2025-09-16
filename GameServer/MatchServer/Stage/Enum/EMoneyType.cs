namespace Game.Http
{
    public enum EMoneyType : byte
    {
        ///不是货币
        none = 0,
        ///大炮数量：消除列
        gun = 1,
        ///垂子数量：粉碎一个
        hammer = 2,
        ///弓箭数量：消除行
        bows = 3,
        ///帽子数量，随机值
        cap = 4,
        ///炸弹数量：以炸弹当前位置向外扩大2各消除
        bomb = 5,
        ///蜻蜓数量：优先随机查找障碍物消除
        dragonfly = 6,
        ///火箭数量：行或列消除
        rocket = 7,
        ///彩虹球数量：与交换的颜色属性，或随机的颜色属性消除
        rainbowBall = 8,
        ///金币数量：购买5次移动数
        gold = 9,
        ///生命值
        life = 10,

        ///大炮购买次数：消除列
        gunCount = 11,
        ///垂子购买次数：粉碎一个
        hammerCount = 12,
        ///弓箭购买次数：消除行
        bowsCount = 13,
        ///帽子购买次数，随机值
        capCount = 14,
        ///关卡等级
        level = 15,
        /// <summary>
        /// 生命持续时间
        /// </summary>
        lifehour = 16,
        //最大生命是5，奖励可以超过
        lifeMax = 17,
        ///昵称购买次数
        nickCount = 18,
        ///生命购买次数
        lifeCount = 19,
        ///生命购免费次数
        lifeFree = 20,
        ///移动购买次数
        moveCount = 21,
        ///移动次数累计
        passMove = 22,
        //最多可创建的数量
        metaExpendNum = 23,
        //用户积分
        integral = 24

    }
}
