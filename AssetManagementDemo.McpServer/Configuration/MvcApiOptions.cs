namespace AssetManagementDemo.McpServer.Configuration
{
    public class MvcApiOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:5091/";
        public int TimeoutSeconds { get; set; } = 30;
		public string ApiKey { get; set; } = string.Empty;
	}
}
