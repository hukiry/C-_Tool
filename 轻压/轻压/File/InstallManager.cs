using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using 轻压.Properties;

namespace Filejieyasuo
{
    class InstallManager
    {
        public static InstallManager ins { get; } = new InstallManager();


        /// <summary>
        /// 开始安装
        /// </summary>
        /// <param name="_this"></param>
        public void EnableRegistry(string exePath)
        {
            string exeName = Path.GetFileNameWithoutExtension(exePath);
            //1, 自己定义安装程序，默认路径
            //2，拷贝程序
            this.StartRegedit("");
            this.StartReg();
            this.CreateShortcut();
            this.StartRunning(exeName, exePath);
            this.KillSelf();
            //3, 注册程序
            //4, 创建快捷方式
            //5，默认开机启动
            //6，删除自己 
        }

        [DllImport("kernel32.dll")]
        public static extern uint WinExec(string lpCmdLine, uint uCmdShow);
        private void KillSelf(int waitDelay = 2000)
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
        private void StartReg()
        {
            Task.Run(async () =>
            {
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("*\\shell\\Hukiry");
                if (registryKey == null)
                {
                    await Task.Delay(100);
                    string fileName = "__.reg";
                    File.WriteAllText(fileName, Resources.轻压);
                    File.SetAttributes(fileName, FileAttributes.Hidden);

                    await Task.Delay(100);
                    try
                    {
                        System.Diagnostics.Process proc = new System.Diagnostics.Process();
                        proc.StartInfo.WorkingDirectory = "";
                        proc.StartInfo.FileName = fileName;
                        proc.StartInfo.Arguments = "regedit " + fileName;
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
        private void CreateShortcut()
        {
            string pathLink = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Hukiry\\轻压.lnk";
            if (!Directory.Exists(pathLink))
            {
                string directoryName = Path.GetDirectoryName(pathLink);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                IWshRuntimeLibrary.IWshShell_Class shell = new IWshRuntimeLibrary.IWshShell_Class();
                IWshRuntimeLibrary.IWshShortcut wshShortcut = shell.CreateShortcut(pathLink);
                wshShortcut.TargetPath = Application.StartupPath + "\\轻压.exe";
                wshShortcut.WorkingDirectory = Application.StartupPath;
                wshShortcut.Description = "hxk软压缩";
                wshShortcut.IconLocation = Application.StartupPath + "\\轻压.exe";
                wshShortcut.Save();
            }
        }

        public void StartRegedit(string ExecutablePath)
        {
            try
            {
                this.RegistryExe(ExecutablePath);
                this.CreateSubKeyToFile(ExecutablePath, "Sip");
                this.CreateSubKeyToFile(ExecutablePath, "Hxp");
                this.CreateSubKeyToDirectory(ExecutablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("没有写入权限：" + ex.ToString());
            }
        }

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
        /// <summary>
        /// 关联文件
        /// </summary>
        /// <param name="ExecutablePath"></param>
        private void RegistryExe(string ExecutablePath)
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("*\\shell\\hukiry");
            if (registryKey == null)
            {
                registryKey = Registry.ClassesRoot.CreateSubKey("*\\shell\\hukiry", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registryKey?.SetValue("", "用轻压打开");
                registryKey?.SetValue("Icon", ExecutablePath);
                RegistryKey registrySub = registryKey?.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
                registrySub?.Close();
            }
            registryKey?.Close();
        }

        private void CreateSubKeyToFile(string ExecutablePath, string ext, string type="File")
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey($".{ext}");
            if (registryKey == null)
            {
                registryKey = Registry.ClassesRoot.CreateSubKey($".{ext}");
                registryKey?.SetValue("", $"{type}.{ext}");
                registryKey?.SetValue("Content Type", $"{type}/{ext}");
                registryKey?.SetValue("DefaultIcon", ExecutablePath);

                RegistryKey registrySub = registryKey?.CreateSubKey("shell\\open\\command", RegistryKeyPermissionCheck.ReadWriteSubTree);
                registrySub?.SetValue("", "\"" + ExecutablePath + "\" \"%1\"");
                registrySub?.Close();
            }
            registryKey?.Close();
        }

        private void CreateSubKeyToDirectory(string ExecutablePath)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Directory\\shell\\hukiry");
            if (registryKey == null)
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
        /// 开机启动
        /// </summary>
        /// <param name="isStart">默认开机启动是、</param>
        /// <param name="exeName">执行文件名</param>
        /// <param name="path">执行文件路径</param>
        /// <returns></returns>
        private bool StartRunning( string exeName, string path, bool isStart=true)
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
    }
}
