using System;
using System.Collections.Generic;
namespace Hukiry.Table
{
	public class TablePass:ITable
	{
		public int Id=>this.id;
		///<summary>
		///通行证id
		///<summary>
		public int id;
		///<summary>
		///月天数类型
		///<summary>
		public int monthType;
		public void ParseData(string line){
			string[] array = line.Split(',');
			this.id = int.Parse(array[0]);

			this.monthType = int.Parse(array[1]);

		}
		///Custom_Code_Begin
		///Custom_Code_End
	}
}
