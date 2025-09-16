using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
public enum Win32
{
    Win32_Processor,
    Win32_PhysicalMemory,
    Win32_Keyboard,
    Win32_PointingDevice,
    Win32_FLoppyDrive,
    Win32_DiskDrive,
    Win32_CDROMDrive,
    Win32_BaseBoard,
    Win32_BIOS,
    Win32_ParallelPort,
    Win32_SerialPort,
    Win32_SerialPortConfiguration,
    Win32_SoundDevice,
    Win32_SystemSlot,
    Win32_USBControlLer,
    Win32_NetworkAdapter,
    Win32_NetworkAdapterConfiguration,
    Win32_Printer,
    Win32_PrinterConfiguration,
    Win32_PrintJob,
    Win32_TCPIPPrinterPort,
    Win32_POTSModem,
    Win32_POTSModemToserialPort,
    Win32_DesktopMonitor,
    Win32_DisplayConfiguration,
    Win32_DisplayControllerConfiguration,
    Win32_VideoController,
    Win32_VideoSettings,
    // 操作系统
    Win32_TimeZone,
    Win32_SystemDriver,
    Win32_DiskPartition,
    Win32_LogicalDisk,
    Win32_LogicalDiskToPartition,
    Win32_LogicalMemoryConfiguratio,
    Win32_PageFile,
    Win32_PageFileSetting,
    Win32_BootConfiguration,
    Win32_ComputerSystem,
    Win32_OperatingSystem,
    Win32_StartupCommand,
    Win32_Service,
    Win32_Group,
    Win32_GroupUser,
    Win32_UserAccount,
    Win32_Process,
    Win32_Thread,
    Win32_Share,
    Win32_NetworkClient,
    Win32_NetworkProtocol
}
public class HardwareInfoPart
{
  
    public static HardwareInfoPart ins { get; } = new HardwareInfoPart();
    public class Hardware
    {
        public ManagementObjectCollection ManagementCollection;
        public string title;
        public string CreationClassName;
        public Dictionary<string, string>[] Properties=new Dictionary<string, string>[0];

        public Hardware(string title, string winClass)
        {
            this.title = title;
            
            ManagementObjectSearcher query = new ManagementObjectSearcher($"SELECT * FROM {winClass}");
            this.ManagementCollection = query?.Get();
            try
            {
                if (this.ManagementCollection != null && this.ManagementCollection.Count > 0)
                {
                    int index = 0;
                    Properties = new Dictionary<string, string>[this.ManagementCollection.Count];
                    foreach (ManagementObject mo in this.ManagementCollection)
                    {
                        Properties[index] = new Dictionary<string, string>();
                        foreach (var item in mo.Properties)
                        {
                            Properties[index][item.Name] = item.Value==null?"":item.Value.ToString();
                            if (item.Name.Equals(nameof(CreationClassName)))
                            {
                                this.CreationClassName = item.Value.ToString();
                            }
                        }
                        index++;
                    }
                   
                }
            }
            catch { }
        }

        public string GetValue(string key)
        {
            if (Properties.Count() > 0)
            {
                if (this.Properties[0].ContainsKey(key)) return this.Properties[0][key];
            }
            return string.Empty;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(this.title + "-------------------------");

            foreach (var mo in this.Properties)
            {
                stringBuilder.AppendLine(title + ":Properties");
                foreach (var item in mo)
                {
                    stringBuilder.AppendLine($"  \t{item.Key}:{item.Value}");

                }
            }
            return stringBuilder.ToString();
        }
    }

