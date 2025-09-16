//using ExcelExportChartTool;
//using ExcelExportChartTool.Data;
//using Microsoft.Office.Interop.Excel;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
////引用Microsoft.Office.Interop.Excel.dll 版本 12.0.0
//namespace ReadExcel
//{
//    public class LoadExcel
//    {

//        private async void OpenExcel()
//		{
//			await KillExcel();
//            Application excel = new Application();//lauch excel application
//            if (excel != null)
//			{
//				int index = 0;
//                foreach (KeyValuePair<int,string> item in filesTableDic)
//                {
//					index++;
//					Stopwatch stopwatch = new Stopwatch();
//					stopwatch.Start();
//                    this.OpenSingleExcel(excel,item.Key, item.Value);
//                    string fileName = Path.GetFileNameWithoutExtension(item.Value);
//                    LogManager.Log(ConsoleColor.Yellow, $"文件{index}：{fileName}  用时:{stopwatch.ElapsedMilliseconds}ms");
//				}
//            }
//			await Task.Yield();
//			excel.Quit(); excel = null;
//			await KillExcel();
//			LogManager.Log(ConsoleColor.Green, "导出完成");
//			GC.Collect();
//        }

//		private async Task KillExcel()
//		{
//			Process[] procs = Process.GetProcessesByName("Excel");
//			if (procs != null)
//			{
//				foreach (Process pro in procs)
//				{
//					pro?.Kill();//没有更好的方法,只有杀掉进程
//					pro.Dispose();
//				}
//			}
//			await Task.Yield();
//		}

//        private async void OpenSingleExcel(Application excel,int key,string strFileName)
//        {
//            object missing = System.Reflection.Missing.Value;
//            excel.Visible = false; excel.UserControl = true;
//            // 以只读的形式打开EXCEL文件
//            Workbook wb = excel.Application.Workbooks.Open(strFileName, missing, true, missing, missing, missing,
//             missing, missing, missing, true, missing, missing, missing, missing, missing);
//            int count = wb.Worksheets.Count;
//            for (int i = 1; i <= count; i++)
//            {
//                //取得第一个工作薄
//                Worksheet ws = (Worksheet)wb.Worksheets.get_Item(i);

//                ReadSingleWorksheet(ws, key, strFileName);
//            }
//			await Task.Yield();
//		}

//        //获取一个工作薄
//        private void ReadSingleWorksheet(Worksheet ws, int key, string strFileName)
//        {
//            //取得总记录行数   (包括标题列)
//            int rowsint = ws.UsedRange.Cells.Rows.Count; //得到行数
//            int columnsint = ws.UsedRange.Cells.Columns.Count;//得到列数
			

//            string rangeS1 = ((char)65).ToString() + 1;
//            int curCell = columnsint > 26 ? (columnsint - 1) % 26 : columnsint;

//            string rangeS2 = ((char)(65 + columnsint - 1)).ToString() + rowsint;
//            if (columnsint > 26)//列数
//            {
//                string redCell = ((char)(65 + (columnsint / 26 - 1))).ToString();
//                rangeS2 = redCell + ((char)(65 + curCell)).ToString() + rowsint;
//            }

//            Range rng = ws.Rows.get_Range(rangeS1, rangeS2);
//            object[,] arryCus = (object[,])rng.Value2;//索引从一开始

//            if (arryCus == null)
//            {
//                return;
//            }

//			ExportManger.instance.CalculateWorkSheet(ws.Name, strFileName, arryCus);
//        }
//    }
//}