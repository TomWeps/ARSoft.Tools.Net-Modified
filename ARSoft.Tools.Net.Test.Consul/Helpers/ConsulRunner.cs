using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace ARSoft.Tools.Net.Test.Consul.Helpers
{
	[ExcludeFromCodeCoverage]
	public class ConsulRunner : IDisposable
	{
		public readonly Uri ConsulUri = new Uri("http://127.0.0.1:8500");

		private Process consulProcess;
		private readonly string pathToConsul;

		public ConsulRunner(string pathToConsul)
		{
			this.pathToConsul = pathToConsul;
		}

		public string GetConsoleOutput()
		{
			if (consulProcess != null)
			{
				try
				{
					return consulProcess.StandardOutput.ReadToEnd();
				}
				catch 
				{				
				}				
			}
			return string.Empty;
		}

		public void Init(string consulConfigName)
		{
			// https://www.consul.io/docs/guides/bootstrapping.html

			var processStartInfo = new ProcessStartInfo
			{
				WorkingDirectory = pathToConsul,
				FileName = "consul.exe",
				Arguments = @"agent -bind 127.0.0.1 -client 127.0.0.1 -data-dir .\consul-data  -config-file " + consulConfigName,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			consulProcess = Process.Start(processStartInfo);
			if (consulProcess.HasExited)
			{				
				string error = consulProcess.StandardError.ReadToEnd();
				throw new InvalidOperationException(GetConsoleOutput() + "\n" + error);
			}
			else
			{
				// wait for consul init.
				Thread.Sleep(TimeSpan.FromSeconds(5));
			}
			
		}

		public void Cleanup()
		{
			if (consulProcess != null)
			{
				if (!consulProcess.HasExited)
				{
					consulProcess.Kill();
					Thread.Sleep(200);
				}
				consulProcess = null;
			}

			var dir = new DirectoryInfo(Path.Combine(pathToConsul,"consul-data"));
			if (dir.Exists)
			{
				dir.Delete(true);
			}

			Thread.Sleep(500);
		}

		public void Dispose()
		{
			Cleanup();
		}
	}

}
