using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using 轻压Install.Properties;

namespace 轻压Install
{
    class InstallManager
    {
        public static InstallManager ins { get; } = new InstallManager();
        /// <summary>
        /// 开始安装
        /// </summary>
        /// <param name="_this"></param>
        public void EnableRegistry(Form1 form, string dirPath, string exePath, bool isDelete, string softName)
        {
            Thread.Sleep(500);
            form.label1.Text = "准备就绪...";
            //1, 自己定义安装程序，默认路径
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            string exeName = Path.GetFileNameWithoutExtension(exePath);
            //2，拷贝程序
            File.WriteAllBytes(exeName + ".exe", Resources.轻压app);
            string icoPath = Path.Combine(dirPath, $"{exeName}.ico");
            if (!File.Exists(icoPath))
            {
                using (FileStream fs = new FileStream(icoPath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    Resources.sip.Save(fs);
                }
            }
            form.label1.Text = "安装中...";
            Thread.Sleep(500);
            File.Copy(exeName + ".exe", exePath, true);
            form.label1.Text = "拷贝...";
            Thread.Sleep(500);
            //3, 注册程序
            this.StartRegedit(dirPath, exePath, true);
            form.label1.Text = "注册程序...";
            Thread.Sleep(500);
            //4, 创建快捷方式
            this.CreateShortcut(dirPath, softName);
            Thread.Sleep(500);
            form.label1.Text = "创建快捷方式...";
            this.StartLoadServer(dirPath);
            form.label1.Text = "注册服务...";
            Thread.Sleep(500);
            //5，默认开机启动
            this.StartRunning(exeName, exePath, false);
            Thread.Sleep(500);
            form.label1.Text = "默认开机启动...";
            File.Delete(exeName + ".exe");
            this.StartReg(dirPath, exePath);
            form.label1.Text = "安装完成...";
            Thread.Sleep(500);
            
            if (isDelete)
            {
                //6，删除自己 
                this.KillSelf();
            }
            else
            {
                form.InistallFinish();
            }

        }

        private void WriteFile(string FilePAHT,byte[] buffer, string text="")
        {
            FileInfo fileInfo = new FileInfo(FilePAHT);
            if (fileInfo.Exists)
            {
                Console.WriteLine("delete:" + FilePAHT);
                File.Delete (FilePAHT);
            }

            

            if (buffer != null)
            {
                File.WriteAllBytes(FilePAHT, buffer);
            }
            else
            {
                File.WriteAllText(FilePAHT, text);
            }
        }

        private void StartLoadServer(string dirPath)
        {

            this.KillProcess("explorer");
            Process.Start("explorer.exe");
            
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryMenuExtension)}.dll", false);
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryThumbnailExtendion)}.dll", false);
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryPropertySheet)}.dll", false);

            Thread.Sleep(100);
            try
            {
                this.WriteFile(dirPath + $"\\{nameof(Resources.HukiryThumbnailExtendion)}.dll", Resources.HukiryThumbnailExtendion);//缩略图注册
                this.WriteFile(dirPath + $"\\{nameof(Resources.HukiryMenuExtension)}.dll", Resources.HukiryMenuExtension);//菜单注册
                this.WriteFile(dirPath + $"\\{nameof(Resources.HukiryPropertySheet)}.dll", Resources.HukiryPropertySheet);//属性注册
               
                Thread.Sleep(100);

                this.WriteFile(dirPath + $"\\{nameof(Resources.SharpShell)}.dll", Resources.SharpShell);
                this.WriteFile(dirPath + $"\\{nameof(Resources.ServerRegistrationManager)}.exe", Resources.ServerRegistrationManager);
                this.WriteFile(dirPath + $"\\{nameof(Resources.installThumbnail)}.bat", null, Resources.installThumbnail);
                this.WriteFile(dirPath + $"\\{nameof(Resources.uninstallThumbnail)}.bat", null, Resources.uninstallThumbnail);
            }
            catch (Exception ex)
            {
                Thread.Sleep(100);
                Console.WriteLine(ex);
            }
           

            Thread.Sleep(200);
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryMenuExtension)}.dll");

            Thread.Sleep(100);
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryThumbnailExtendion)}.dll");

            Thread.Sleep(100);
            this.InstallRegAsmPath(dirPath + $"\\{nameof(Resources.HukiryPropertySheet)}.dll");
        }

        public void KillProcess(string processName)
        {
            try
            {
                foreach (Process item in Process.GetProcessesByName(processName))
                {
                    item.Kill();
                    item.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(processName, ex);
            }


        }

        [DllImport("kernel32.dll")]
        public static extern uint WinExec(string lpCmdLine, uint uCmdShow);
        private void KillSelf(int waitDelay = 500)
        {
            Task.Run(async () =>
            {
                await Task.Delay(waitDelay);
                string vBatFile = "DeleteItself.bat";
                using (StreamWriter vStreamWriter = new StreamWriter(vBatFile, false, Encoding.Default))
                {
                    vStreamWriter.Write(string.Format(
                    ":del\r\n" +
                    " del \"{0}\"\r\n" +
                    "if exist \"{0}\" goto del\r\n" +
                    "del %0\r\n", Application.ExecutablePath));
                }
                //************ 执行批处理
                WinExec(vBatFile, 0);
                //************ 结束退出
                Application.Exit();
            });
        }

        //启动注册文件
        private void StartReg(string dirPath, string exePath)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                string dirPATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
                string fileName =  "TEMP.reg";
                string tempFilePath = Path.Combine(dirPATH, fileName);
                File.WriteAllText(tempFilePath, Resources.轻压.Replace("##", exePath).Replace("&&", dirPath));
                await Task.Delay(100);
                File.Copy(tempFilePath, dirPath + "\\" + fileName);
                await Task.Delay(100);
                File.SetAttributes(dirPath + "\\" + fileName, FileAttributes.Hidden);
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("*\\shell\\Hukiry");
                await Task.Delay(100);
                File.Delete(tempFilePath);
                if (registryKey == null)
                {
                    try
                    {
                        System.Diagnostics.Process proc = new System.Diagnostics.Process();
                        proc.StartInfo.WorkingDirectory = "";
                        proc.StartInfo.FileName = fileName;
                        proc.StartInfo.Arguments = "regedit " + fileName;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.Start();

                        proc.WaitForExit();
                        proc.Close();
                        proc.Dispose();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        //创建快捷键
        #region 创建快捷键
        private void CreateShortcut(string dirPath, string softName)
        {
            string linkDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Hukiry");
            string pathLink = Path.Combine(linkDirPath, "轻压.lnk");
            if (!File.Exists(pathLink))
            {
                if (!Directory.Exists(linkDirPath))
                {
                    Directory.CreateDirectory(linkDirPath);
                }

                IWshRuntimeLibrary.IWshShell_Class shell = new IWshRuntimeLibrary.IWshShell_Class();
                IWshRuntimeLibrary.IWshShortcut wshShortcut = shell.CreateShortcut(pathLink);
                wshShortcut.TargetPath = dirPath + $"\\{softName}.exe";
                wshShortcut.WorkingDirectory = dirPath;
                wshShortcut.Description = "hxp软压缩";
                wshShortcut.IconLocation = dirPath + $"\\{softName}.exe";
                wshShortcut.Save();
            }
        } 
        #endregion
        #region 注册表

        private void StartRegedit(string dirPath, string ExecutablePath, bool isAND = false)
        {
            try
            {
                this.AddSystemRegedit(dirPath, ExecutablePath, isAND);
                this.RegistryExe(ExecutablePath, isAND);
                this.CreateSubKeyToFile(ExecutablePath, "sip", isAND);
                this.CreateSubKeyToFile(ExecutablePath, "hxp", isAND);
                this.CreateSubKeyToDirectory(ExecutablePath, isAND);
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

        /// <summary>
        /// 文件图标+新建+打开
        /// </summary>
        /// <param name="ExecutablePath"></param>
        /// <param name="ext"></param>
        /// <param name="isAnd"></param>
        /// <param name="type"></param>
        private void CreateSubKeyToFile(string ExecutablePath, string ext, bool isAnd = false, string type = "File")
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey($".{ext}", true);
            if (registryKey == null || isAnd)
            {
                registryKey = Registry.ClassesRoot.CreateSubKey($".{ext}", true);
                registryKey?.SetValue("", $"{type}.{ext}");
                registryKey?.SetValue("Content Type", $"{type}/{ext}");
                //添加邮件新建文件
                RegistryKey registrySub = registryKey?.CreateSubKey("ShellNew", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("NullFile", "");
                //添加文件图标
                registryKey = Registry.ClassesRoot.CreateSubKey($"{type}.{ext}", true);
                if (ext.ToLower().Equals("hxp"))
                {
                    registryKey?.SetValue("", "轻压文件");//添加新建文件名称
                }
                else
                {
                    registryKey?.SetValue("", "");
                }
                registrySub = registryKey?.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", ExecutablePath);

                //添加双击打开命令
                registrySub = registryKey?.CreateSubKey("shell\\open\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
                registrySub?.Close();

            }
            registryKey?.Close();
        }

        /// <summary>
        /// 注册到系统
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="ExecutablePath"></param>
        /// <param name="isAnd"></param>
        private void AddSystemRegedit(string dirPath, string ExecutablePath, bool isAnd = false)
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
                registrySub?.SetValue("Publisher", "HUKIRY开发者");//开发者
                registrySub?.SetValue("InstallDate", DateTime.Now.ToString("yyyyddMM"));//安装日期
                registrySub?.SetValue("UninstallString", dirPath + $"{nameof(Resources.HukiryThumbnailExtendion)}.bat");//卸载执行路径
                FileInfo fi = new FileInfo(ExecutablePath);
                
                registrySub?.SetValue("EstimatedSize", fi.Length/1024F, RegistryValueKind.DWord);//安装尺寸
                registrySub?.SetValue("NoModify", 1, RegistryValueKind.DWord);//无更新
                registrySub?.SetValue("NoRepair", 1, RegistryValueKind.DWord);//无修复

                registrySub.Close();
            }
            registryKey?.Close();

        }

        /// <summary>
        /// 开机启动
        /// </summary>
        /// <param name="isStart">默认开机启动是、</param>
        /// <param name="exeName">执行文件名</param>
        /// <param name="path">执行文件路径</param>
        /// <returns></returns>
        private bool StartRunning(string exeName, string path, bool isStart = true)
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
                    key.SetValue(exeName, path);
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
                return false;
            }
            return true;
        } 
        #endregion
        #region 安装服务

        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="serviceName">无扩展名</param>
        /// <param name="exePath">服务应用程序路径</param>
        /// <param name="param">参数</param>
        private void InstallServer(string serviceName, string exePath, string param)
        {
            if (ServiceController.GetServices().Length > 0 &&
                ServiceController.GetServices()[0].ServiceName == serviceName)
            {
                return;
            }
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "Hukiry";
            string arguments = string.Format("create \"{0}\" binPath= \"\"{1}\"   {2}\" start= auto", serviceName, exePath, param);
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        /// <summary>
        /// 卸载服务
        /// </summary>
        /// <param name="serviceName"></param>
        private void UninstallServer(string serviceName)
        {
            if (ServiceController.GetServices().Length <= 0) return;

            ServiceController service = new ServiceController(serviceName);
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                }

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "Hukiry";
                string arguments = string.Format("delete \"{0}\"", serviceName);
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();
                proc.WaitForExit();
                proc.Close();
                proc.Dispose();
            }
        }
        
        /// <summary>
        /// 安装控件库 支持32和64位操作系统
        /// </summary>
        /// <param name="dllPath">dll 文件路径</param>
        private void InstallRegAsmPath(string dllPath, bool isInstall=true)
        {
            string frameworkFolder = Environment.Is64BitOperatingSystem ? "Framework64" : "Framework";
            //  This function essentially will set for folders inside the 'Framework' folder, then
            //  build a path to a hypothetical 'regasm.exe' in the folder:
            //  C:\WINDOWS\Microsoft.Net\Framework\v1.0.3705\regasm.exe
            //  C:\WINDOWS\Microsoft.Net\Framework\v1.1.4322\regasm.exe
            //  C:\WINDOWS\Microsoft.Net\Framework\v2.0.50727\regasm.exe
            //  C:\WINDOWS\Microsoft.Net\Framework\v4.0.30319\regasm.exe
            //  It will then sort descending, and pick the first regasm which actually exists, or return null.
            //  Build an array of candidate paths - these are paths which *might* point to a valid regasm executable.
            var searchRoot = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"Microsoft.Net", frameworkFolder);
            var frameworkDirectories = Directory.GetDirectories(searchRoot, "v*", SearchOption.TopDirectoryOnly);
            var candidates = frameworkDirectories.Select(c => Path.Combine(c, @"regasm.exe")).ToArray();
            //  Sort descending, i.e. we're shooting for the latest framework available.
            var sorted = candidates.OrderByDescending(s => s);
            //  Return the first element which exists, or null.
            var regasmPath = sorted.Where(File.Exists).FirstOrDefault();
            //  If we failed to find the path, boot an exception.
            if (regasmPath == null)
            {
                throw new InvalidOperationException($"Failed to find regasm in '{searchRoot}'. Checked: {Environment.NewLine + string.Join(Environment.NewLine, candidates)}");
            }

            string arguments = isInstall?  $"/codebase \"{dllPath}\"": $"/u \"{dllPath}\"" ;
            var regasm = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = regasmPath,// regasm.exe
                    Arguments = arguments,// /codebase "xxx.dll"
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            regasm.Start();
            regasm.WaitForExit();
            regasm.Close();
            regasm.Dispose();
        }
        #endregion
    }
}