    Dictionary<string, Hardware> managementDic = new Dictionary<string, Hardware>();
    private void initData()
    {
        string[] strArray = {
           "CPU 处理器",
           "物理内存条",
           "键盘",
           "输入设备，鼠标",
           "软盘驱动器",
           "硬盘驱动器",
           "光盘驱动器",
           "主版",
           "BIOS 心片",
           "并口",
           "串口",
           "串口配置",
           "多媒体设置，一般指声卡。",
           "主板插槽(ISA & PCI & AGP)",
           "USB 控制器",
           "网络适配器",
           "网络适配器设置",
           "打印机",
           "打印机设置",
           "打印机任务",
           "打印机端口",
           "MODEM",
           "MODEM 端口",
           "显示器",
           "显卡",
           "显卡设置",
           "显卡细节",
           "显卡支持的显示模式",
           "时区",
           "驱动程序",
           "磁盘分区",
           "逻辑磁盘",
           "逻辑磁盘所在分区及的表位置",
           "逻辑内存配置",
           "系统页文件信息",
           "页件设置",
           "系统启动配置",
           "计品信息简要",
           "操作系统信息",
           "系统自动启动程序",
           "系统安装的服务",
           "系统管里组",
           "系统组账号",
           "用户账号",
           "系统进强",
           "系统线强",
           "共享",
           "己安装的网络客户端",
           "己安装的网络协战"
        };

        for (Win32 i = Win32.Win32_Processor; i <= Win32.Win32_USBControlLer; i++)
        {
            string key = i.ToString();
            string title = strArray[(int)i];
            if(!managementDic.ContainsKey(key))
            managementDic[key] = new Hardware(title, key);
        }
    }

    public HardwareInfoPart() => this.initData();
    public Hardware GetHardware(Win32 winClass)=> managementDic.ContainsKey(winClass.ToString())?managementDic[winClass.ToString()] :null;
}

public static class HardwareInfo
{

    /// <summary>
    /// 盘符分区
    /// </summary>
    /// <returns></returns>
    public static String GetLogicalDiskPartition()
    {
        ManagementClass mangnmt = new ManagementClass("Win32_LogicalDisk");
        ManagementObjectCollection mcol = mangnmt.GetInstances();
        StringBuilder sb = new StringBuilder();
        foreach (ManagementObject strt in mcol)
        {
            var serialNumber = Convert.ToString(strt["VolumeSerialNumber"]);
            // 盘符名称
            var n = strt["Name"];
            // 磁盘类型
            var d = strt["Description"];
            // 文件系统(分区类型、格式)
            var fs = strt["FileSystem"];
            // 可用空间
            var fr = Convert.ToInt64(strt["FreeSpace"]) /1024 / 1024 / 1024;
            // 总空间
            var s = Convert.ToInt64(strt["Size"]) / 1024 / 1024 / 1024;
            sb.AppendLine($"盘符{n}>>{d}，文件系统{fs}，可用空间{fr}GB，总空间{s}GB");
        }
        return sb.ToString();
    }


    /// <summary>
    /// 硬盘信息
    /// </summary>
    /// <returns></returns>
    public static String GetDiskDriveInfo()
    {
        ManagementClass mangnmt = new ManagementClass("Win32_DiskDrive");
        ManagementObjectCollection mcol = mangnmt.GetInstances();
        StringBuilder sb = new StringBuilder();
        uint i = 1;
        foreach (ManagementObject mo in mcol)
        {
            var m = mo["Model"];
            var size = Convert.ToInt64(mo["Size"]);
            var sn = mo["SerialNumber"];
            var it = mo["InterfaceType"];
            sb.AppendLine($"硬盘{i}>>{m}，{size / 1024 / 1024 / 1024}GB，序列号{sn}，{it}");
            i++;
        }
        return sb.ToString();
    }


    /// <summary>
    /// 获取主板信息
    /// </summary>
    /// <returns></returns>
    public static string GetBoardMaker()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_BaseBoard");

        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {
                // 主板型号，品牌
                return "\n\t主板型号：" + wmi.GetPropertyValue("Product").ToString()+ "\n\t厂商：" + wmi.GetPropertyValue("Manufacturer").ToString()+
                  "\n\t主板编号：" + wmi.GetPropertyValue("SerialNumber").ToString() ;
            }

