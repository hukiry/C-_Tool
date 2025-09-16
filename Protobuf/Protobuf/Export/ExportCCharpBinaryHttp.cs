using System;
using System.Collections.Generic;
using System.IO;

namespace Protobuf
{
    class ExportCCharpBinaryHttp: ExportCCharpBinary
	{
		
		protected const string method_TemplateHttp = @"
		/// <summary>
		/// {PROTO_DESC}
		/// </summary>
		bool receive_{PROTO_CMD}(IProto proto, string ipAddress)//接收客户端
		{
			msg_{PROTO_CMD} msg = proto as msg_{PROTO_CMD};
			return true;
		}
	
		/// <summary>
		/// {PROTO_DESC}
		/// </summary>
		IProto send_{PROTO_CMD}()//发给客户端
		{
			IProto msg = new msg_{PROTO_CMD}()
			{
			};
			return msg;
		}";
		public ExportCCharpBinaryHttp():base(true) {}

        public override void CreatePBMessage(List<cmd_info> cmdList)
        {
			const string PROTO_CLASS = "{PROTO_CLASS}";
			const string PROTO_Method = "{PROTO_Method}";
			const string PROTO_CMD = "{PROTO_CMD}";
			const string PROTO_DESC = "{PROTO_DESC}";
			string className = this.fileName.Split('_')[0];
			string saveClassName = $"{className}_message_Pb.cs";
			string fileTemplate = "Template/ServerHttpTemplate.cs";
			if (File.Exists(fileTemplate))
			{
				string methodText = string.Empty;
				string text = File.ReadAllText(fileTemplate);
				for (int i = 0; i < cmdList.Count; i++)
				{
					methodText += method_TemplateHttp
						.Replace(PROTO_CLASS, className)
						.Replace(PROTO_CMD, cmdList[i].cmd)
						.Replace(PROTO_DESC, cmdList[i].desc);
				}
				text = text.Replace(PROTO_CLASS, className).Replace(PROTO_Method, methodText);
				//创建目录
				string writeDir = WriteSubDirPath.Replace('\\', '/');
				string filePath = Path.Combine(writeDir, saveClassName);
				if (!File.Exists(filePath))
				{
					File.WriteAllText(filePath, text);
				}
			}
			else
			{
				Console.WriteLine("文件模板不存在：" + fileTemplate);
			}
		}
	}
}
