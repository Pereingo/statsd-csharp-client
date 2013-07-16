namespace StatsdClient
{
	public class MetricsConfig
	{
		public string StatsdServerName { get; set; }
		public int StatsdServerPort { get; set; }
		public int StatsdMaxUDPPacketSize { get; set; }
		public string Prefix { get; set; }
		public const int DefaultStatsdServerPort = 8125;
		public const int DefaultStatsdMaxUDPPacketSize = 512;

		public MetricsConfig()
		{
			StatsdServerPort = DefaultStatsdServerPort;
			StatsdMaxUDPPacketSize = DefaultStatsdMaxUDPPacketSize;
		}
	}
}
