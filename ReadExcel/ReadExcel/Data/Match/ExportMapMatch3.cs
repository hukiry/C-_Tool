using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelExportChartTool.Data
{
	/// <summary>
	/// 导出三消游戏地图
	/// //https://github.com/ExcelDataReader/ExcelDataReader
	/// </summary>
	class ExportMapMatch3 : ExportBase
    {
        public ExportMapMatch3(string worksheet, string clientTableName, string fileName) : base(worksheet, clientTableName, fileName) { }

		private temp_map_match mapData;
		private static List<string> mapList = new List<string>();
		public override void Export(DataTable table, bool isSameTable)
        {
			mapData = new temp_map_match();
			base.Export(table, isSameTable);
			this.SaveMapData(table);
		}

		private void SaveMapData(DataTable table)
		{
			int row = table.Rows.Count;
			int cell = table.Columns.Count;

			mapData.colorMin = CellToInt(table, 0, 3);
			mapData.colorMax = CellToInt(table, 0, 4);

			mapData.totalMove = CellToInt(table, 0, 5);
			mapData.jsonItem = CellToString(table, 0, 6);
			mapData.name = this.clientTableName;

			for (int i = 2; i < row; i++)
			{
				int y = -(i - 6);//正数开始
				for (int j = 1; j < cell; j++)
				{
					int x = j - 5;//负数开始
					if (x <= 4 && y <= 4)
					{
						var text = table.Rows[i][j].ToString().Trim();
						var array = text.Split(',', '_', ';', '=');
						temp_grid_data data = new temp_grid_data();
						data.x = x;
						data.y = y;
						int.TryParse(array[0], out data.itemId);
						if (array.Length >= 3)
						{
							int.TryParse(array[1], out int id);
							int.TryParse(array[2], out int type);
							if (type == 1)
							{
								data.itemId_bottom = id;
							}
							else
							{
								data.itemId_float = id;
							}
						}

						mapData.grids.Add(data);
					}
				}
			}

			string jsonData = JsonConvert.SerializeObject(mapData);
			string _s = "\\\"";
			jsonData = "return \"" + jsonData.Replace("\"", _s) + "\"";
			File.WriteAllText(GetFilePath(), jsonData);

			mapList.Add("	" + this.clientTableName + "=" + (mapList.Count+1) + ",");
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
			return Path.Combine(ConstConfig.GetMapDirPath, "MapLevel.lua");
		}
	}


    [Serializable]
    public class temp_map_match
    {
        public string name;
        public int colorMin;
        public int colorMax;
		public int totalMove;
		public string jsonItem;
		public List<temp_grid_data> grids = new List<temp_grid_data>();
    }

    /// <summary>
    /// 格子数据
    /// </summary>
	[Serializable]
	public struct temp_grid_data
	{
		public int x;//x坐标
		public int y;//y坐标
        public int itemId;
        public int itemId_float;
        public int itemId_bottom;
    }
}
