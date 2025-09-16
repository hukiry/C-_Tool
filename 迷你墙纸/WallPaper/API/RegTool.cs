using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;
using 迷你墙纸.Properties;

namespace 迷你墙纸
{
    class RegTool
    {


        //创建快捷键
        #region 创建快捷键
        /// <summary>
        /// 创建桌面图标
        /// </summary>
        public static void CreateShortcutDesktop(string StartupPath, string ExecutablePath)
        {
            string appName = Path.GetFileNameWithoutExtension(ExecutablePath);
            string desk = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string pathLink = Path.Combine(desk, $"{appName}.lnk");
            if (!File.Exists(pathLink))
            {
                if (Directory.Exists(desk))
                {
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(pathLink);
                    sc.TargetPath = ExecutablePath;
                    sc.WorkingDirectory = StartupPath;
                    sc.Description = "位置：" + StartupPath;
                    sc.IconLocation = ExecutablePath;
                    sc.Save();
                }
            }
        }

        /// <summary>
        /// 附加到开始菜单
        /// </summary>
        public static void CreateShortcut_StartMenu(string StartupPath, string ExecutablePath)
        {
            string dirPath =StartupPath;
            string nameApp = Path.GetFileNameWithoutExtension(ExecutablePath);

            string linkDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Hukiry");
            string pathLink = Path.Combine(linkDirPath, $"{nameApp}.lnk");
            if (!File.Exists(pathLink))
            {
                if (!Directory.Exists(linkDirPath))
                {
                    Directory.CreateDirectory(linkDirPath);
                }

                IWshRuntimeLibrary.IWshShell_Class shell = new IWshRuntimeLibrary.IWshShell_Class();
                IWshRuntimeLibrary.IWshShortcut wshShortcut = shell.CreateShortcut(pathLink);
                wshShortcut.TargetPath = dirPath + $"\\{nameApp}.exe";
                wshShortcut.WorkingDirectory = dirPath;
                wshShortcut.Description = nameApp;
                wshShortcut.IconLocation = dirPath + $"\\{nameApp}.exe";
                wshShortcut.Save();
            }
        }
        #endregion
        #region 注册表

        private void StartRegedit(string dirPath, string ExecutablePath, bool isAND = false)
        {
            try
            {
               AddSystemRegedit(dirPath, ExecutablePath, isAND);
                //this.RegistryExe(ExecutablePath, isAND);
                //this.CreateSubKeyToFile(ExecutablePath, "sip", isAND);
                //this.CreateSubKeyToFile(ExecutablePath, "hxp", isAND);
                //this.CreateSubKeyToDirectory(ExecutablePath, isAND);
            }
            catch (Exception ex)
            {
                MessageBox.Show("没有写入权限：" + ex.ToString());
            }
        }

        /// <summary>
        /// 关联文件-鼠标右键
        /// </summary>
        /// <param name="ExecutablePath"></param>
        private void RegistryExe(string ExecutablePath, bool isAnd = false)
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("*\\shell\\Hukiry", true);
            if (registryKey == null || isAnd)
            {
                registryKey = Registry.ClassesRoot.CreateSubKey("*\\shell\\Hukiry", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registryKey?.SetValue("", "用轻压打开");
                registryKey?.SetValue("Icon", ExecutablePath);
                RegistryKey registrySub = registryKey?.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
                registrySub?.Close();
            }
            registryKey?.Close();
        }

        /// <summary>
        /// 关联目录-鼠标右键
        /// </summary>
        /// <param name="ExecutablePath"></param>
        /// <param name="isAnd"></param>
        private void CreateSubKeyToDirectory(string ExecutablePath, bool isAnd = false)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\shell\\hukiry", true);
            if (registryKey == null || isAnd)
            {
                //图标显示
                registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Classes\\Directory\\shell\\hukiry", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registryKey?.SetValue("", "用轻压压缩");
                registryKey?.SetValue("Icon", ExecutablePath);
                //应用程序命令
                RegistryKey registrySub = registryKey?.CreateSubKey("Command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
                registrySub?.Close();
            }
            registryKey?.Close();
        }

