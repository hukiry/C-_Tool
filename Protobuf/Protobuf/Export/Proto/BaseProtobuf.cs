using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Protobuf
{
	public abstract class BaseProtobuf : IProtobuf
	{
		/// <summary>
		/// 文件中的所有消息类集合
		/// </summary>
		public List<LuaProtobuf_data> luaProtobufs = null;
		/// <summary>
		/// 文件名
		/// </summary>
		public string fileName;
		/// <summary>
		/// 包名
		/// </summary>
		public string packageName;
		/// <summary>
		/// 导入的模块
		/// </summary>
		public List<string> exportBuff = null;
		void IProtobuf.Run(string filePath)
		{
			this.fileName = Path.GetFileNameWithoutExtension(filePath);
			this.luaProtobufs = new List<LuaProtobuf_data>();
			this.exportBuff = new List<string>();

			LuaProtobuf_data luaProtobuf = null;
			string[] lines = File.ReadAllLines(filePath);
			int length = lines.Length;
			string classDesc = string.Empty;
			for (int i = 0; i < length; i++)
			{

				string line = lines[i].Trim();
				if (line.ToLower().StartsWith("package"))
				{
					this.packageName = line.Split(' ', ';').Where(p => !string.IsNullOrEmpty(p)).ToArray()[1];
				}

				if (line.ToLower().StartsWith("import"))
				{
					string[] import = line.Split(' ', '"', '.').Where(p => p != "import" && (!string.IsNullOrEmpty(p))).ToArray();
					this.exportBuff.Add(import[0]);
				}


				if (line.StartsWith("//"))
				{
					classDesc= line.Replace("//", "---");
					continue;
				}

				if (line.StartsWith("enum"))
				{
					luaProtobuf = new LuaProtobuf_data();
					luaProtobuf.isEnum = true;
					luaProtobuf.desc = classDesc;
					var message = line.Split(' ').Where(p => p != "enum" && (!string.IsNullOrEmpty(p))).ToArray();
					luaProtobuf.className = message[0].TrimEnd('{');
					classDesc = string.Empty;
				}

				if (line.StartsWith("message"))
				{
					luaProtobuf = new LuaProtobuf_data();
					luaProtobuf.desc = classDesc;
					var message = line.Split(' ').Where(p => p != "message" && (!string.IsNullOrEmpty(p))).ToArray();
					luaProtobuf.className = message[0].TrimEnd('{');
					classDesc = string.Empty;
				}

				if (luaProtobuf != null)
				{
					if (luaProtobuf.isEnum)
					{
						var members = line.Split(' ', '"', '.', '=',  ';', ']', '[', '{').Where(p => !string.IsNullOrEmpty(p)).ToArray();
						if (!members.Contains("enum") && members.Length > 1)
						{
							var data = new Member_data
							{
								name = members[0],
								number = int.Parse(members[1]),
								full_name = $"{this.packageName}.{luaProtobuf.className}.{members[0]}",
							};

							int k_len = 0;
                            for (int K = 0; K < members.Length; K++)
                            {
								if (members[K].StartsWith("//"))
								{
									k_len = K;
								}
								if (k_len > 0 && k_len >= K)
								{
									data.desc += members[K].TrimStart('/','\\');
								}
                            }
							luaProtobuf.members.Add(data);
						}

					}
					else
					{
						if (line.StartsWith("required") || line.StartsWith("optional") || line.StartsWith("repeated"))
						{

							var members = line.Split(' ', '"', '.', '=', ';', ']', '[').Where(p => !string.IsNullOrEmpty(p)).ToArray();
							try
							{
								var data = new Member_data
								{
									label = Descriptor.GetFieldLabel(members[0]),
									labelName = members[0],
									typeName = members[1],
									isMessage = this.luaProtobufs.Find(p => p.className == members[1]) != null,
									type = this.luaProtobufs.Find(p => p.className == members[1]) != null ? Descriptor.GetFieldType("message") : Descriptor.GetFieldType(members[1]),
									cpp_type = Descriptor.GetFieldCppType(members[1]),
									name = members[2],
									number = int.Parse(members[3]),
									full_name = $"{this.packageName}.{luaProtobuf.className}.{members[2]}",
									default_value = members.Length > 4 && members[4].Equals("default") ? members[5] : null
								};

								int k_len = 0;
								for (int K = 1; K < members.Length; K++)
								{
									if (k_len==0 && members[K].StartsWith("//"))
									{
										k_len = K;
									}

									if (k_len >= K)
									{
										data.desc += members[K].TrimStart('/', '\\');
									}
								}

								if (string.IsNullOrEmpty(data.desc))
								{
									data.desc = classDesc.Replace("---","");
									classDesc = string.Empty;
								}

								luaProtobuf.members.Add(data);
							}
							catch (Exception ex)
							{
								Console.ForegroundColor = ConsoleColor.Green;
								foreach (var item in members)
								{
									Console.WriteLine(item);
								}
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine(ex.StackTrace);
								Console.WriteLine(ex.ToString());
								Console.ReadKey();
							}

						}
					}
					if (line.EndsWith("}")) this.luaProtobufs.Add(luaProtobuf);
				}

			}

			this.CreateBufCode();
		}

		private LuaProtobuf_data SplitLine(string line, LuaProtobuf_data luaProtobuf)
		{
			List<char> flagList = new List<char>() {
				' ', ';','{','}','=','[',']'
			};
			//=为结束符号，注释为结束符号
			int len = line.Length;
			string str = string.Empty;
			bool isZhu = false;
			bool isMessage = false;
			Member_data data = null;
			for (int i = 0; i < len; i++)
			{
				if (flagList.Contains(line[i]) && isZhu == false)
				{
					if (isMessage)
					{
						//类名
						luaProtobuf.className = str;
					}

					if (data != null)//类型，成员,数字
					{
						if (luaProtobuf.isEnum)
						{
							if (string.IsNullOrEmpty(data.name))
							{
								data.name = str;
							}
							else
							{
								data.number = int.Parse(str);
								data.full_name = $"{this.packageName}.{luaProtobuf.className}.{str}";
							}
						}
						else
						{
							if (string.IsNullOrEmpty(data.typeName))
							{
								data.typeName = str;
								data.isMessage = this.luaProtobufs.Find(p => p.className == str) != null;
								data.type = this.luaProtobufs.Find(p => p.className == str) != null ? Descriptor.GetFieldType("message") : Descriptor.GetFieldType(str);
								data.cpp_type = Descriptor.GetFieldCppType(str);
							}
							else if (string.IsNullOrEmpty(data.name))
							{
								data.name = str;
								data.full_name = $"{this.packageName}.{luaProtobuf.className}.{str}";
							}
							else
							{
								if (str == "default")
								{
									data.isHasDefaultvalue = true;
								}

								if (data.isHasDefaultvalue)
								{
									data.default_value = str;
								}
								else
								{
									data.number = int.Parse(str);
								}
							}
						}
					}

					string word = str.ToLower();
					if (word == "message"||word== "enum")
					{
						luaProtobuf = new LuaProtobuf_data();
						luaProtobuf.isEnum = word == "enum";
						isMessage = true;
						if (luaProtobuf.isEnum)
						{
							data = new Member_data();
						}
					}
					else if (word == "optional" || word == "required" || word == "repeated")
					{
						data = new Member_data();
						data.label = Descriptor.GetFieldLabel(word);
						data.labelName = word;
					}
					str = string.Empty;
				}
				else
				{
					if (line[i] == '/')
					{
						isZhu = true;
					}
					str += line[i];
					str = str.Trim();
				}
			}

			str = str.TrimStart('/');//注释文本


			if (data != null)
			{
				data.desc = str;
			}
			else
			{
				if (string.IsNullOrEmpty(luaProtobuf.className))
				{
					luaProtobuf.className = str;
				}
				else
				{
					luaProtobuf.desc = str;
				}
			}

			if (luaProtobuf != null)
			{
				luaProtobuf.members.Add(data);
			}

			return luaProtobuf;
		}

		public virtual void CreateBufCode()
		{

		}

		public abstract void WriteFile();

	}

	/// <summary>
	/// 消息类
	/// </summary>
	public class LuaProtobuf_data
	{
		/// <summary>
		/// 类名
		/// </summary>
		public string className;
		/// <summary>
		/// 类注释
		/// </summary>
		public string desc;
		public bool isEnum;
		public List<Member_data> members = new List<Member_data>();
	}

	/// <summary>
	/// 消息类成员
	/// </summary>
	public class Member_data
	{
		/// <summary>
		/// 字段名称
		/// </summary>
		public string name;
		/// <summary>
		/// 包名，类名，字段名称
		/// </summary>
		public string full_name;
		/// <summary>
		///  lua 中1开始
		/// </summary>
		public int number;
		/// <summary>
		/// c++ 0开始
		/// </summary>
		public int label;
		/// <summary>
		/// 标签名repeated|optional
		/// </summary>
		public string labelName;
		/// <summary>
		/// lua 中的类型
		/// </summary>
		public int type;
		/// <summary>
		/// 类型名
		/// </summary>
		public string typeName;
		/// <summary>
		/// c++ 中的类型
		/// </summary>
		public int cpp_type;
		/// <summary>
		/// 默认值
		/// </summary>
		public string default_value;//根据字段类型判断， 集合，字符串，数字 bool
		/// <summary>
		/// 是否有默认值
		/// </summary>
		public bool isHasDefaultvalue;
		/// <summary>
		/// 是否是类型声明的字段
		/// </summary>
		public bool isMessage;
		/// <summary>
		/// 数据标签
		/// </summary>
		public string dataTag;

		/// <summary>
		/// 描述
		/// </summary>
		public string desc;
		public string Getdefault_value()
		{
			if(label==3)//repeated
			{
				return "{}";
			}

			if (type == 9)
			{
				return has_default_value ? default_value : "\"\"";
			}
			else if (type == 8)
			{
				return has_default_value ? default_value : "false";
			}
			else if (type == 14 || type == 10 || type == 11)
			{
				return has_default_value ? default_value : "nil";
			}
			else
			{
				return has_default_value ? default_value : "0";
			}
		}
		/// <summary>
		/// c++, C# 0开始索引
		/// </summary>
		public int index => number - 1;
		/// <summary>
		/// 是否有默认值
		/// </summary>
		public bool has_default_value => default_value != null;
	}
}
