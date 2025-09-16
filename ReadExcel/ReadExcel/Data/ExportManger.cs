using ExcelDataReader;
using ReadExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//https://github.com/ExcelDataReader/ExcelDataReader
namespace ExcelExportChartTool.Data
{
	class ExportManger: Singleton<ExportManger>
	{
		private string helpInfo = @"
	--------------------------配表举例说明----------------------------------
	|	A1		B1		C1		D1		| 
	|----------------------------------------------------------------------	
	|	ServerTable	ClientTable	1
	|	列名称		唯一ID	奖励ID
	|	CLIENT		int		int
	|	SERVER		int		int	
	|	KEY		ID		RewardID
	|	no		1		4
	|	end		2		6
	|--------------------------Help---------------------------------------------
	|1，ServerTable = 服务器表名A1	；ClientTable = 客户端表名B1		
	|2，列名称	： 字段描述						
	|3，CLIENT	： 此行为客户端 类型声明				
	|4，SERVER	： 此行为服务端 类型声明				
	|5，KEY		： 字段名						
	|6，none|0|no	： 此行不导出						
	|7，end		： 此行以上的数据被导出					
	|8，C1		:  请在表中 C1 处标记为数字: 
	|		Lua=1, C#后端=2，C#前端=3, map=9, match_lua=10, matchFlag_lua = 11 导出标记的类型
	|9，D1		:  请在表中 D1 处标记为1, 用来合并相同的表	
	|--------------------------------------------------------------------------|
	|注意：字段名或字段类型中的一个不填，则表示此列字段不导出		   |
	----------------------------------------------------------------------------
					";
		private int DelayTime = 1;
		private Dictionary<string, string> filesTableDic = new Dictionary<string, string>();
		private void InitFile()
		{
			
			LogManager.Log(ConsoleColor.Cyan, helpInfo);
			if (!Directory.Exists(ConstConfig.GetTableDirPath))
			{
				LogManager.Log(ConsoleColor.Red, "未配置TableDirPath路径");
				return;
			}
			var filesTable = System.IO.Directory.GetFiles(ConstConfig.GetTableDirPath, "*.*", System.IO.SearchOption.AllDirectories);
			if (filesTable.Length > 0)
			{
				filesTable = filesTable.Where(s => s.EndsWith(".xlsx")
					|| s.EndsWith(".xls")
					|| s.EndsWith(".xlt")
					|| s.EndsWith(".xlsm")
					|| s.EndsWith(".xltm")).Where(s => !Path.GetFileName(s).StartsWith("~$")).ToArray();
				foreach (var item in filesTable)
				{
					filesTableDic[Path.GetFileNameWithoutExtension(item)] = item;
				}
			}
			LogManager.Log(ConsoleColor.Yellow, "初始化文件数量：" + filesTableDic.Count);
		}

		public async void Run()
		{
			ConstConfig.InitConfig();
			InitFile();
			await Task.Run(() =>
			{
				while (this.KillExcel())
				{
					Task.Delay(100);
				}

				if (DelayTime >= 3)
				{
					LogManager.Log(ConsoleColor.Yellow, "表已关闭，继续...");
				}
				Task.Delay(DelayTime);

				int index = 0;
				foreach (var item in filesTableDic.Values)
				{
					if (File.Exists(item))
					{
						index++;
						string fileName = Path.GetFileName(item);
						Stopwatch stopWatch = new Stopwatch();
						stopWatch.Start();
						this.ReadExcel(item, () =>
						{
							LogManager.Log(ConsoleColor.Green, $"{index},文件：{fileName}；用时：{stopWatch.ElapsedMilliseconds}ms");
						});
					}
				}

				ConstConfig.ExportFileConifg();
				LogManager.Log(ConsoleColor.Yellow, "导出完成！ 按下任意键继续... ...");
				
			});
			await Task.Delay(1);
			await Task.Yield();
		}

