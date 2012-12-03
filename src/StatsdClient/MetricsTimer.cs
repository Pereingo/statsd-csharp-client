using System;

namespace StatsdClient
{
	public class MetricsTimer : IDisposable
	{
		private readonly string _name;
		private readonly Stopwatch _stopWatch;
		private bool _disposed;

		public MetricsTimer(string name)
		{
			_name = name;
			_stopWatch = new Stopwatch();
			_stopWatch.Start();
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_stopWatch.Stop();
				Metrics.Timer(_name, _stopWatch.ElapsedMilliseconds());
			}
		}
	}
}