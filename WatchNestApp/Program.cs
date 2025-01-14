
using WatchNestApp.Middleware;
using WatchNestApp.Models.Implementation;
using WatchNestApp.Models.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedSqlServerCache(opts =>
{
    //Change to your own connection to store cache, set to localDB
    opts.ConnectionString = builder.Configuration.GetConnectionString("LocalDB");
    opts.SchemaName = "dbo";
    opts.TableName = "AppCache";
});

builder.Services.AddMvc();

builder.Services.AddHttpsRedirection(opts =>
{
    opts.HttpsPort = 44301;
});

builder.Services.AddAuthentication("CookieAuth").AddCookie("CookieAuth",opts =>
{
    opts.Cookie.Name = "AuthToken";
    opts.LoginPath = "/Home/Index";
    opts.LogoutPath = "/Home/Logout";
    opts.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddHttpClient("APIClient",client =>
{
    client.BaseAddress = new Uri("https://localhost:44350/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).SetHandlerLifetime(TimeSpan.FromMinutes(30));

builder.Services.AddSession();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IWatchListService, WatchListService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

app.UseMiddleware<JwtClaimsMiddleware>();

//Added these for development and testing purposes,
//normally, would be in production
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/ErrorCode", "?code={0}");

}

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapDefaultControllerRoute();

app.Run();

