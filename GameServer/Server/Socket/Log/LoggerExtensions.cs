
using System;

namespace Server
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public static class LoggerExtensions
    {
      
        #region 日志

        /// <summary>
        /// 输出中断日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Critical(this Logger logger, string msg)
        {
            logger.Log(LogType.Critical, null, msg, null);
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Debug(this Logger logger, string msg)
        {
            logger.Log(LogType.Debug, null, msg, null);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Error(this Logger logger, string msg)
        {
            logger.Log(LogType.Error, null, msg, null);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Error(this Logger logger, object source, string msg)
        {
            logger.Log(LogType.Error, source, msg, null);
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        public static void Exception(this Logger logger, Exception ex)
        {
            logger.Log(LogType.Error, null, ex.Message, ex);
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="ex"></param>
        public static void Exception(this Logger logger, object source, Exception ex)
        {
            logger.Log(LogType.Error, source, ex.Message, ex);
        }

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Info(this Logger logger, string msg)
        {
            logger.Log(LogType.Information, null, msg, null);
        }

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Info(this Logger logger, object source, string msg)
        {
            logger.Log(LogType.Information, source, msg, null);
        }

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        [Obsolete("该方法已被弃用，请使用“Info”替代。")]
        public static void Message(this Logger logger, string msg)
        {
            logger.Log(LogType.Information, null, msg, null);
        }

        /// <summary>
        /// 输出消息日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        [Obsolete("该方法已被弃用，请使用“Info”替代。")]
        public static void Message(this Logger logger, object source, string msg)
        {
            logger.Log(LogType.Information, source, msg, null);
        }

        /// <summary>
        /// 输出详细日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Trace(this Logger logger, string msg)
        {
            logger.Log(LogType.Trace, null, msg, null);
        }

        /// <summary>
        /// 输出警示日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="msg"></param>
        public static void Warning(this Logger logger, string msg)
        {
            logger.Log(LogType.Warning, null, msg, null);
        }

        /// <summary>
        /// 输出警示日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="source"></param>
        /// <param name="msg"></param>
        public static void Warning(this Logger logger, object source, string msg)
        {
            logger.Log(LogType.Warning, source, msg, null);
        }

        #endregion 日志
    }
}