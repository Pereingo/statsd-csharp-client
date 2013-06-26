namespace StatsdClient
{
	public class MetricsConfig
	{
		public string StatsdServerName { get; set; }
        public int StatsdPort { get; set; }
		public string Prefix { get; set; }
	}
}
