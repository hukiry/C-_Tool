using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Protobuf
{
	public enum PathType
	{
		GetProtobufDir,
		GetExportBinaryDir,
		GetExportBinarySubDir,
		GetExportProtobufDir,
		GetExportType
	}


	class ConstConfig
	{
		private const string CONFIG_PATH = "ConfigPath.ini";
		[ConfigEnum((int)PathType.GetProtobufDir)] private static string ProtobufPath = string.Empty;
		[ConfigEnum((int)PathType.GetExportBinaryDir)] private static string ExportBinary = string.Empty;
		[ConfigEnum((int)PathType.GetExportBinarySubDir)] private static string ExportBinarySubDir = string.Empty;
		[ConfigEnum((int)PathType.GetExportProtobufDir)] private static string ExportProtobuf = string.Empty;
		[ConfigEnum((int)PathType.GetExportType)] private static string ExportType = string.Empty;

		private static Dictionary<int, FieldInfo> dicInfo = new Dictionary<int, FieldInfo>();

		public static void InitConfig()
		{
			
			InitField();
			string filePath = Path.Combine(Application.StartupPath, CONFIG_PATH);

			if (File.Exists(filePath))
			{
				string[] lines = File.ReadAllLines(filePath);
				foreach (var line in lines)
				{
					if (line.Trim().StartsWith("#"))
					{
						continue;
					}
					string[] array = line.Split('=');
					if (array != null && array.Length >= 2)
					{
						dicInfo.Values.First(p => p.Name == array[0].Trim())?.SetValue(typeof(ConstConfig), array[1].Trim());
					}
				}
			}
			else
			{
				WriteConifg();
			}
		}
		public static string GetValue(PathType KEY)
		{
			int key = (int)KEY;
			return GetValue(key);
		}

		private static string GetValue(int KEY)
		{
			return (string)dicInfo[KEY].GetValue(typeof(ConstConfig));
		}

		private static void InitField()
		{
			FieldInfo[] fieldInfos = typeof(ConstConfig).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (var item in fieldInfos)
			{
				ConfigEnumAttribute configEnum = item.GetCustomAttribute<ConfigEnumAttribute>();
				if (configEnum)
				{
					dicInfo[configEnum.exportDocumentType] = item;
				}
			}
		}

		private static void WriteConifg()
		{

			List<string> configs = new List<string>();
			configs.Add("#协议配置目录路径");
			configs.Add("ProtobufPath =");
			configs.Add("#生成的目录路径 自定义Lua二进制");
			configs.Add("ExportBinary =");
			configs.Add("#生成的目录路径 Lua Protobuf");
			configs.Add("ExportProtobuf =");
			configs.Add("#生成协议类型 1 = luaProtobuf		2 = luaBinary");
			configs.Add("ExportType =");
			File.WriteAllLines(CONFIG_PATH, configs.ToArray());
		}
	}

	[AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class ConfigEnumAttribute : Attribute
	{
		public int exportDocumentType;
		public ConfigEnumAttribute(int Name)
		{
			this.exportDocumentType = Name;
		}

		public static implicit operator bool(ConfigEnumAttribute configEnum)
		{
			return configEnum != null;
		}
	}
}
