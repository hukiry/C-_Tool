using DMSkin.Server;
using DMSkin.WPF.API;
using System;
using System.IO.Pipes;
using System.Windows.Forms;

namespace XunleiPlayer
{
    static class Program
    {
		private static Player play;

		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
            NamedPipeListenServer namedPipeListenServer = new NamedPipeListenServer("Play.Server");
            namedPipeListenServer.ProcessMessage = ProcessMessage;
            namedPipeListenServer.Run();
            play = new XunleiPlayer.Player();
            play.InDeskTop();
            Application.Run(play);
		}

		public static void ProcessMessage(ServerMsg msg, NamedPipeServerStream pipeServer)
        {
            //Console.WriteLine("--------------"+ msg.ServerMsgType);

            switch (msg.ServerMsgType)
			{
				case ServerMsgType.OpenUrl:
					Execute.OnUIThread(delegate
					{
						play?.Play(msg.Value, msg.IntValue);
					});
					break;
				case ServerMsgType.Volume:
					Execute.OnUIThread(delegate
					{
						play?.SetVolume(msg.IntValue);
					});

					break;

				case ServerMsgType.TaskBar:
						Execute.OnUIThread(delegate
						{
							play?.SetTaskBar();
						});
					break;
			}
			pipeServer.Close();
		}
	}
}