        ///// <summary>
        ///// 文件图标+新建+打开
        ///// </summary>
        ///// <param name="ExecutablePath"></param>
        ///// <param name="ext"></param>
        ///// <param name="isAnd"></param>
        ///// <param name="type"></param>
        //private void CreateSubKeyToFile(string ExecutablePath, string ext, bool isAnd = false, string type = "File")
        //{
        //    RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey($".{ext}", true);
        //    if (registryKey == null || isAnd)
        //    {
        //        registryKey = Registry.ClassesRoot.CreateSubKey($".{ext}", true);
        //        registryKey?.SetValue("", $"{type}.{ext}");
        //        registryKey?.SetValue("Content Type", $"{type}/{ext}");
        //        //添加邮件新建文件
        //        RegistryKey registrySub = registryKey?.CreateSubKey("ShellNew", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        registrySub?.SetValue("NullFile", "");
        //        //添加文件图标
        //        registryKey = Registry.ClassesRoot.CreateSubKey($"{type}.{ext}", true);
        //        if (ext.ToLower().Equals("hxp"))
        //        {
        //            registryKey?.SetValue("", "轻压文件");//添加新建文件名称
        //        }
        //        else
        //        {
        //            registryKey?.SetValue("", "");
        //        }
        //        registrySub = registryKey?.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        registrySub?.SetValue("", ExecutablePath);

        //        //添加双击打开命令
        //        registrySub = registryKey?.CreateSubKey("shell\\open\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
        //        registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
        //        registrySub?.Close();

        //    }
        //    registryKey?.Close();
        //}

        /// <summary>
        /// 注册到系统
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="ExecutablePath"></param>
        /// <param name="isAnd"></param>
        public static void AddSystemRegedit(string dirPath, string ExecutablePath, bool isAnd = false)
        {

            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                if (registryKey == null || isAnd)
                {
                    RegistryKey registrySub = registryKey?.CreateSubKey("hukiry", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    string fileName = Path.GetFileNameWithoutExtension(ExecutablePath);
                    registrySub?.SetValue("InstallLocation", dirPath);//安装目录
                    registrySub?.SetValue("HelpLink", "www.hukiry.com");//帮助链接
                    registrySub?.SetValue("URLInfoAbout", "www.hukiry.com");//支持链接
                    registrySub?.SetValue("DisplayName", fileName);//陈列名称
                    registrySub?.SetValue("DisplayVersion", Application.ProductVersion);//陈列版本号
                    registrySub?.SetValue("DisplayIcon", ExecutablePath);//陈列图标
                    registrySub?.SetValue("Publisher", "胡雄坤独立工作室");//开发者
                    registrySub?.SetValue("InstallDate", DateTime.Now.ToString("yyyyddMM"));//安装日期
                                                                                            //registrySub?.SetValue("UninstallString", dirPath + $"{nameof(Resources.HukiryThumbnailExtendion)}.bat");//卸载执行路径
                    FileInfo fi = new FileInfo(ExecutablePath);

                    registrySub?.SetValue("EstimatedSize", fi.Length / 1024F, RegistryValueKind.DWord);//安装尺寸
                    registrySub?.SetValue("NoModify", 1, RegistryValueKind.DWord);//无更新
                    registrySub?.SetValue("NoRepair", 1, RegistryValueKind.DWord);//无修复

                    registrySub?.Close();
                }
                registryKey?.Close();
            }
            catch{}

        }

        /// <summary>
        /// 开机启动
        /// </summary>
        /// <param name="isStart">默认开机启动是、</param>
        /// <param name="exeName">执行文件名</param>
        /// <param name="path">执行文件路径</param>
        /// <returns></returns>
        public static void StartRunning(string exeName, string path, bool isStart = true)
        {
            try
            {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                //若开机自启动则添加键值对
                if (isStart)
                {
                    key.SetValue(exeName, $"\"{path}\"");
                    key.Close();
                }
                else//否则删除键值对
                {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames)
                    {
                        if (keyName.ToUpper() == exeName.ToUpper())
                        {
                            key.DeleteValue(exeName);
                            key.Close();
                            break;
                        }
                    }
                }
            }
            catch
            {
               
            }
        }
        #endregion
    }
}
