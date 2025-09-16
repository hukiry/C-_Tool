using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
//https://github.com/ExcelDataReader/ExcelDataReader
namespace ExcelExportChartTool.Data
{
    internal class ExportMapGrid : ExportBase
    {
		Dictionary<GridType, DataTable> dicTable = new Dictionary<GridType, DataTable>();
		private temp_exportData mapData = null;
		public ExportMapGrid(string worksheet, string clientTableName, string fileName) : base(worksheet, clientTableName, fileName){
			mapData = new temp_exportData();
		}

		public override void Export(DataTable table, bool isSameTable)
		{
			base.Export(table, isSameTable);
			if (string.IsNullOrEmpty(ConstConfig.GetMapDirPath))
			{
				return;
			}

			GridType gridType = (GridType)CellToInt(table, 0, 1);
			this.SaveMapData(gridType,table);
		}

		private void SaveMapData(GridType gridType, DataTable table)
		{
			int row = table.Rows.Count;
			int cell = table.Columns.Count;

			if (gridType == GridType.Grid)
			{
				(int rowsize,int cellsize) =  (row - 2 , cell - 1);
				int sizeHigth = rowsize % 2 == 1 ? (rowsize - 1) / 2 : rowsize / 2;
				int sizeWidth = cellsize % 2 == 1 ? (cellsize - 1) / 2 : cellsize / 2;
				mapData.sizeWidth = sizeWidth;
				mapData.sizeHigth = sizeHigth;
				mapData.skinIndex = CellToInt(table, 0, 3);
				mapData.spriteAtlasName = CellToString(table, 0, 4);
			}

			for (int i = 2; i < row; i++)
			{
				int x = mapData.sizeHigth - (i-2);
				for (int j = 1; j < cell; j++)
				{
					int y = mapData.sizeWidth - (j - 1);
					var s = table.Rows[i][j].ToString().Trim();
					temp_data_port port = mapData.GetPort(x, y);
					if (port == null)
					{
						port = new temp_data_port();
					}

                    switch (gridType)
                    {
                        case GridType.Grid:
							port.x = x;
							port.y = y;
							int space = 0;
							if (!string.IsNullOrEmpty(s))//存在格子
							{
								space = 1;
								string[] arrayGrid = s.Split('_', ';', '=');
								if (arrayGrid.Length == 2)//超级死地
								{
									if (int.TryParse(arrayGrid[1], out int resultSpace))
									{
										space = 3;
									}
									
									if (int.TryParse(arrayGrid[0], out int result))
									{
										port.integral = result;
									}
								}
								else if(arrayGrid.Length == 1)//死地
								{
									if (int.TryParse(s, out int result))
									{
										port.integral = result;
										if (result > 0)
										{
											space = 2;
										}
									}
								}
							}
							port.space = space;
							break;
                        case GridType.Item:
							if (int.TryParse(s, out int itemId))
							{
								port.itemId = itemId;
							}
							break;
                        case GridType.Cloud:
							if (int.TryParse(s, out int cloudId))
							{
								port.cloudId = cloudId;
							}
							break;
						case GridType.Skin:
							if (int.TryParse(s, out int skinIndex))
							{
								port.skin = skinIndex;
							}
							break;
					}

					if (port.space > 0)
					{
						mapData.Add(x, y, port);
					}
                }
			}

			File.WriteAllText(GetFilePath(gridType), JsonConvert.SerializeObject(mapData));
		}

		private string GetFilePath(GridType gridType)
		{
			if (!Directory.Exists(ConstConfig.GetMapDirPath))
			{
				Directory.CreateDirectory(ConstConfig.GetMapDirPath);
			}
			return Path.Combine(ConstConfig.GetMapDirPath, this.clientTableName+ ".txt");
		}

		private string GetFileEditorPath(GridType gridType)
		{
			if (!Directory.Exists(ConstConfig.GetMapDirPath))
			{
				Directory.CreateDirectory(ConstConfig.GetMapDirPath);
			}
			return Path.Combine(ConstConfig.GetMapEditorDirPath, this.GetLuaName(gridType) + ".txt");
		}


		private string GetLuaName(GridType gridType)
		{
			return this.clientTableName + gridType.ToString();
		}

		public enum GridType
		{
			Grid =1,
			Item,
			Cloud,
			Skin
		}
	}

	[Serializable]//临时数据导出通道
	public class temp_exportData
	{
		// 格子宽
		public float grid_w = 2.56F;
		//格子高
		public float grid_H = 1.56F;

		//格子边长的一半。
		public int sizeWidth;
		public int sizeHigth;
		/// <summary>
		/// 边角和海索引皮肤： 0 表示无皮肤
		/// </summary>
		public int skinIndex;

		public string spriteAtlasName = string.Empty;

		public List<temp_data_port> list = new List<temp_data_port>();
		public temp_data_port GetPort(int x, int y)
		{
			return list.Find(p => p.x == x && p.y == y);
		}

		public void Add(int x, int y, temp_data_port port)
		{
			int index = list.FindIndex(p => p.x == x && p.y == y);
			if (index >= 0)
			{
				list[index] = port;
			}
			else
			{
				list.Add(port);
			}
		}
	}


	[Serializable]
	public class temp_data_port
	{
		public int x;//x坐标
		public int y;//y坐标
		public int itemId;//物品id
		public int cloudId;//云id
		public int integral;//治愈积分
		public int space;//1=有格子 0=无格子 2=死地 3=超级死地, 4,放物品换钥匙,5,放物品开云块
		public int skin;//皮肤索引  

		public string spriteName = string.Empty;//格子精灵名
		public string spriteName_cloud = string.Empty;//云精灵名
		public int index;
	}
}
