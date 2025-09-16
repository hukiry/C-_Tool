using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using ����ǽֽ;

namespace DMSkin.Wallpaper.API
{
	public class WindowAPI
	{
        private EventWaitHandle ProgramStarted;
        private int SW_SHOWNOMAL=1;

        internal enum AccentState
		{
			ACCENT_DISABLED,
			ACCENT_ENABLE_GRADIENT,
			ACCENT_ENABLE_TRANSPARENTGRADIENT,
			ACCENT_ENABLE_BLURBEHIND,
			ACCENT_ENABLE_ACRYLICBLURBEHIND,
			ACCENT_INVALID_STATE
		}

		internal struct AccentPolicy
		{
			public AccentState AccentState;

			public uint AccentFlags;

			public uint GradientColor;

			public uint AnimationId;
		}

		public struct WindowCompositionAttributeData
		{
			public WindowCompositionAttribute Attribute;

			public IntPtr Data;

			public int SizeOfData;
		}

		public enum WindowCompositionAttribute
		{
			WCA_ACCENT_POLICY = 19
		}

		[DllImport("user32.dll")]
		public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

		///<summary>
		/// �ú��������ɲ�ͬ�̲߳����Ĵ��ڵ���ʾ״̬
		/// </summary>
		/// <param name="hWnd">���ھ��</param>
		/// <param name="cmdShow">ָ�����������ʾ���鿴����ֵ�б������ShowWindow������˵������</param>
		/// <returns>�������ԭ���ɼ�������ֵΪ���㣻�������ԭ�������أ�����ֵΪ��</returns>
		[DllImport("User32.dll")]
		private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

		/// <summary>
		///  �ú���������ָ�����ڵ��߳����õ�ǰ̨�����Ҽ���ô��ڡ���������ת��ô��ڣ���Ϊ�û��ĸ��ֿ��ӵļǺš�
		///  ϵͳ������ǰ̨���ڵ��̷߳����Ȩ���Ը��������̡߳� 
		/// </summary>
		/// <param name="hWnd">�������������ǰ̨�Ĵ��ھ��</param>
		/// <returns>�������������ǰ̨������ֵΪ���㣻�������δ������ǰ̨������ֵΪ��</returns>
		[DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

		/// <summary>
		/// ֻ��һ������
		/// </summary>
		/// <param name="e"></param>
		public void OnStartup(StartupEventArgs e, Action action)
		{
			string mutexName = "32283F61-EC4D-43B1-9C44-40280D5854DD";

			ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, mutexName, out var createNew);

			if (!createNew)
			{
				try
				{
					var processes = Process.GetProcessesByName("����ǽֽ");
					if (processes.Length > 0)
					{
						MessageBox.Show("�Ѿ�����");

						Process process = processes.First();
						ShowWindowAsync(process.MainWindowHandle, SW_SHOWNOMAL);
						SetForegroundWindow(process.MainWindowHandle);
						SwitchToThisWindow(process.MainWindowHandle, true);
					}
				}
				catch (Exception exception)
				{
					 MessageBox.Show(exception.ToString(), "��������������ʱ����");
				}

				App.Current.Shutdown();
				Environment.Exit(-1);
			}
			else
			{
				action?.Invoke();
			}
		}
	}
}
