using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AssetManagementDemo.Web.Security
{
	public class ApiKeyAuthenticationHandler
		: AuthenticationHandler<ApiKeyAuthenticationOptions>
	{
		private readonly IConfiguration _configuration;

		public ApiKeyAuthenticationHandler(
			IOptionsMonitor<ApiKeyAuthenticationOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			IConfiguration configuration)
			: base(options, logger, encoder)
		{
			_configuration = configuration;
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			// Step 1: Check whether the X-API-Key header exists
			if (!Request.Headers.TryGetValue(ApiKeyDefaults.HeaderName, out var apiKeyHeader))
			{
				return Task.FromResult(
					AuthenticateResult.Fail("API Key header is missing."));
			}

			// Step 2: Read the expected API Key from appsettings.json
			var configuredApiKey = _configuration["ApiSecurity:ApiKey"];

			// Step 3: Compare the received key with the configured key
			if (!string.Equals(apiKeyHeader, configuredApiKey))
			{
				return Task.FromResult(
					AuthenticateResult.Fail("Invalid API Key."));
			}

			// Step 4: Create an authenticated user
			var claims = new[]
			{
				new Claim(ClaimTypes.Name, "McpServer")
			};

			var identity = new ClaimsIdentity(
				claims,
				ApiKeyDefaults.AuthenticationScheme);

			var principal = new ClaimsPrincipal(identity);

			var ticket = new AuthenticationTicket(
				principal,
				ApiKeyDefaults.AuthenticationScheme);

			// Step 5: Authentication successful
			return Task.FromResult(
				AuthenticateResult.Success(ticket));
		}
	}
}