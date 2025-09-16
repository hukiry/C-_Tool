
using ReadExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ExcelExportChartTool.Data
{

    public class ConstConfig
    {
        private const string CONFIG_PATH = "ConfigPath.ini";
        [ConfigEnum(100)]
        private static string TableDirPath = string.Empty;
        [ConfigEnum(ExportDocumentType.Lua)]
        private static string ExportLuaDirPath = string.Empty;
        [ConfigEnum(ExportDocumentType.CCharp)]
        private static string ExportCCharpDirPath = string.Empty;
		[ConfigEnum(ExportDocumentType.CCharpTxt)]
		private static string ExportCCharpTxtDirPath = string.Empty;
		[ConfigEnum(ExportDocumentType.Xml)]
        private static string ExportXmlDirPath = string.Empty;
		[ConfigEnum(ExportDocumentType.MapLua)]
		private static string ExportMapDirPath = string.Empty;
		[ConfigEnum(ExportDocumentType.MapTxt)]
		private static string ExportMapEditorDirPath = string.Empty;

		private static Dictionary<int, FieldInfo> dicInfo = new Dictionary<int, FieldInfo>();

        /// <summary>
        /// 获取目录l路径
        /// </summary>
        public static string GetTableDirPath => (string)dicInfo[100].GetValue(typeof(ConstConfig));
        /// <summary>
        /// 写入目录路径
        /// </summary>
        public static string GetLuaDirPath => GetValue(ExportDocumentType.Lua);
        /// <summary>
        /// 写入目录路径
        /// </summary>
        public static string GetCCharpDirPath => GetValue(ExportDocumentType.CCharp);

		/// <summary>
		/// 写入目录路径
		/// </summary>
		public static string GetCCharpTxtDirPath => GetValue(ExportDocumentType.CCharpTxt);
		/// <summary>
		/// 写入目录路径
		/// </summary>
		public static string GetXmlDirPath => GetValue(ExportDocumentType.Xml);

		public static string GetMapDirPath => GetValue(ExportDocumentType.MapLua);
		public static string GetMapEditorDirPath => GetValue(ExportDocumentType.MapTxt);

		private const string HELPINFO = @"
	--------------------------ConfigPath 配置文件----------------------------------
	|	1，在导表工具应用程序所在路径下：创建文本文件 ConfigPath.ini
	|	2，必须配置的字段：TableDirPath=
	|	3, 可选配置的字段：ExportLuaDirPath=, ExportCCharpDirPath=,
	|	           ExportXmlDirPath=, ExportMapDirPath=, ExportMapEditorDirPath=
	|	4，字段配置注释：以#号开头
	|	5，配置例子：
	|		#Excel 表目录路径
	|		TableDirPath		= ..\Table
	------------------------------------------------------------------------------
";
		public static void InitConfig()
        {
			InitField();
            string filePath =Path.Combine( Application.StartupPath , CONFIG_PATH);
			if (File.Exists(filePath))
			{
				string[] lines = File.ReadAllLines(filePath);
				foreach (var line in lines)
				{
					string[] array = line.Split('=');
					if (array != null && array.Length >= 2)
					{
						dicInfo.Values.First(p => p.Name == array[0].Trim())?.SetValue(typeof(ConstConfig), array[1].Trim());
					}
				}
			}
			else
			{
				List<string> temp = new List<string>();
				temp.Add("#Excel 加载表目录路径");
				temp.Add("TableDirPath		= ../ReadExcel/ExcelTable");
				temp.Add("#导出Lua配置表的目录");
				temp.Add("ExportLuaDirPath	= ");
				temp.Add("#导出Map数据的目录");
				temp.Add("ExportMapDirPath	= ");
				temp.Add("#导出c#配置表的目录");
				temp.Add("ExportCCharpDirPath = ");
				temp.Add("#导出c#-txt配置表的目录");
				temp.Add("ExportCCharpTxtDirPath = ");
				temp.Add("#导出XML配置表的目录");
				temp.Add("ExportXmlDirPath	= ");
				temp.Add("#导出Map 编辑器查看数据的目录");
				temp.Add("ExportMapEditorDirPath =");
				File.WriteAllLines(filePath, temp.ToArray());
				LogManager.Log(ConsoleColor.Yellow, HELPINFO);
			}
        }

		private static string GetValue(ExportDocumentType exportDocumentType)
		{
			try
			{
				string dirPath = (string)dicInfo[(int)exportDocumentType].GetValue(typeof(ConstConfig));
				if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
				return dirPath;
			}
			catch (Exception ex)
			{
				LogManager.Log(ConsoleColor.Cyan, HELPINFO);
				LogManager.Log(ConsoleColor.Red, ex.ToString());
				return null;
			}
		}

        private static void InitField()
        {
            FieldInfo[] fieldInfos = typeof(ConstConfig).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var item in fieldInfos)
            {
                ConfigEnumAttribute configEnum = item.GetCustomAttribute<ConfigEnumAttribute>();
                if (configEnum)
                {
                    dicInfo[configEnum.exportDocumentType] = item;
                }
            }
        }

		public  static void ExportFileConifg()
		{
			var arry =typeof(ExportDocumentType).GetEnumValues();
			foreach (ExportDocumentType documentType in arry)
			{
				switch (documentType)
				{
					case ExportDocumentType.Lua:
						ExportLuaConfig();
						break;
					case ExportDocumentType.CCharp:
						ExportCCharpConfig();
						break;
					case ExportDocumentType.Xml:
						ExportXmlConfig();
						break;
					case ExportDocumentType.MapLua:
						ExportMapConfig();
						break;
				}
			}
		}

		public static void AddLuaData(Conifg_data conifg_Data)
		{
			luaCofig[conifg_Data.taleName] = conifg_Data;
		}

		private static Dictionary<string, Conifg_data> luaCofig = new Dictionary<string, Conifg_data>();
		private static void ExportLuaConfig()
		{
			if (string.IsNullOrEmpty(GetLuaDirPath)) return;

			List<string> lines = new List<string>();
			lines.Add($"");
			lines.Add($"---athor:hukiry");
			lines.Add($"---this class was generated by tools，Please Don't Modify! ");
			lines.Add($"");
			lines.Add($"require('LuaConfig.ConfigurationBase')");
			lines.Add($"");
			lines.Add($"---@class SingleConfig");
			lines.Add("SingleConfig = {}");

			lines.Add($"local m_list = {{}}");
			lines.Add($"---@private");
			lines.Add($"function SingleConfig._ImportClass(className, classPath)");
			lines.Add($"\tif m_list[className] == nil then");
			lines.Add($"\t\tm_list[className] = require(classPath).New()");
			lines.Add($"\tend");
			lines.Add($"\treturn m_list[className]");
			lines.Add($"end");
			string pathName = GetLuaDirPath.Substring(GetLuaDirPath.Replace('\\', '/').LastIndexOf('/')+1);
			var configList = luaCofig.Values.ToList();
			int length = configList.Count;
			for (int i = 0; i < length; i++)
			{
				lines.AddRange(configList[i].ToString(pathName));
			}
			string singlecofigFile = Path.Combine(GetLuaDirPath, "SingleConfig.lua");
			
			File.WriteAllLines(singlecofigFile, lines.ToArray());
			luaCofig.Clear();
		}

		public static void AddCCharpData(Conifg_data conifg_Data)
		{
			cCharpofig[conifg_Data.taleName] = conifg_Data;
		}

		private static Dictionary<string, Conifg_data> cCharpofig = new Dictionary<string, Conifg_data>();

		private static void ExportCCharpConfig()
		{
			string singlecofigFile = Path.Combine(ConstConfig.GetCCharpDirPath, "AssetDataTableEditor.cs");
			List<string> lines = new List<string>();
			lines.Add("public class AssetDataTableEditor{");
			lines.Add("#if UNITY_EDITOR");
			lines.Add("	[UnityEditor.MenuItem(\"Hukiry/Data/CreateTable\")]");
			lines.Add("#endif");
			lines.Add("	public static void Init(){");
			foreach (var item in cCharpofig.Values)
            {
				lines.Add($"		AssetData.Init<{item.fileName}>();");
			}
			lines.Add("	}");
			lines.Add("}");
			File.WriteAllLines(singlecofigFile, lines.ToArray());
		}

		private static void ExportXmlConfig()
		{

		}

		private static void ExportMapConfig()
		{

		}

	}


	public class Conifg_data
	{
		public string fileName;
		public string taleName;
		public string excelFileName;

		public  string[] ToString(string topDirName)
		{
			List<string> lines = new List<string>();
			lines.Add($"");
			lines.Add($"---{excelFileName}");
			lines.Add($"---@return {fileName}");
			lines.Add($"function SingleConfig.{taleName}()");
			lines.Add($"\treturn SingleConfig._ImportClass('{fileName}', '{topDirName}.{taleName}.{fileName}')");//新增
			lines.Add($"end");
			return lines.ToArray();
		}
	}
}
