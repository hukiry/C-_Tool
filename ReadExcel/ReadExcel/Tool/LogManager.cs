using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelExportChartTool
{
    class LogManager
    {
        public static void Log(ConsoleColor consoleColor,params object[] objs)
        {
            string s = "";
            for (int i = 0; i < objs.Length; i++)
            {
                s += objs[i].ToString();
            }
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(s);
        }

        public static void Log( params object[] objs)
        {
            string s = "";
            for (int i = 0; i < objs.Length; i++)
            {
                s += objs[i].ToString()+ "\t" ;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);
        }
    }
}
