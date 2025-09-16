using System;
using System.Collections.Generic;
namespace Hukiry.Table
{
	public class TableGlobal:ITable
	{
		public int Id=>this.id;
		///<summary>
		///
		///<summary>
		public int id;
		///<summary>
		///配置内容
		///<summary>
		public string itemValue;
		public void ParseData(string line){
			string[] array = line.Split(',');
			this.id = int.Parse(array[0]);

			this.itemValue = array[1];

		}
		///Custom_Code_Begin
		///Custom_Code_End
	}
}
