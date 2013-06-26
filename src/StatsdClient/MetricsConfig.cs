namespace StatsdClient
{
	public class MetricsConfig
	{
		public string StatsdServerName { get; set; }
		public int StatsdServerPort { get; set; }
		public string Prefix { get; set; }

		public MetricsConfig()
		{
			StatsdServerPort = 8125;
		}
	}
}
