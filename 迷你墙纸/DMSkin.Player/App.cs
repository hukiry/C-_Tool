// DMSkin.Player.App
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO.Pipes;
using System.Windows;
using DMSkin.Player;
using DMSkin.Server;
using DMSkin.WPF.API;
using Microsoft.Win32;

public class App : Application
{
	private Player p;

	protected override void OnStartup(StartupEventArgs e)
	{
		
		base.OnStartup(e);
		Execute.InitializeWithDispatcher();
		base.ShutdownMode = ShutdownMode.OnExplicitShutdown;
		NamedPipeListenServer namedPipeListenServer = new NamedPipeListenServer("Play.Server");
		namedPipeListenServer.ProcessMessage = ProcessMessage;
		namedPipeListenServer.Run();

	}

	public void ProcessMessage(ServerMsg msg, NamedPipeServerStream pipeServer)
	{
		switch (msg.ServerMsgType)
		{
			case ServerMsgType.OpenUrl:
				Execute.OnUIThread(delegate
				{
					if (p == null)
					{
						p = new Player();
						p.Show();
						p.InDeskTop();
					}
					p.Play(msg.Value.ToString(), msg.IntValue, msg.boolValue);
				});
				break;
			case ServerMsgType.Volume:
				Execute.OnUIThread(delegate
				{
					p?.SetVolume(msg.IntValue);
				});
				break;
			case ServerMsgType.TaskBar:
				if (msg.boolValue)
				{
				  Execute.OnUIThread(delegate
				  {
					  p?.SetTaskBar();
				  });
				}
				break;
		}
		pipeServer.Close();
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

	[STAThread]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public static void Main()
	{
		new App().Run();
	}
}
