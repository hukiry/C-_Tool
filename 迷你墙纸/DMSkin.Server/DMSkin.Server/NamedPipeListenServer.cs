using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DMSkin.Server
{
	public class NamedPipeListenServer
	{
		private List<NamedPipeServerStream> _serverPool = new List<NamedPipeServerStream>();

		private string _pipName = "test";

		public Action<ServerMsg, NamedPipeServerStream> ProcessMessage;

		public NamedPipeListenServer(string pipName)
		{
			_pipName = pipName;
		}

		protected NamedPipeServerStream CreateNamedPipeServerStream()
		{
			NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(_pipName, PipeDirection.InOut, 10);
			_serverPool.Add(namedPipeServerStream);
			return namedPipeServerStream;
		}

		protected void DistroyObject(NamedPipeServerStream npss)
		{
			npss.Close();
			if (_serverPool.Contains(npss))
			{
				_serverPool.Remove(npss);
			}
		}

		public void Run()
		{
			Task.Run(delegate
			{
				using NamedPipeServerStream namedPipeServerStream = CreateNamedPipeServerStream();
				namedPipeServerStream.WaitForConnection();
				new Action(Run).BeginInvoke(null, null);
				try
				{
					bool flag = true;
					while (flag)
					{
						string text = null;
						string text2 = null;
						StringBuilder stringBuilder = new StringBuilder();
						StreamReader streamReader = new StreamReader(namedPipeServerStream);
						while (namedPipeServerStream.CanRead && (text = streamReader.ReadLine()) != null)
						{
							if (text == "#END")
							{
								text2 = stringBuilder.ToString();
								if (text2.EndsWith("\r\n\r\n"))
								{
									break;
								}
							}
							else if (text == "")
							{
								stringBuilder.AppendLine();
							}
							else
							{
								stringBuilder.AppendLine(text);
							}
						}
						text2 = text2.Substring(0, text2.Length - "\r\n\r\n\r\n".Length);
						ProcessMessage(JsonConvert.DeserializeObject<ServerMsg>(text2), namedPipeServerStream);
						if (!namedPipeServerStream.IsConnected)
						{
							flag = false;
							break;
						}
						Thread.Sleep(50);
					}
				}
				catch (IOException ex)
				{
					Console.WriteLine("ERROR: {0}", ex.Message);
				}
				finally
				{
					DistroyObject(namedPipeServerStream);
				}
			});
		}

		public void Stop()
		{
			for (int i = 0; i < _serverPool.Count; i++)
			{
				NamedPipeServerStream npss = _serverPool[i];
				DistroyObject(npss);
			}
		}
	}
}
