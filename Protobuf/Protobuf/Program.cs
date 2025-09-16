namespace Protobuf
{
	class Program
	{
		static void Main(string[] args)
		{
			/* 配置文件：
			 * 协议配置目录路径：
			 * 生成的目录路径：
			 * 生成协议类型：1 = luaProtobuf		2 = luaBinary(自定义解析)
			 */
			ExportFactory factory = new ExportFactory();
			factory.Run();
		}
	}
}
