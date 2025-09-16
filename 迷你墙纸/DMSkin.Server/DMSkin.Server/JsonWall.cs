using System.Collections.Generic;

namespace DMSkin.Server
{
    public class JsonWall
	{
		public string url;
		public double volume;
		public bool isStartEnable;
		public bool isBackRun;
		public uint opacity = 1;

		public List<string> urlList = new List<string>();

	}
}
