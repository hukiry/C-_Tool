using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ExcelExportChartTool.Data
{
	//第二行，字段注释
	//第三行，客户端数据类型
	//第四行，服务端数据类型
	//第五行，字段类型
	//第六行以上，数据
	//第一列数据无用  其他列，配置类型的导出
	//https://github.com/ExcelDataReader/ExcelDataReader
	internal class ExportLua: ExportBase
	{
		public static Dictionary<string, List<string>> dictable = new Dictionary<string, List<string>>();

		public Dictionary<int, lua_data> luaDic = new Dictionary<int, lua_data>();
		public ExportLua(string worksheet, string clientTableName, string fileName) : base(worksheet, clientTableName, fileName){}

		public override void Export(DataTable table, bool isSameTable)
		{
			base.Export(table, isSameTable);
			if (string.IsNullOrEmpty(ConstConfig.GetLuaDirPath))
			{
				return;
			}

			List<string> lines = new List<string>();
			string tableFileName = $"Table{this.clientTableName}";
			this.CheckSameTable(tableFileName);
			lines.Add($"\n---文件：{this.fileName}->工作表：{this.worksheet}->{tableFileName}");
			lines.Add($"local {tableFileName}=" + "{");
			for (int i = 1; i < rows; i++)//行
			{
				bool isExportRowData = i >= 5;
				bool isBreakExport = false;
				string line = string.Empty;
				for (int j = 0; j < columns; j++)//列
				{
					string strValue = table.Rows[i][j].ToString().Trim().Replace("\n", "").Replace("\r", "").Replace("'", "\\'");

					if (j == 0)
					{
						string ext = strValue.ToLower();
						if (ext == "none" || ext == "0" || ext == "no")
						{
							isExportRowData = false;
							break;
						}

						if (ext == "end")
						{
							isBreakExport = true;
						}
						continue;
					}

					if (j == 1)
					{
						if (string.IsNullOrEmpty(strValue))
						{
							isExportRowData = false;
							break;
						}
						else
						{
							if (int.TryParse(strValue, out int resultKey))
								line = $"\t[{strValue}]=" + "{";
							else
								line = $"\t['{strValue}']=" + "{";
						}

					}


					if (!luaDic.ContainsKey(j))
					{
						luaDic[j] = new lua_data();
						luaDic[j].n = j;
					}
					switch (i)
					{
						case 1://字段注释
							luaDic[j].description = strValue;
							break;
						case 2://客户端数据类型
							luaDic[j].fieldType = strValue;
							break;
						case 3://服务端数据类型
							break;
						case 4://字段名称
							luaDic[j].fieldName = strValue;
							break;
						default:
							//lua表数据导出部分
							if (luaDic[j].IsExport())
							{
								if (luaDic[j].IsBool())
								{
									strValue = string.IsNullOrEmpty(strValue) ? "0" : strValue.ToLower();
									if (bool.TryParse(strValue, out bool result))
									{
										strValue = result ? "1" : "0";
									}
									if (string.IsNullOrEmpty(luaDic[j].boolValue)) luaDic[j].boolValue = strValue;
								}
								else if (luaDic[j].IsString()) strValue = "\'" + strValue + "\'";
								else if (luaDic[j].IsList()) strValue = "\'" + strValue + "\'";
								else if (luaDic[j].IsInt()) strValue = string.IsNullOrEmpty(strValue) ? "0" : strValue;
								else if (luaDic[j].IsDictionary()) strValue = "\'" + strValue + "\'";
								else if (string.IsNullOrEmpty(strValue)) strValue = "\'" + strValue + "\'";
								line += strValue + ",";
							}
							break;
					}
				}
				if (isExportRowData)
				{
					line = line.TrimEnd(',');
					line += "},";
					lines.Add(line);
				}

				if (isBreakExport)
				{
					break;
				}

			}


			string[] temp = new string[lines.Count - 2];
			lines.CopyTo(2, temp, 0, temp.Length);
			//同一张表时
			if (isSameTable)
			{
				//插入数据
				if (dictable[tableFileName].Count > 0)
				{
					lines.InsertRange(2, dictable[tableFileName]);
				}
			}
			this.CollectSameTable(tableFileName, temp);

			lines.Add("}");
			lines.Add($"return {tableFileName}");
			this.WriteLuaData(tableFileName, lines.ToArray());
			this.WriteLuaHelp();
			this.WriteHelpBase();
		}


		private void CheckSameTable(string tableFileName)
		{
			if (!dictable.ContainsKey(tableFileName))
			{
				dictable[tableFileName] = new List<string>();
			}
		}

		private void CollectSameTable(string tableFileName, string[] temp)
		{
			if (dictable.ContainsKey(tableFileName))
			{
				dictable[tableFileName].AddRange(temp);
			}
		}

		private void WriteLuaData(string fileName, string[] lines)
		{
			string filePathData = this.GetFilePath(ConstConfig.GetLuaDirPath, $"{fileName}.lua");
			File.WriteAllLines(filePathData, lines);
		}

		//lua帮助基类
		private void WriteHelpBase()
		{
			string filePathHelp = Path.Combine(ConstConfig.GetLuaDirPath, "ConfigurationBase.lua");
			string luaTemplate = this.GetLuaHelpBase();
			File.WriteAllText(filePathHelp, luaTemplate);
		}

		//lua帮助类
		private void WriteLuaHelp()
		{
			string topDirName = ConstConfig.GetLuaDirPath.Substring(ConstConfig.GetLuaDirPath.Replace('\\','/').LastIndexOf('/')+1);
			string luaTemplate = this.GetLuaTemplate();
			string classItem = $"Table{this.clientTableName}Item";
			string className = $"Table{this.clientTableName}Help";
			string descFileName = $"{this.worksheet}->{this.clientTableName}";
			string bodyContent = string.Empty;
			string memberName = string.Empty;
			string tableFileName = $"Table{this.clientTableName}";

			string filePathHelp = this.GetFilePath(ConstConfig.GetLuaDirPath, $"{className}.lua");
			string customCode = this.GetCustomCode(filePathHelp);

			var luaLst = luaDic.Values.Where(p => p.IsExport()).ToList();
			luaLst.Sort((n, m) => n.n - m.n);
			int length = luaLst.Count;
			//字段列表导出
			for (int i = 0; i < length; i++)
			{
				if (i == 0) memberName = luaLst[i].fieldName;
				bodyContent += ("\n");
				bodyContent += ($"\t---{luaLst[i].description}\n");
				bodyContent += ($"\t---{luaLst[i].GetTypeDesc()}\n");
				if (luaLst[i].IsListDictionary())
				{
					bodyContent += this.SplitCode(luaLst[i].fieldName, i + 1, 3, luaLst[i].GetTypeMethod());
				}
				else if(luaLst[i].IsList())
				{
					bodyContent += this.SplitCode(luaLst[i].fieldName, i + 1, 1, luaLst[i].GetTypeMethod());
				}
				else if (luaLst[i].IsDictionary())
				{
					bodyContent += this.SplitCode(luaLst[i].fieldName, i + 1, 2, luaLst[i].GetTypeMethod());
				}
				else if (luaLst[i].IsBool())
				{
					bodyContent += this.SplitCode(luaLst[i].fieldName, i + 1, 4, luaLst[i].GetTypeMethod());
				}
				else
				{
					bodyContent += ($"\tself.{luaLst[i].fieldName} = data[{i + 1}]\n");
				}
			}
			bodyContent += ($"\n\tdata = nil\n");
			luaTemplate = luaTemplate.Replace("{classitem}", classItem).
				Replace("{class}", className).
				Replace("{body}", bodyContent).
				Replace("{FileName}", descFileName).
				Replace("{member}", memberName).
				Replace("{RootDirectory}", topDirName).
				Replace("{DirectoryName}", this.clientTableName).
				Replace("{Code}", customCode).
			Replace("{classtable}", tableFileName);
			File.WriteAllText(filePathHelp, luaTemplate);
			ConstConfig.AddLuaData(new Conifg_data() {
				fileName = className,
				taleName = this.clientTableName,
				excelFileName = this.fileName
			});
			//表名，文件名，注释，路径
		
		}

	

		private string SplitCode(string fieldName, int index, int listState, Hukiry<string, string> hukiry)
		{
			string temp = "\tself." + fieldName + " = {}\n";
			if (listState == 1)
			{
				temp += $"\tlocal arr_list = string.Split(data[{index}], ',')\n";
				temp += $"\tfor i, v in ipairs(arr_list) do\n";
				temp += $"\t\tif v ~= '' then\n";
				temp += $"\t\t\ttable.insert(self.{fieldName}, {hukiry.value1}(v))\n";
				temp += $"\t\tend\n";
				temp += $"\tend\n";
			}
			else if (listState == 3)
			{
				temp += $"\tlocal arr_dic = string.Split(data[{index}], '|')\n";
				temp += $"\tfor i, v in ipairs(arr_dic) do\n";
				temp += $"\t\tlocal temp_dic = string.Split(v, ',')\n";
				temp += $"\t\tif #temp_dic == 2 then\n";
				temp += $"\t\t\ttable.insert(self.{fieldName}, " + "{" + $"{hukiry.value1}(temp_dic[1]), {hukiry.value2}(temp_dic[2])" + "})\n";
				temp += $"\t\tend\n";
				temp += $"\tend\n";
			}
			else if (listState == 4)
			{
				if (!string.IsNullOrEmpty(hukiry.value1))
				{
					if (int.TryParse(hukiry.value1, out int bool_Int))
					{
						return $"\tself.{fieldName} = data[{index}] == 1\n";
					}
				}
				return $"\tself.{fieldName} = data[{index}]\n";
			}
			else
			{
				temp += $"\tlocal arr_dic = string.Split(data[{index}], '|')\n";
				temp += $"\tfor i, v in ipairs(arr_dic) do\n";
				temp += $"\t\tlocal temp_dic = string.Split(v, ',')\n";
				temp += $"\t\tif #temp_dic == 2 then\n";
				temp += $"\t\t\tself.{fieldName}[{hukiry.value1}(temp_dic[1])] = {hukiry.value2}(temp_dic[2])\n";
				temp += $"\t\tend\n";
				temp += $"\tend\n";
			}
			return temp;
		}

		private string GetCustomCode(string filePathHelp)
		{
			if (File.Exists(filePathHelp))
			{
				string text = File.ReadAllText(filePathHelp);
				string beginStr = "--Custom_Code_Begin";
				int startIndex = text.IndexOf(beginStr) + beginStr.Length;
				int endIndex = text.IndexOf("--Custom_Code_End");
				if (startIndex > 0 && endIndex > startIndex)
				{
					string subTxt = text.Substring(startIndex, endIndex - startIndex).TrimStart('\n', '\r').TrimEnd('\n', '\r');
					return $"\n{subTxt}\n";
				}
			}
			return string.Empty;
		}
		private string GetLuaTemplate()
		{
			return @"
---
--- {FileName}
--- Created by Hukiry 工具自动生成.

---@class {classitem}
local {classitem} = Class()
function {classitem}:ctor(data)
{body}
end

---@class {class}:ConfigurationBase
local {class} = Class(ConfigurationBase)

function {class}:ctor()
	self.TableItem = {classitem}
	self.sourceTable = require('{RootDirectory}.{DirectoryName}.{classtable}')
end

---@param {member} number 主键
---@param faultTolerance boolean 默认为true:容错，如果未找到则返回第一条数据，false:不容错
---@return {classitem}
function {class}:GetKey({member}, faultTolerance)
	return self:_GetKey({member}, faultTolerance)
end

--Custom_Code_Begin
{Code}
--Custom_Code_End

return {class}
";
		}

		private string GetLuaHelpBase()
		{
			return @"
---
--- ConfigurationBase       
--- Created by Hukiry 工具自动生成. 
---
---@class ConfigurationBase
ConfigurationBase = Class()

function ConfigurationBase:ctor()
	---子类源数据
    ---@type table<number, {}>
    self.sourceTable={}	
	---解析后的数据
    ---@type table<number, {}>
    self.classTable = {}	
    ---子类
    self.TableItem = nil
end

---@private
---初始化所有配置数据
function ConfigurationBase:_InitAllConfig()
    if table.length(self.sourceTable) > 0 then
        for k, v in pairs(self.sourceTable) do
            self.classTable[k] = self.TableItem.New(v)
            self.sourceTable[k] = nil
        end
    end
end

---获取整张配置表数据
---@return table<number, {}>
function ConfigurationBase:GetTable()
    self:_InitAllConfig()
    return self.classTable;
end

---@private
---@param key number 主键
---@param faultTolerance boolean 默认为true:容错，如果未找到则返回第一条数据，false:不容错
---@return table
function ConfigurationBase:_GetKey(key, faultTolerance)
    if key == nil then
        log(string.format('从TableCurrency获取数据错误,key:%s', key),'pink')
    end

    if self.classTable[key] then
        return self.classTable[key]
    elseif self.sourceTable[key] then
        self.classTable[key] = self.TableItem.New(self.sourceTable[key])
        self.sourceTable[key] = nil
    else
        if faultTolerance == nil or faultTolerance == true then
            log(string.format('从TableCurrency配置表获取Key错误 Key:%s  允许容错处理返回第一条数据', key),'red')
            if table.length(self.classTable) > 0 then
                return table.first(self.classTable)
            elseif table.length(self.sourceTable) > 0 then
                local v, k = table.first(self.sourceTable)
                self.classTable[k] = self.TableItem.New(v)
                self.sourceTable[k] = nil
                return self.classTable[k]
            end
        end
    end
    return self.classTable[key]
end
";
		}
	}


	public class lua_data
	{
		public int n;
		public string description;
		public string fieldName;
		public string boolValue;
		/// <summary>
		/// 一维数组：1|1|2
		/// 二维数组：1_2|2_4|1_4
		/// </summary>
		/// </summary>
		public string fieldType;

		public bool IsExport()
		{
			if (string.IsNullOrEmpty(this.fieldType) || string.IsNullOrEmpty(this.fieldName))
			{
				return false;
			}

			return true;
		}

		public bool IsBool()
		{
			return fieldType.ToLower() == "bool";
		}

		public bool IsInt()
		{
			return fieldType.ToLower() == "int";
		}

		public bool IsString()
		{
			return fieldType.ToLower() == "string";
		}

		public bool IsList()
		{
			return fieldType.ToLower().Contains("list<") && fieldType.ToLower().EndsWith(">");
		}

		public bool IsDictionary()
		{
			return fieldType.ToLower().Contains("dictionary<") && fieldType.ToLower().EndsWith(">");
		}

		public bool IsListDictionary()
		{
			return (fieldType.ToLower().Contains("list<dictionary<")|| fieldType.ToLower().Contains("list<hukiry<")) && fieldType.ToLower().EndsWith(">>");
		}

		public Hukiry<string, string> GetTypeMethod()
		{
			Func<string, string> getDicMethod = typeStr =>
			{
				if (typeStr == "int")
				{
					return "tonumber";
				}
				else if (typeStr == "string")
				{
					return "tostring";
				}
				else
				{
					return "ToBoolean";
				}
			};


			if (IsListDictionary())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);

					index = lastS.IndexOf('<') + 1;
					lastS = lastS.Substring(index, lastS.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string method1 = getDicMethod(arr[0]);
						string method2 = getDicMethod(arr[1]);
						return new Hukiry<string, string>(method1, method2);
					}
				}
				catch { }
				return new Hukiry<string, string>("string", "string");
			}
			else if (IsBool())
			{
				return new Hukiry<string, string>(boolValue, null);
			}
			else if (IsList())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);
					if (lastS == "int")
					{
						return new Hukiry<string, string>("tonumber", "tonumber");
					}
					else if (lastS == "string")
					{
						return new Hukiry<string, string>("tostring", "tostring");
					}
					else if (lastS == "bool")
					{
						return new Hukiry<string, string>("ToBoolean", "ToBoolean");
					}
				}
				catch
				{
				}

				return new Hukiry<string, string>("string", "string");
			}
			else
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string method1 = getDicMethod(arr[0]);
						string method2 = getDicMethod(arr[1]);
						return new Hukiry<string, string>(method1, method2);
					}
				}
				catch { }
				return new Hukiry<string, string>("string", "string");
			}
		}

		public Hukiry<string, string, string, string> GetTypeMethodCS()
		{
			Func<string, string> getDicMethod = typeStr =>
			{
				if (typeStr == "int")
				{
					return "int.Parse";
				}
				else if (typeStr == "bool")
				{
					return "int.Parse";
				}
				else if (typeStr == "float")
				{
					return "float.Parse";
				}
				else
				{
					return "";
				}
			};


			if (IsListDictionary())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);

					index = lastS.IndexOf('<') + 1;
					lastS = lastS.Substring(index, lastS.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string method1 = getDicMethod(arr[0]);
						string method2 = getDicMethod(arr[1]);
						return new Hukiry<string, string, string, string>(arr[0], arr[1], method1, method2);
					}
				}
				catch { }
				return new Hukiry<string, string, string, string>("string", "string", string.Empty, string.Empty);
			}
			else if (IsList())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);
					if (lastS == "int")
					{
						return new Hukiry<string, string, string, string>("int", "int.Parse", string.Empty, string.Empty);
					}
					else if (lastS == "string")
					{
						return new Hukiry<string, string, string, string>("string", string.Empty, string.Empty, string.Empty);
					}
					else if (lastS == "bool")
					{
						return new Hukiry<string, string, string, string>("bool", "int.Parse", string.Empty, string.Empty);
					}
					else if (lastS == "float")
					{
						return new Hukiry<string, string, string, string>("float", "float.Parse", string.Empty, string.Empty);
					}
				}
				catch { }
				return new Hukiry<string, string, string, string>("string", "string", string.Empty, string.Empty);
			}
			else
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string method1 = getDicMethod(arr[0]);
						string method2 = getDicMethod(arr[1]);
						return new Hukiry<string, string, string, string>(arr[0], arr[1], method1, method2);
					}
				}
				catch { }
				return new Hukiry<string, string, string, string>("string", "string", string.Empty, string.Empty);
			}
		}

		public string GetTypeDesc()
		{
			if (IsBool())
			{
				return "@type boolean";
			}
			else if (IsString())
			{
				return "@type string";
			}
			else if (IsListDictionary())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);

					index = lastS.IndexOf('<') + 1;
					lastS = lastS.Substring(index, lastS.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string typeArr = string.Empty;
						for (int i = 0; i < 2; i++)
						{
							if (arr[i] == "int")
							{
								typeArr += "number, ";
							}
							else if (arr[i] == "string")
							{
								typeArr += "string, ";
							}
							else if (arr[i] == "bool")
							{
								typeArr += "boolean, ";
							}
						}
						typeArr = typeArr.Trim().TrimEnd(',');

						return "@type table<table<" + typeArr + ">>";
					}
				}
				catch { }
				return "@type table<table<number,number>>";
			}
			else if(IsList())
			{
                try
                {
                    string s = fieldType.ToLower();
                    int index = s.IndexOf('<') + 1;
                    string lastS = s.Substring(index, s.Length - index - 1);
					if (lastS == "int")
					{
						return "@type table<number>";
					}
					else if(lastS == "string")
					{
						return "@type table<string>";
					}
					else if(lastS == "bool")
					{
						return "@type table<boolean>";
					}
				}
                catch
                {
                }
				
				return "@type table<number>";
			}
			else if (IsInt())
			{
				return "@type number";
			}
			else if (IsDictionary())
			{
				try
				{
					string s = fieldType.ToLower();
					int index = s.IndexOf('<') + 1;
					string lastS = s.Substring(index, s.Length - index - 1);
					string[] arr = lastS.Split(',');

					if (arr.Length == 2)
					{
						string typeArr = string.Empty;
                        for (int i = 0; i < 2; i++)
                        {
							if (arr[i] == "int")
							{
								typeArr += "number, ";
							}
							else if (arr[i] == "string")
							{
								typeArr += "string, ";
							}
							else if (arr[i] == "bool")
							{
								typeArr += "boolean, ";
							}
						}
						typeArr = typeArr.Trim().TrimEnd(',');

						return "@type table<" + typeArr + ">";
					}
				}
				catch
				{
				}

				return "@type table<number,number>";
			}
			
			return "@type table";
		}
	}


	public class Hukiry<T1, T2>
	{
		public T1 value1;
		public T2 value2;
		public Hukiry(T1 t1, T2 t2)
		{
			this.value1 = t1;
			this.value2 = t2;
		}
	}

	public class Hukiry<T1, T2,T3,T4>
	{
		public T1 value1;
		public T2 value2;
		public T3 value3;
		public T4 value4;
		public Hukiry(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			this.value1 = t1;
			this.value2 = t2;
			this.value3 = t3;
			this.value4 = t4;
		}
	}
}
