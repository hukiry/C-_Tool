using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelExportChartTool.Data
{
	/*
     return {
	    flagNumbers = {1,1,1,1}
	    map = {
		    1,2,3,4,4,5,5,6,
		    0,0,0,0,0,0,0,0,
	    }
    }
     */
	/// <summary>
	/// 地图配置标记
	/// 1，对应障碍物数量集合
	/// 2，地图集合
	/// //https://github.com/ExcelDataReader/ExcelDataReader
	/// </summary>
	class ExportMapMatchFlag : ExportBase
    {
        public ExportMapMatchFlag(string worksheet, string clientTableName, string fileName) : base(worksheet, clientTableName, fileName) { }
		private static List<string> mapList = new List<string>();
		public override void Export(DataTable table, bool isSameTable)
        {
            base.Export(table, isSameTable);
			this.SaveMapData(table);
		}

		private void SaveMapData(DataTable table)
		{
			int row = table.Rows.Count;
			int cell = table.Columns.Count;
			string mapStr = string.Empty;
			string flags = string.Empty;
			List<int> items = new List<int>() { 0,0,0,0};
			for (int i = 2; i < row; i++)
			{
				int y = i - 2;//正数开始
				for (int j = 1; j < cell; j++)
				{
					int x = j - 1;//负数开始
					if (x <= 8 && y <= 8)
					{
						var text = table.Rows[i][j].ToString().Trim();
						int.TryParse(text, out int flag);
						mapStr = mapStr + flag + ",";
						if (flag <= 4 && flag >= 1)
							items[flag - 1] = items[flag - 1] + 1;
					}
					if (x == 8)
						mapStr = mapStr + "\n		";
				}
			}

			items.ForEach(p=> { flags = flags + p + ","; });
			mapStr = mapStr.TrimEnd(',', '\n', ' ','\t');
			flags = flags.TrimEnd(',');

			string jsonData = @"return {
	flagNumbers = {#1},
	map = {
		#2
	}
}";
			jsonData = jsonData.Replace("#1", flags).Replace("#2", mapStr);
			File.WriteAllText(GetFilePath(), jsonData);

			mapList.Add("	" + this.clientTableName + "=" + (mapList.Count + 1) + ",");
			List<string> temp = new List<string>();
			temp.Add("return {");
			temp.AddRange(mapList);
			temp.Add("}");
			File.WriteAllLines(GetFileListPath(), temp.ToArray());
		}

		private string GetFilePath()
		{
			if (!Directory.Exists(ConstConfig.GetMapDirPath))
			{
				Directory.CreateDirectory(ConstConfig.GetMapDirPath);
			}
			return Path.Combine(ConstConfig.GetMapDirPath, this.clientTableName + ".lua");
		}

		private string GetFileListPath()
		{
			if (!Directory.Exists(ConstConfig.GetMapDirPath))
			{
				Directory.CreateDirectory(ConstConfig.GetMapDirPath);
			}
			return Path.Combine(ConstConfig.GetMapDirPath, "MapShape.lua");
		}
	}
}