            catch { }

        }
        return "(Board)Unknown";

    }

    /// <summary>
    /// BIOS信息
    /// </summary>
    /// <returns></returns>
    public static string GetBIOScaption()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_BIOS");

        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {
                // BIOS型号，品牌
                return wmi.GetPropertyValue("Name").ToString()+"，"+wmi.GetPropertyValue("Manufacturer").ToString();

            }
            catch { }
        }
        return "(Win32_BIOS)Unknown";
    }

    /// <summary>
    /// 获取内存信息
    /// </summary>
    /// <returns></returns>
    public static string GetPhysicalMemory()
    {
        ManagementScope oMs = new ManagementScope();
        ObjectQuery oQuery = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
        ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs,oQuery);
        ManagementObjectCollection oCollection = oSearcher.Get();

        StringBuilder sb = new StringBuilder();
        uint i = 1;
        foreach (ManagementObject obj in oCollection)
        {

            var indexSize = Convert.ToInt64(obj.GetPropertyValue("Capacity"));
            var m = obj.GetPropertyValue("Manufacturer");
            var c = obj.GetPropertyValue("Caption");
            object s;
            try
            {
                // 时钟频率属性在虚拟机测试环境下可能无法获取
                s = obj.GetPropertyValue("ConfiguredClockSpeed")+ "MHz";
            }
            catch (Exception)
            {
                s = "频率未知";
            }
            
            sb.AppendLine($"内存{i}>>{indexSize / 1024 / 1024 / 1024}GB，{s}，{m}，{c}");
            i++;
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// 获取已安装内存总量
    /// </summary>
    /// <returns></returns>
    public static string GetPhysicalMemoryCapacity()
    {
        ManagementScope oMs = new ManagementScope();
        ObjectQuery oQuery = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
        ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs,oQuery);
        ManagementObjectCollection oCollection = oSearcher.Get();

        long installedMemSize = 0;
        foreach (ManagementObject obj in oCollection)
        {
            installedMemSize += Convert.ToInt64(obj["Capacity"]);
        }
        return $"{installedMemSize / 1024 / 1024 / 1024}GB";
    }

    /// <summary>
    /// 获取主板内存插槽数
    /// </summary>
    /// <returns></returns>
    public static string GetPhysicalMemorySlots()
    {
        int MemSlots = 0;
        ManagementScope oMs = new ManagementScope();
        ObjectQuery oQuery2 = new ObjectQuery("SELECT * FROM Win32_PhysicalMemoryArray");
        ManagementObjectSearcher oSearcher2 = new ManagementObjectSearcher(oMs,oQuery2);
        ManagementObjectCollection oCollection2 = oSearcher2.Get();
        foreach (ManagementObject obj in oCollection2)
        {
            MemSlots = Convert.ToInt32(obj["MemoryDevices"]);

        }
        return MemSlots.ToString();
    }

    public static string GetUSBController()
    {
        StringBuilder stringBuilder = new StringBuilder();
        var hardware = HardwareInfoPart.ins.GetHardware(Win32.Win32_USBControlLer);
        stringBuilder.AppendLine("名称："+hardware.GetValue("Name"));
        stringBuilder.AppendLine("厂商："+hardware.GetValue("Manufacturer"));
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取网络适配器信息：名称，ip，网关，mac
    /// </summary>
    /// <returns></returns>
    public static string GetNetworkAdapters()
    {
        ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection objCol = mgmt.GetInstances();
        
        List<ManagementObject> objList = new List<ManagementObject>();
        foreach (ManagementObject obj in objCol) 
        {
            objList.Add(obj);
        }
        objList = objList.OrderByDescending(o => (bool)o.Properties["IPEnabled"].Value == true).ToList();
        StringBuilder sb = new StringBuilder();
        uint i = 1;
        foreach (ManagementObject obj in objList)
        {
            var d = obj["Description"]?.ToString();
            var ipEnabled = obj["IPEnabled"];
            var ip = obj["IPAddress"];
            var gateway = obj["DefaultIPGateway"];
            var mac = obj["MACAddress"];
            if (ip!=null)
            {
                if (((string[])ip).Length > 1)
                {
                    ip = $"IP地址{((string[])ip)[0]}|{((string[])ip)[1]}";
                }
                else
                {
                    ip = $"IP地址{((string[])ip)[0]}";
                }

            }
            else
            {
                ip = $"IP地址暂无";
            }

            if (gateway != null)
            {
                gateway = $"网关{((string[])gateway)[0]}";
            }
            else
            {
                gateway = $"网关暂无";
            }

            if (mac != null)
            {
                mac = $"MAC地址{mac}";

            }
            else
            {
                mac = $"MAC地址暂无";
            }
            sb.AppendLine($"网络适配{i}>>{d}，{ip}，{gateway}，{mac}");
            i++;
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获取显卡信息
    /// </summary>
    /// <returns></returns>
    public static string GetDisplayAdapters()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_VideoController");
        StringBuilder stringBuilder = new StringBuilder();
        uint i = 1;
        foreach (ManagementObject wmi in searcher.Get())
        {
            var n = wmi.GetPropertyValue("Name")?.ToString().Replace("(TM)","™").Replace("(tm)","™").Replace("(R)","®").Replace("(r)","®").Replace("(C)","©").Replace("(c)","©");
            var ram = Convert.ToInt64(wmi.GetPropertyValue("AdapterRAM")) / 1024 / 1024 / 1024;
            var chr = wmi.GetPropertyValue("CurrentHorizontalResolution");
            var cvr = wmi.GetPropertyValue("CurrentVerticalResolution");
            var cr = wmi.GetPropertyValue("CurrentRefreshRate");
            var minR = wmi.GetPropertyValue("MinRefreshRate");
            var maxR = wmi.GetPropertyValue("MaxRefreshRate");
            var dv = wmi.GetPropertyValue("DriverVersion");
            stringBuilder.AppendLine($"显卡{i}>>{n}，{ram}GB RAM，分辨率{chr}x{cvr}，刷新率{cr}Hz，最大可设刷新率{maxR}，最小可设刷新率{minR}，驱动版本{dv}");
            i++;
                        
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取声卡
    /// </summary>
    /// <returns></returns>
    public static string GetSoundAdapters()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT * FROM Win32_SoundDevice");
        StringBuilder stringBuilder = new StringBuilder();
        uint i = 1;
        foreach (ManagementObject wmi in searcher.Get())
        {
            var n = wmi.GetPropertyValue("Name")?.ToString().Replace("(TM)","™").Replace("(tm)","™").Replace("(R)","®").Replace("(r)","®").Replace("(C)","©").Replace("(c)","©");
            var m = wmi.GetPropertyValue("Manufacturer")?.ToString();
            var pn = wmi.GetPropertyValue("ProductName")?.ToString();
            stringBuilder.AppendLine($"声卡{i}>>{n}，{m}");
            i++;

        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// cpu时钟频率
    /// </summary>
    /// <returns></returns>
    public static double? GetCpuSpeedInGHz()
    {
        double? GHz = null;
        using (ManagementClass mc = new ManagementClass("Win32_Processor"))
        {
            foreach (ManagementObject mo in mc.GetInstances())
            {
                // CurrentClockSpeed:单位是MHz
                GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                break;
            }
        }
        return GHz;
    }


    /// <summary>
    /// 获取操作系统信息
    /// </summary>
    /// <returns></returns>
    public static string GetOSInformation()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        ManagementObjectCollection moc = searcher.Get();
        foreach (ManagementObject mo in moc)
        {
            try
            {
                // Windows 10 专业版，10.0.0000，64位
                return ((string)mo["Caption"]).Trim() + "，" + (string)mo["Version"] + "，" + (string)mo["OSArchitecture"];
            }
            catch { }
        }
        return "(Win32_OperatingSystem)Unknown";
    }

    /// <summary>
    /// 获取处理器信息
    /// </summary>
    /// <returns></returns>
    public static String GetProcessorInformation()
    {
        ManagementClass mc = new ManagementClass("win32_processor");
        ManagementObjectCollection moc = mc.GetInstances();
        String info = String.Empty;
        StringBuilder stringBuilder = new StringBuilder();
        foreach (ManagementObject mo in moc)
        {
            // 处理器型号,说明,插槽类型,时钟频率
            string name = (string)mo["Name"];
            name = name.Replace("(TM)","™").Replace("(tm)","™").Replace("(R)","®").Replace("(r)","®").Replace("(C)","©").Replace("(c)","©");
            name = name.TrimEnd(' ');
            info = name + "，" + (string)mo["Caption"] + "，" + (string)mo["SocketDesignation"]+ "插槽";
            info += "，"+GetCpuSpeedInGHz()+"GHz";
        }
        stringBuilder.AppendLine(info);
        var hardware = HardwareInfoPart.ins.GetHardware(Win32.Win32_Processor);
        stringBuilder.AppendLine("\tCPU制造厂商："+hardware.GetValue("Manufacturer"));
        stringBuilder.AppendLine("\tCPU名称：" + hardware.GetValue("Name"));
        stringBuilder.AppendLine("\tCPU最大速度：" + hardware.GetValue("MaxClockSpeed"));
        stringBuilder.AppendLine("\tCPU核心数：" + hardware.GetValue("NumberOfCores"));
        stringBuilder.AppendLine("\tCPU启动核心数：" + hardware.GetValue("NumberOfEnabledCore"));
        stringBuilder.AppendLine("\tCPU逻辑进程数：" + hardware.GetValue("NumberOfLogicalProcessors"));
        stringBuilder.AppendLine("\tCPU序列号：" + hardware.GetValue("ProcessorId"));
        stringBuilder.AppendLine("\tCPU线程数：" + hardware.GetValue("ThreadCount"));
        return stringBuilder.ToString();
    }


    /// <summary>
    /// 获取计算机名
    /// </summary>
    /// <returns></returns>
    public static String GetComputerName()
    {
        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
        ManagementObjectCollection moc = mc.GetInstances();
        String info = String.Empty;
        foreach (ManagementObject mo in moc)
        {
            info = (string)mo["Name"];
        }
        return info;
    }

}


/*
 硬件
Win32_Processor，// CPU 处理器
Win32_PhysicalMemory，// 物理内存案
Win32_Keyboard，// 键盘
Win32_PointingDevice，// 输入设备，鼠标
Win32_FLoppyDrive，// 软盘驱动器
Win32_DiskDrive，// 硬盘驱动器
Win32_CDROMDrive，// 光盘驱动器
Win32_BaseBoard，// 主版
Win32_BIOS，// BIOS 心片
Win32_ParallelPort，// 并口
Win32_SerialPort，// 串口
Win32_SerialPortConfiguration，// 串口配置
Win32_SoundDevice，// 多蔡体战置，一般指声卡。
Win32_SystemSlot，// 主板插槽(ISA & PCI & AGP)
Win32_USBControlLer，// USB 控制器
Win32_NetworkAdapter，// 网络适配器
Win32_NetworkAdapterConfiguration，// 网络适配器设置
Win32_Printer.// 打印机
Win32_PrinterConfiguration，// 打印机设置
Win32_PrintJob，// 打印机任务
Win32_TCPIPPrinterPort，// 打印机端口
Win32_POTSModem，// MODEM
Win32_POTSModemToserialPort，// MODEM 端口
Win32_DesktopMonitor，// 显示器
Win32_DisplayConfiguration，// 显卡
Win32_DisplayControllerConfiguration，// 显卡设置
Win32_VideoController，// 显卡细节
Win32_VideoSettings，// 显卡支持的显示模式


// 操作系统
Win32_TimeZone，// 时区
Win32_SystemDriver，// 驱动程序
Win32_DiskPartition，// 磁盘分区
Win32_LogicalDisk，// 逻辑磁盘
Win32_LogicalDiskToPartition，//逻辑磁盘所在分区及的表位置
Win32_LogicalMemoryConfiguration，// 逻辑内存配置
Win32_PageFile，// 系统页文件信息
Win32_PageFileSetting，// 页件设置
Win32_BootConfiguration，// 系统启动配置
Win32_ComputerSystem，// 计品信息简要
Win32_OperatingSystem，// 操作系统信息
Win32_StartupCommand，// 系统自动启动程序
Win32_Service，// 系统安装的服务
Win32_Group，// 系统管里组
Win32_GroupUser，// 系统组账号
Win32_UserAccount，// 用户账号
Win32_Process，// 系统进强
Win32_Thread，// 系统线强
Win32_Share，// 共享
Win32_NetworkClient，// 己安装的网络客户端
Win32_NetworkProtocol，// 己安装的网络协战
 */