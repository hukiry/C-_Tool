using System;

namespace Server
{
    class SocketTimer
    {
        System.Timers.Timer timer = null;

        Action<DateTime> timerUpdate;
        /// <summary>
        /// 时间计时器
        /// </summary>
        /// <param name="interval">毫秒</param>
        /// <param name="timerUpdate">更新回到</param>
        public SocketTimer(uint interval, Action<DateTime> timerUpdate)
        {
            this.timerUpdate = timerUpdate;
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpdate);
            timer.AutoReset = true;
        }

        /// <summary>
        /// 启动计时器
        /// </summary>
        public void Start()
        {
            timer.Enabled = true; //是否触发Elapsed事件
            timer.Start();
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop()
        {
            timer.Close();
        }

        /// <summary>
        /// 计时器更新
        /// </summary>
        private void OnTimeUpdate(object sender, System.Timers.ElapsedEventArgs e)
        {
            //做其他事
            timerUpdate?.Invoke(e.SignalTime);
        }
    }
}
