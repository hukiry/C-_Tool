using System;
using System.Collections.Generic;
namespace Hukiry.Table
{
	public class TableTask:ITable
	{
		public int Id=>this.taskId;
		///<summary>
		///任务ID
		///<summary>
		public int taskId;
		///<summary>
		///完成目标数量
		///<summary>
		public int completeNum;
		public void ParseData(string line){
			string[] array = line.Split(',');
			this.taskId = int.Parse(array[0]);

			this.completeNum = int.Parse(array[1]);

		}
		///Custom_Code_Begin
		///Custom_Code_End
	}
}
