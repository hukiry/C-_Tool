using System;
using System.Collections.Generic;
using System.IO;

namespace Protobuf
{
	class ExportFactory
	{
		static string helpInfo = @"
	 ________________________________帮助说明_________________________________
	|	
	|	[协议模板定义]
	|	//注释
	|	message 消息名
	|	{
 	|	   required|repeated 字段类型 字段名称 = 序号;//注释
	|	   ...
	|	}
	|	生成协议类型 1 = lua pb导出, 2 = lua二进制导出, 3 = C#二进制导出, 4=C#Http二进制导出
	|_________________________________________________________________________
";
		public ExportFactory()
		{
			ConstConfig.InitConfig();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(helpInfo);
		}

		public void Run()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			IProtobuf protobuf;


			if (ConstConfig.GetValue(PathType.GetExportType).Equals("1"))
			{
				Console.WriteLine("开始 Lua buf 导出...");
				protobuf = new ExportLuaProtobuf();
			}
			else if (ConstConfig.GetValue(PathType.GetExportType).Equals("2"))
			{
				Console.WriteLine("开始 Lua 二进制导出...");
				protobuf = new ExportLuaBinary();
			}
			else if (ConstConfig.GetValue(PathType.GetExportType).Equals("3"))
			{
				Console.WriteLine("开始C# 导出...");
				protobuf = new ExportCCharpBinary();
            }
            else {
				protobuf = new ExportCCharpBinaryHttp();
			}

			Console.Title = "协议导出";
			var list = GetFiles();
			int index = 0;
			foreach (var filePath in list)
			{
				index++;
				protobuf.Run(filePath);
				protobuf.WriteFile();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"{index}，{filePath}");
				Console.ForegroundColor = ConsoleColor.Yellow;
			}
			Console.WriteLine("...Finished！");
			Console.ReadKey();
		}

		private List<string> GetFiles()
		{
			string dirPath = ConstConfig.GetValue(PathType.GetProtobufDir);
			if (Directory.Exists(dirPath))
			{
				return new List<string>(Directory.GetFiles(dirPath, "*.proto", SearchOption.AllDirectories));
			}
			return new List<string>();
		}
	}
}
