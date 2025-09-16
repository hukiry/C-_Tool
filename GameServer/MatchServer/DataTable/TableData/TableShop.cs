using System;
using System.Collections.Generic;
namespace Hukiry.Table
{
	public class TableShop:ITable
	{
		public int Id=>this.productId;
		///<summary>
		///货品ID
		///<summary>
		public int productId;
		///<summary>
		///购买获取物品ID
		///<summary>
		public int getId;
		///<summary>
		///购买的货币类型
		///<summary>
		public int currency;
		///<summary>
		///购买的价格
		///<summary>
		public int price;
		///<summary>
		///购买的价格递增
		///<summary>
		public int addVal;
		///<summary>
		///充值获得砖石
		///<summary>
		public int diamondNum;
		public void ParseData(string line){
			string[] array = line.Split(',');
			this.productId = int.Parse(array[0]);

			this.getId = int.Parse(array[1]);

			this.currency = int.Parse(array[2]);

			this.price = int.Parse(array[3]);

			this.addVal = int.Parse(array[4]);

			this.diamondNum = int.Parse(array[5]);

		}
		///Custom_Code_Begin
		///Custom_Code_End
	}
}