		ExportBase exportMap = null;
		private void ReadExcel(string FilePath ,Action succAction)
		{
			try
			{
				exportMap = null;
				FileStream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read);
				IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(stream);
				
				DataSet result = excelDataReader.AsDataSet();

				int tableCount = result.Tables.Count;
				for (int i = 0; i < tableCount; i++)
				{
					this.ExportTable(FilePath,result.Tables[i]);
				}
				excelDataReader.Close();
				
				succAction?.Invoke();
				
			}
			catch (Exception ex)
			{
				LogManager.Log(ConsoleColor.Red, ex);
			}
		}

		
		private void ExportTable(string FilePath,DataTable table)
		{
			int columns = table.Columns.Count;
			int rows = table.Rows.Count;
			if (columns >= 3 && rows >= 5)
			{
				//第一行数据
				var export = GetScriptType(table);
				var exportTab = GetExportTableName(table);
				var isSame = IsSameTable(table);
				string workname = table.TableName;

				string fileName = Path.GetFileNameWithoutExtension(FilePath);

				if (!string.IsNullOrEmpty(exportTab.Item1))
				{
					if (export.luaExport == 9)//Lua合并地图导出
					{
						if (exportMap == null)
						{
							exportMap = new ExportMapGrid(workname, exportTab.Item2, fileName);
						}

						if (!string.IsNullOrEmpty(exportTab.Item2) && string.IsNullOrEmpty(exportMap.clientTableName))
						{
							exportMap.clientTableName = exportTab.Item2;
						}
						exportMap.Export(table, isSame);
					}
					else if (export.luaExport == 1)//Lua表导出
					{
						ExportBase exportBase = new ExportLua(workname, exportTab.Item1, fileName);
						exportBase.Export(table, isSame);
					}
					else if (export.luaExport == 10)//三消地图导出
					{
						ExportBase exportBase = new ExportMapMatch3(workname, exportTab.Item2, fileName);
						exportBase.clientTableName = exportTab.Item2;
						exportBase.Export(table, isSame);

					}
					else if (export.luaExport == 11)//三消地图形状导出
					{
						ExportBase exportBase = new ExportMapMatchFlag(workname, exportTab.Item2, fileName);
						exportBase.clientTableName = exportTab.Item2;
						exportBase.Export(table, isSame);

					}
					else if(export.luaExport == 2)
					{
						ExportBase exportBase = new ExportCCharp(workname, exportTab.Item1, fileName);
						exportBase.Export(table, isSame);
					}
					else if (export.luaExport == 3)
					{
						ExportScriptableObject exportBase = new ExportScriptableObject(workname, exportTab.Item1, fileName);
						exportBase.Export(table, isSame);
					}
					else
					{
						if (export.csExport == 1)
						{
							ExportBase exportBase = new ExportCCharp(workname, exportTab.Item2, fileName);
							exportBase.Export(table, isSame);
						}
					}


					//LogManager.Log("请检查Lua！导出标记：" + export.csExport, "导出表名：" + exportTab.Item2, "导出工作表：" + workname, "导出文件名：" + fileName);
				}
				
			}
		}

		private (int luaExport, int csExport) GetScriptType(DataTable table)
		{
			int rowIndex = 0, cellIndex =2, cellIndexB = 3;//c1
			string s = table.Rows[rowIndex][cellIndex].ToString();
			int.TryParse(s, out int result);

			if (table.Columns.Count >= 4)//cs是否导出
			{
				string sB = table.Rows[rowIndex][cellIndexB].ToString();
				int.TryParse(sB, out int resultB);
				return (result, resultB);
			}
			return (result, 0) ;
		}

		private bool IsSameTable(DataTable table)
		{
			if (table.Columns.Count < 4) return false;
			int rowIndex = 0, cellIndex = 3;
			string s = table.Rows[rowIndex][cellIndex].ToString();
			int.TryParse(s, out int result);
			return result==1;
		}

		//1=c 2=s
		private System.Tuple<string, string> GetExportTableName(DataTable table)
		{
			string c = table.Rows[0][1].ToString();
			string s = table.Rows[0][0].ToString();
			return new System.Tuple<string, string>(c, s);
		}

		private bool KillExcel()
		{
			bool isOpenedExcel = false;
			Process[] procs = Process.GetProcessesByName("excel");
			if (procs != null&& procs.Length>0)
			{
				isOpenedExcel = true;
				foreach (Process pro in procs)
				{
					pro?.Kill();//没有更好的方法,只有杀掉进程
					pro.Dispose();
				}
			}

			procs = Process.GetProcessesByName("et");
			if (procs != null && procs.Length > 0)
			{
				foreach (Process pro in procs)
				{
					string keyFileName = pro.MainWindowTitle.Split('.')[0].Trim();
					if (filesTableDic.ContainsKey(keyFileName))
					{
						CheckLog();
						isOpenedExcel = true;
					}
				}
			}
			return isOpenedExcel;
		}

		private bool isLog = false;
		private bool CheckLog()
		{
			if (!isLog)
			{
				isLog = true;
				DelayTime = 3;
				LogManager.Log(ConsoleColor.Red, "当前目录:", ConstConfig.GetTableDirPath);
				LogManager.Log(ConsoleColor.Red, "请关闭当前目录下的表文件后 继续执行!");
			}

			return false;
		}
	}
}