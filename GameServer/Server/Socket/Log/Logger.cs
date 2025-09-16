using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{  /// <summary>
   /// 日志类型。
   /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 不使用日志类输出
        /// </summary>
        None = 0,

        /// <summary>
        /// 更为详细的步骤型日志输出
        /// </summary>
        Trace = 1,

        /// <summary>
        /// 调试信息日志
        /// </summary>
        Debug = 2,

        /// <summary>
        /// 消息类日志输出
        /// </summary>
        Information = 4,

        /// <summary>
        /// 警告类日志输出
        /// </summary>
        Warning = 8,

        /// <summary>
        /// 错误类日志输出
        /// </summary>
        Error = 16,

        /// <summary>
        /// 不可控中断类日输出
        /// </summary>
        Critical = 32
    }

    public class Logger
    {
        /// <summary>
        /// 初始化一个日志记录器
        /// </summary>
        public Logger()
        {
            this.m_consoleForegroundColor = Console.ForegroundColor;
            this.m_consoleBackgroundColor = Console.BackgroundColor;
        }

        private readonly ConsoleColor m_consoleForegroundColor;
        private readonly ConsoleColor m_consoleBackgroundColor;

        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void WriteLog(LogType logType, object source, string message, Exception exception)
        {
            if (this.IsEnable)
            {
                lock (typeof(Logger))
                {
                    Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
                    Console.Write(" | ");
                    switch (logType)
                    {
                        case LogType.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case LogType.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;

                        case LogType.Information:
                        default:
                            Console.ForegroundColor = this.m_consoleForegroundColor;
                            break;
                    }
                    Console.Write(logType.ToString());
                    Console.ForegroundColor = this.m_consoleForegroundColor;
                    Console.Write(" | ");
                    Console.Write(message);

                    if (exception != null)
                    {
                        Console.Write(" | ");
                        Console.Write($"【异常消息】：{exception.Message}");
                        Console.Write($"【堆栈】：{(exception == null ? "未知" : exception.StackTrace)}");
                    }
                    Console.WriteLine();

                    Console.ForegroundColor = this.m_consoleForegroundColor;
                    Console.BackgroundColor = this.m_consoleBackgroundColor;
                }
            }
        }

        public void Log(LogType logType, object source, string message=null, Exception exception=null)
        {
           
            this.WriteLog(logType, source, message, exception);
            
        }
    }
}
