using System.Diagnostics;

namespace DMSkin.Server
{
	public class PlayServer
	{
		public static PlayerType PlayerType { get; set; }
		private const string SERVER_NAME = "Play.Server";
		private const string PLAYER_NAME = "DMSkin.Player";
		private const string XUNLEI_PLAYER = "XunleiPlayer";
		public const string UrlFileName = "url.json";

		private static ServerMsg msg = new ServerMsg();

		public static void Initialize()
		{
			switch (PlayerType)
			{
			case PlayerType.MN:
				CloseXL();
				StartMN();
				break;
			case PlayerType.XL:
				CloseMN();
				StartXL();
				break;
			}
		}

		public static void Play(string Url)
		{
			msg.Value = Url;
			msg.ServerMsgType = ServerMsgType.OpenUrl;
			if (!IsPlayerServer()) return;
			using NamedPipeClient namedPipeClient = new NamedPipeClient(".", SERVER_NAME);
			namedPipeClient.Query(msg);
		}

		public static void CloseAll()
		{
			CloseXL();
			CloseMN();
		}

		public static void SetVolume(int Volume)
		{
			msg.IntValue = Volume;
			if (!IsPlayerServer()) return;
			
			msg.ServerMsgType = ServerMsgType.Volume;
			using NamedPipeClient namedPipeClient = new NamedPipeClient(".", SERVER_NAME);
			namedPipeClient.Query(msg);
		}

		public static void SetTaskBar(bool boolValue)
		{
			msg.boolValue = boolValue;
			if (!IsPlayerServer()) return;
			msg.ServerMsgType = ServerMsgType.TaskBar;
			using NamedPipeClient namedPipeClient = new NamedPipeClient(".", SERVER_NAME);
			namedPipeClient.Query(msg);
		}

		private static void StartXL()
		{
			if (Process.GetProcessesByName(XUNLEI_PLAYER).Length == 0)
			{
				Process.Start($"{XUNLEI_PLAYER}.exe");
			}
		}

		private static void CloseXL()
		{
			Process[] processesByName = Process.GetProcessesByName(XUNLEI_PLAYER);
			for (int i = 0; i < processesByName.Length; i++)
			{
				processesByName[i].Kill();
			}
		}

		private static void StartMN()
		{
			if (Process.GetProcessesByName(PLAYER_NAME).Length == 0)
			{
				Process.Start($"{PLAYER_NAME}.exe");
			}
		}

		private static void CloseMN()
		{
			Process[] processesByName = Process.GetProcessesByName(PLAYER_NAME);
			for (int i = 0; i < processesByName.Length; i++)
			{
				processesByName[i].Kill();
			}
		}

		private static bool IsPlayerServer()
		{
			Process[] processesByName =Process.GetProcessesByName(PLAYER_NAME);
			return processesByName != null && processesByName.Length > 0;
		}
	}
}
