using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuf
{
	public class Descriptor
	{
		private static Dictionary<string, int> FieldDescriptor = new Dictionary<string, int>  {
			/*type*/
			{"TYPE_DOUBLE",		1},
			{"TYPE_FLOAT",		2},
			{"TYPE_INT64",		3},//
			{"TYPE_UINT64",		4},
			{"TYPE_INT32",		5},//
			{"TYPE_FIXED64",	6},
			{"TYPE_FIXED32",	7},
			{"TYPE_BOOL",		8},//
			{"TYPE_STRING",		9 },//
			{"TYPE_GROUP",		10},
			{"TYPE_MESSAGE",	11},//字段为类 {}
			{"TYPE_BYTES",		12},
			{"TYPE_UINT32",		13},
			{"TYPE_ENUM",		14},//
			{"TYPE_SFIXED32",	15},
			{"TYPE_SFIXED64",	16},
			{"TYPE_SINT32",		17},
			{"TYPE_SINT64",		18},
			{"MAX_TYPE",		18 },
			/*cpp_type*/
			{"CPPTYPE_INT32",	1},//
			{"CPPTYPE_INT64",	2},//
			{"CPPTYPE_UINT32",	3},
			{"CPPTYPE_UINT64",	4},
			{"CPPTYPE_DOUBLE",	5},
			{"CPPTYPE_FLOAT",	6},
			{"CPPTYPE_BOOL",	7},//
			{"CPPTYPE_ENUM",	8},//
			{"CPPTYPE_STRING",	9},//
			{"CPPTYPE_MESSAGE", 10},
			{"MAX_CPPTYPE",		10},
			/*label*/
			{"LABEL_OPTIONAL",	1},//可选
			{"LABEL_REQUIRED",	2},//要求
			{"LABEL_REPEATED",	3},//重复 repeated
			{"MAX_LABEL",		3}
		};

		private static Dictionary<string, List<string>> Make_descriptor = new Dictionary<string, List<string>>
		{
			{"Descriptor",new List<string> {
					"name",
					"full_name",
					//"filename",
					//"containing_type",
					"fields",
					"nested_types",
					"enum_types",
					"extensions",
					//"options",
					"is_extendable",
					//"extension_ranges"
				}
			},

			{"FieldDescriptor",new List<string> {
					"name",
					"full_name",
					"index",
					"number",
					"type",
					"cpp_type",
					"label",
					"has_default_value",
					"default_value",
					//"containing_type",
					"message_type",
					//"enum_type",
					//"is_extendable",
					//"extension_ranges"
				}
			},
			{"EnumDescriptor",new List<string> {
					"name",
					"full_name",
					"values",
					"containing_type",
					"options"}
			},
			{"EnumValueDescriptor",new List<string> {
					"name",
					"index",
					"number",
					"type",
					"options"}
			}
		};

		/// <summary>
		/// 类名描述输出
		/// </summary>
		public static List<string> GetClassDescriptor() => Make_descriptor["Descriptor"];
		/// <summary>
		/// 类成员描述输出
		/// </summary>
		public static List<string> GetClassFieldDescriptor() => Make_descriptor["FieldDescriptor"];
		/// <summary>
		/// 枚举描述输出
		/// </summary>
		public static List<string> GetEnumDescriptor() => Make_descriptor["EnumDescriptor"];
		/// <summary>
		/// 枚举成员描述输出
		/// </summary>
		public static List<string> GetEnumValueDescriptor() => Make_descriptor["EnumValueDescriptor"];

		/// <summary>
		/// label
		/// </summary>
		public static int GetFieldLabel(string label) => FieldDescriptor.ContainsKey("LABEL_" + label.ToUpper()) ?FieldDescriptor["LABEL_" + label.ToUpper()]: FieldDescriptor["MAX_LABEL"];
		/// <summary>
		/// type
		/// </summary>
		public static int GetFieldType(string type) => FieldDescriptor.ContainsKey("TYPE_" + type.ToUpper()) ? FieldDescriptor["TYPE_" + type.ToUpper()] : FieldDescriptor["MAX_TYPE"];
		/// <summary>
		/// cpp_type
		/// </summary>
		public static int GetFieldCppType(string cpp_type) => FieldDescriptor.ContainsKey("CPPTYPE_" + cpp_type.ToUpper()) ? FieldDescriptor["CPPTYPE_" + cpp_type.ToUpper()] : FieldDescriptor["MAX_CPPTYPE"];
	}

}
