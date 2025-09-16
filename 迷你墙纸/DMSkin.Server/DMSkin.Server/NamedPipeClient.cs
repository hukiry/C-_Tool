using System;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace DMSkin.Server
{
	public class NamedPipeClient : IDisposable
	{
		private string _serverName;

		private string _pipName;

		private NamedPipeClientStream _pipeClient;

		private bool _disposed;

		public NamedPipeClient(string serverName, string pipName)
		{
			_serverName = serverName;
			_pipName = pipName;
			_pipeClient = new NamedPipeClientStream(serverName, pipName, PipeDirection.InOut);
		}

		public ServerMsg Query(ServerMsg request)
		{
			if (!_pipeClient.IsConnected)
			{
				_pipeClient.Connect(10000);
			}
			StreamWriter streamWriter = new StreamWriter(_pipeClient);
			streamWriter.WriteLine(JsonConvert.SerializeObject(request));
			streamWriter.WriteLine();
			streamWriter.WriteLine();
			streamWriter.WriteLine("#END");
			streamWriter.Flush();
			return JsonConvert.DeserializeObject<ServerMsg>(new StreamReader(_pipeClient).ReadToEnd());
		}

		public void Dispose()
		{
			if (!_disposed && _pipeClient != null)
			{
				_pipeClient.Dispose();
				_disposed = true;
			}
		}
	}
}
