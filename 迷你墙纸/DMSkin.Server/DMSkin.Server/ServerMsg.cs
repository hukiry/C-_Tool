namespace DMSkin.Server
{
	public class ServerMsg
	{
		private ServerMsgType serverMsgTypeVar;

		public ServerMsgType ServerMsgType
		{
			get
			{
				return serverMsgTypeVar;
			}
			set
			{
				serverMsgTypeVar = value;
			}
		}

		public string Value { get; set; }

		public int IntValue { get; set; }
		public float floatValue { get; set; }
		public bool boolValue { get; set; }
	}
}
