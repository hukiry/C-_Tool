
using ExcelExportChartTool;
using ExcelExportChartTool.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Runtime.DesignerServices;
using System.Runtime.Hosting;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace ReadExcel
{
   
    static class Program
    {
        const string CONFIG_PATH = "ConfigPath.ini";
       
        static void Main(string[] args)
        {
            Console.Title = "导表工具";
            if (GetDotNetVersion("4.0"))
            {

                try
                {
                    ExportManger.instance.Run();
                }
                catch {}

				Console.ReadKey();
            } 
        }

        /// <summary>
        /// 是否是4.0以上版本
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static bool GetDotNetVersion(string version)
        {
            CheckVersion();

            string oldname = "0";
            using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string newname = (string)versionKey.GetValue("Version", "");
                        if (string.Compare(newname, oldname) > 0)
                        {
                            oldname = newname;
                        }
                        if (newname != "")
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            newname = (string)subKey.GetValue("Version", "");
                            if (string.Compare(newname, oldname) > 0)
                            {
                                oldname = newname;
                            }
                        }
                    }
                }
            }
            bool isDoVersion = string.Compare(oldname, version) > 0 ? true : false;

            if (!isDoVersion)
            {
                MessageBox.Show("需安装 .NET Framework 4.0 或以上版本。");
            }
            return isDoVersion;
        }

        private static void CheckVersion()
        {
            try
            {
                string text = RuntimeEnvironment.GetSystemVersion().Trim(new char[]
                {
                    'v'
                });
                string[] array = text.Split(new char[]
                {
                    '.'
                });

                if (array.Length > 0)
                {
                    int num;
                    int.TryParse(array[0], out num);
                    if (num < 4)
                    {
                        MessageBox.Show("需安装 .NET Framework 4.0 或以上版本。");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private static void WriteConifg()
        {
            
            List<string> configs = new List<string>();
            configs.Add("TableDirPath=");
            configs.Add("ExportLuaDirPath=");
            configs.Add("ExportCCharpDirPath=");
            configs.Add("ExportTxtDirPath=");
            configs.Add("ExportXmlDirPath=");
            File.WriteAllLines(CONFIG_PATH, configs.ToArray());
        }
    }
}
