using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;

namespace ExcelExportChartTool.Data
{
    class ExportScriptableObject : ExportBase
	{
		public static Dictionary<string, List<string>> dictable = new Dictionary<string, List<string>>();

		public Dictionary<int, lua_data> csDic = new Dictionary<int, lua_data>();
		public ExportScriptableObject(string worksheet, string clientTableName, string fileName) : base(worksheet, clientTableName, fileName) { }
		private string keyType = string.Empty;
		public override void Export(DataTable table, bool isSameTable)
		{
			base.Export(table, isSameTable);
			if (string.IsNullOrEmpty(ConstConfig.GetCCharpDirPath) || string.IsNullOrEmpty(ConstConfig.GetCCharpTxtDirPath))
			{
				return;
			}

			List<string> lines = new List<string>();
			string classData = $"Table{this.clientTableName}Data";
			string classHelpName = $"Table{this.clientTableName}";
			this.CheckSameTable(classData);
			lines.Add($"#文件：{this.fileName}->工作表：{this.worksheet}->{classData}");
			for (int i = 1; i < rows; i++)//行
			{
				bool isExportRowData = i >= 5;
				bool isBreakExport = false;
				string line = string.Empty;
				for (int j = 0; j < columns; j++)//列
				{
					string strValue = table.Rows[i][j].ToString();

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

					if (!csDic.ContainsKey(j))
					{
						csDic[j] = new lua_data();
						csDic[j].n = j;
					}
					switch (i)
					{
						case 1://字段注释
							csDic[j].description = strValue;
							break;
						case 2://客户端数据类型
                            csDic[j].fieldType = strValue;
                            break;
						case 3://服务端数据类型
							//csDic[j].fieldType = strValue;
							break;
						case 4://字段名称
							csDic[j].fieldName = strValue;
							break;
						default:
							//lua表数据导出部分
							if (csDic[j].IsExport())
							{
								if (csDic[j].IsBool())
								{
									strValue = string.IsNullOrEmpty(strValue) ? "0" : strValue.ToLower();
									if (strValue == "true" || strValue == "false") strValue = strValue.Equals("true") ? "1" : "0";
									if (string.IsNullOrEmpty(csDic[j].boolValue)) csDic[j].boolValue = strValue;
								}
								else if (csDic[j].IsList()) strValue = strValue.Replace(',', '_');
								else if (csDic[j].IsInt()) strValue = string.IsNullOrEmpty(strValue) ? "0" : strValue;
								line += strValue + ",";
							}
							break;
					}
				}
				if (isExportRowData)
				{
					line = line.TrimEnd(',');
					lines.Add(line);
				}

				if (isBreakExport)
				{
					break;
				}

			}


			string[] temp = new string[lines.Count - 1];
			lines.CopyTo(1, temp, 0, temp.Length);

			//同一张表时
			if (isSameTable)
			{
				//插入数据
				if (dictable[classData].Count > 0)
					lines.InsertRange(1, dictable[classData]);
			}
			this.CollectSameTable(classData, temp);

			this.WritecsData(classHelpName, lines.ToArray());
			ConstConfig.AddCCharpData(new Conifg_data()
			{
				fileName = classHelpName,
				taleName = this.clientTableName,
				excelFileName = this.fileName
			});

			this.WriteCsData();

			this.WriteCSHelper();
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

		private void WritecsData(string fileName, string[] lines)
		{
			string filePathData = this.GetFilePath(ConstConfig.GetCCharpTxtDirPath, $"{fileName}.txt", false);
			File.WriteAllLines(filePathData, lines);
		}
		private void WriteCsData()
		{
			string classData = $"Table{this.clientTableName}Data";
			string classHelpName = $"Table{this.clientTableName}";
			List<string> lines = new List<string>();
			lines.Add("using System;"); 
			lines.Add("using System.Collections.Generic;");
			lines.Add("using System.Collections;");
			lines.Add($"[System.Serializable]");
			lines.Add($"public class {classData}");
			lines.Add("{");
			var luaLst = csDic.Values.Where(p => p.IsExport()).ToList();
			luaLst.Sort((n, m) => n.n - m.n);
			int length = luaLst.Count;
			string methodBody = string.Empty;
			methodBody += "		public void ParseData(string line){\n";
			methodBody += "			string[] array = line.Split(',');\n";

			for (int i = 0; i < length; i++)
			{
				var data = luaLst[i];
				if (i == 0)
				{
					keyType = data.fieldType;
					lines.Add($"		public {data.fieldType} Key=>{data.fieldName};");
				}
				lines.Add($"		///<summary>");
				lines.Add($"		///{data.description}");
				lines.Add($"		///<summary>");
				lines.Add($"		public {data.fieldType} {data.fieldName};");
				if (data.IsBool())
				{
					methodBody += $"			this.{data.fieldName} = int.Parse(array[{i}])==1;\n";
				}
				else if (data.IsInt())
				{
					methodBody += $"			this.{data.fieldName} = int.Parse(array[{i}]);\n";
				}
				else if (data.IsString())
				{
					methodBody += $"			this.{data.fieldName} = array[{i}];\n";
				}
				else
				{
					if (data.IsList())
					{
						methodBody += this.SplitCode(data.fieldName, i, 1, data.GetTypeMethodCS());
					}
				}
				methodBody += "\n";
			}
			methodBody += "		}";
			lines.Add(methodBody);
			string filePathData = this.GetFilePath(ConstConfig.GetCCharpDirPath, $"{classData}.cs", false);
			string customCode = this.GetCustomCode(filePathData);
			lines.Add(customCode);
			lines.Add("}");
			
			File.WriteAllLines(filePathData, lines.ToArray());
		}

		private string SplitCode(string fieldName, int i, int listState, Hukiry<string, string, string, string> hukiry)
		{
			string temp = string.Empty;
			if (listState == 1)
			{
				temp = $"			this.{fieldName} = new List<{hukiry.value1}>();\n";
				temp += $"\t\t\tif (!string.IsNullOrEmpty(array[{i}]))\n";
				temp += "\t\t\t{\n";
				temp += $"\t\t\t\tstring[] lines{fieldName} = array[{i}].Split('_');\n";
				temp += $"\t\t\t\tfor (int i = 0; i < lines{fieldName}.Length; i++)\n";
				temp += "\t\t\t\t{\n";
				if (hukiry.value1 == "bool")
					temp += $"\t\t\t\t\tthis.{fieldName}.Add({hukiry.value2}(lines{fieldName}[i]) == 1));\n";
				else
					temp += $"\t\t\t\t\tthis.{fieldName}.Add({hukiry.value2}(lines{fieldName}[i]));\n";
				temp += "\t\t\t\t}\n";
				temp += "\t\t\t}\n";
			}
			return temp;
		}


		private void WriteCSHelper()
		{
			string template = @"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class {#ClassName}: ScriptableObject
{
    public List<{#ClassData}> datas = new List<{#ClassData}>();

    private Dictionary<{#KeyType}, {#ClassData}> dic = new Dictionary<{#KeyType}, {#ClassData}>();

	public void InitText(string line)
    {
        var p= new {#ClassData}();
        p.ParseData(line);
        datas.Add(p);
    }

    public {#ClassData} GetData({#KeyType} key)
    {
        if (!dic.ContainsKey(key))
        {
            foreach (var item in datas)
            {
                dic[item.Key] = item;
            }
        }
        return dic[key];
    }
";
			List<string> lines = new List<string>();
			string classData = $"Table{this.clientTableName}Data";
			string classHelpName = $"Table{this.clientTableName}";
			string filePathHelp = this.GetFilePath(ConstConfig.GetCCharpDirPath, classHelpName+ ".cs", false);
			string customCode = this.GetCustomCode(filePathHelp);
			lines.Add(template.Replace("{#ClassName}", classHelpName)
				.Replace("{#ClassData}", classData)
				.Replace("{#KeyType}", keyType));
			lines.Add(customCode);
			lines.Add("}");
			File.WriteAllLines(filePathHelp, lines.ToArray());
		}
		

		private string GetCustomCode(string filePathHelp)
		{
			if (File.Exists(filePathHelp))
			{
				string text = File.ReadAllText(filePathHelp);
				string beginStr = "///Custom_Code_Begin";
				int startIndex = text.IndexOf(beginStr) + beginStr.Length;
				int endIndex = text.IndexOf("///Custom_Code_End");
				if (startIndex > 0 && endIndex > startIndex)
				{
					string subTxt = text.Substring(startIndex, endIndex - startIndex).TrimStart('\n', '\r').TrimEnd('\n', '\r');
					return $"	///Custom_Code_Begin\n{subTxt}///Custom_Code_End";
				}
				else
				{
					return @"
	///Custom_Code_Begin

	///Custom_Code_End";
				}
			}
			else
			{
				return @"
	///Custom_Code_Begin

	///Custom_Code_End";
			}
		}
	}
}
