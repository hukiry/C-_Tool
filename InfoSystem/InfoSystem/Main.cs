using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InfoSystem
{

    public partial class Main : Form
    {
        InfoWIn infoWIn;
        public Main()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            StringBuilder sb = new StringBuilder();
            int index = 0;
            Task.Run(async () =>
            {
                try
                {
                    HardwareInfoPart.ins.GetHardware(Win32.Win32_Processor);
                    await Task.Delay(500);
                    await GetInfo(sb);
                    index++;

                }
                catch (Exception e)
                {
                    index++;
                    sb.AppendLine(e.Message + Environment.NewLine + e.StackTrace);
                    MessageBox.Show("引发异常，错误信息已输出到界面，操作已锁定，请关闭重试。", "程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            while (index >= 0)
            {
                Thread.Sleep(200);
                if (index == 1)
                {
                    break;
                }
            }

            infoWIn = new InfoWIn(sb.ToString(), this);

        }


        public async Task GetInfo(StringBuilder sb)
        {
            /* 逐个获取信息*/
            // 获取计算机名
            sb.AppendLine($"计算机名称：{HardwareInfo.GetComputerName() + Environment.NewLine}");
            sb.AppendLine("-----------------------");

            // 操作系统
            sb.AppendLine($"操作系统信息：{HardwareInfo.GetOSInformation() + Environment.NewLine}");
            sb.AppendLine("-----------------------");

            // 处理器和时钟频率
            sb.AppendLine($"CPU处理器：");
            sb.AppendLine($"{HardwareInfo.GetProcessorInformation() + Environment.NewLine}");
            sb.AppendLine("-----------------------");

            // BIOS
            sb.AppendLine($"BIOS：{HardwareInfo.GetBIOScaption() + Environment.NewLine}");
            sb.AppendLine("-----------------------");

            // 主板
            sb.AppendLine($"主板：{HardwareInfo.GetBoardMaker() + Environment.NewLine}");
            sb.AppendLine("-----------------------");

            // 内存总量
            sb.AppendLine($"主板总内存插槽数：{HardwareInfo.GetPhysicalMemorySlots()}");
            sb.AppendLine($"已安装内存总量：{HardwareInfo.GetPhysicalMemoryCapacity()}");
            sb.AppendLine("-----------------------");

            // 内存信息
            sb.AppendLine($"物理内存条详情信息：");
            sb.AppendLine($"{HardwareInfo.GetPhysicalMemory()}");
            sb.AppendLine("-----------------------");

            // 分区
            sb.AppendLine($"分区详情信息：");
            sb.AppendLine($"{HardwareInfo.GetLogicalDiskPartition()}");
            sb.AppendLine("-----------------------");

            // 硬盘
            sb.AppendLine($"磁盘驱动器：");
            sb.AppendLine($"{HardwareInfo.GetDiskDriveInfo()}");
            sb.AppendLine("-----------------------");
            // USB
            sb.AppendLine($"USB 控制器：");
            sb.AppendLine($"{HardwareInfo.GetUSBController()}");
            sb.AppendLine("-----------------------");
            // 网卡
            sb.AppendLine($"网络适配器：");
            sb.AppendLine($"{HardwareInfo.GetNetworkAdapters()}");
            sb.AppendLine("-----------------------");

            // 显卡
            sb.AppendLine($"显示适配器：");
            sb.AppendLine($"{HardwareInfo.GetDisplayAdapters()}");
            sb.AppendLine("-----------------------");

            // 声卡
            sb.AppendLine($"声卡信息：");
            sb.AppendLine($"{HardwareInfo.GetSoundAdapters()}");

            await Task.Delay(500);
        }

        private void Main_Load(object sender, EventArgs e)
        {
           
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            infoWIn?.ShowDialog();
            this.Close();
        }
    }
}
