using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExcelExportChartTool
{
	public class FileTool
	{
		public static void CollectFilePathsByDir(string dirPath, List<string> outFiles)
		{
			string[] files = Directory.GetFiles(dirPath);
			string[] dir = Directory.GetDirectories(dirPath);
			foreach (var item in files)
			{
				outFiles.Add(item);
			}

			foreach (var item in dir)
			{
				CollectFilePathsByDir(item, outFiles);
			}
		}
	}
}
