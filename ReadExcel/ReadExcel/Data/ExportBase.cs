using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
//https://github.com/ExcelDataReader/ExcelDataReader
namespace ExcelExportChartTool.Data
{
	internal class ExportBase
	{
		public string worksheet;
		public string clientTableName;
		public string fileName;
		public int columns;
		public int rows;
		public ExportBase(string worksheet, string clientTableName, string fileName)
		{
			this.worksheet = worksheet;
			this.clientTableName = clientTableName;
			this.fileName = fileName;
		}
		public virtual void Export(DataTable table, bool isSameTable)
		{
			 columns = table.Columns.Count;
			 rows = table.Rows.Count;
		}

		public string GetFilePath(string dir, string fileName, bool isChildDirectory = true)
		{
			string dirPath = this.IfDirExit(dir, isChildDirectory);
			string filePathData = Path.Combine(dirPath, fileName);
			return filePathData;
		}

		private string IfDirExit(string dirPath, bool isChildDirectory)
		{
			if (isChildDirectory)
			{
				dirPath = Path.Combine(Path.GetFullPath(dirPath), this.clientTableName);
			}

			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
			return dirPath;
		}

		protected int CellToInt(DataTable table, int rowIndex, int cellIndex)
		{
			string s = table.Rows[rowIndex][cellIndex].ToString();
			int.TryParse(s, out int result);
			return result;
		}

		protected string CellToString(DataTable table, int rowIndex, int cellIndex)
		{
			string result = table.Rows[rowIndex][cellIndex].ToString();
			return result;
		}

	}
}
