namespace AssetManagementDemo.McpServer.Constants
{
    public static class McpConstants
    {
        public const string JsonRpcVersion = "2.0";
        public const string ProtocolVersion = "2024-11-05";
        public const string ServerName = "AssetManagementDemo.McpServer";
        public const string ServerVersion = "1.0.0";

        public static class Methods
        {
            public const string Initialize = "initialize";
            public const string Ping = "ping";
            public const string ToolsList = "tools/list";
            public const string ToolsCall = "tools/call";
        }

        public static class ErrorCodes
        {
            public const int ParseError = -32700;
            public const int InvalidRequest = -32600;
            public const int MethodNotFound = -32601;
            public const int InvalidParams = -32602;
            public const int InternalError = -32603;
        }
    }
}
