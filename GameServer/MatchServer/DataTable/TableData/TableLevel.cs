using System;
using System.Collections.Generic;
namespace Hukiry.Table
{
	public class TableLevel:ITable
	{
		public int Id=>this.lvId;
		///<summary>
		///关卡id
		///<summary>
		public int lvId;
		///<summary>
		///关卡地图名
		///<summary>
		public int lv;
		///<summary>
		///消耗体力
		///<summary>
		public int energy;
		///<summary>
		///付费奖励价格
		///<summary>
		public List<int> price;
		///<summary>
		///付费奖励内容
		///<summary>
		public List<Hukiry<int,int>> payContent;
		public void ParseData(string line){
			string[] array = line.Split(',');
			this.lvId = int.Parse(array[0]);

			this.lv = int.Parse(array[1]);

			this.energy = int.Parse(array[2]);

			this.price = new List<int>();
			if (!string.IsNullOrEmpty(array[3]))
			{
				string[] linesprice = array[3].Split('_');
				for (int i = 0; i < linesprice.Length; i++)
				{
					this.price.Add(int.Parse(linesprice[i]));
				}
			}

			this.payContent = new List<Hukiry<int,int>>();
			if (!string.IsNullOrEmpty(array[4]))
			{
				string[] linespayContent = array[4].Split('|');
				for (int i = 0; i < linespayContent.Length; i++)
				{
					string[] dicArray = linespayContent[i].Split('_');
					this.payContent.Add(new Hukiry<int,int>(int.Parse(dicArray[0]),int.Parse(dicArray[1])));
				}
			}

		}
		///Custom_Code_Begin
		///Custom_Code_End
	}
}
