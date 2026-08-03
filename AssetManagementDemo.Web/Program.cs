using AssetManagementDemo.Web.Extensions;
using AssetManagementDemo.Web.Middleware;
using AssetManagementDemo.Web.Security;   //  Add
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container using Extension Methods
builder.Services.AddControllersWithViews();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerConfiguration();


//  Register API Key Authentication
builder.Services
	.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
	.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
		ApiKeyDefaults.AuthenticationScheme,
		options => { });

//  Register Authorization
builder.Services.AddAuthorization();

//  Register Rate Limiting
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddFixedWindowLimiter("ApiPolicy", policyOptions =>
	{
		policyOptions.PermitLimit = 60;
		policyOptions.Window = TimeSpan.FromMinutes(1);
		policyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		policyOptions.QueueLimit = 0;
	});
});
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Rate Limiter Middleware
app.UseRateLimiter();

//  Authentication Middleware
app.UseAuthentication();

// Authorization Middleware
app.UseAuthorization();

app.UseSwaggerUIConfiguration();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();