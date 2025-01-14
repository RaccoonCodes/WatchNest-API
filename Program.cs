using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WatchNest.Models;
using WatchNest.Models.Implementation;
using WatchNest.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WatchNest.Constants;
using WatchNest.Swagger;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opts =>
{
    opts.CacheProfiles.Add("NoCache", new CacheProfile()
    {
        Location = ResponseCacheLocation.None,
        NoStore = true
    });

    opts.CacheProfiles.Add("Any-60", new CacheProfile()
    {
        Location = ResponseCacheLocation.Any,
        Duration = 60
    });

}); //For API 

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    });

    opts.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });

});

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
    //Change to your own connection, set to localDB
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:LocalDB"]);
});

builder.Services.AddDistributedSqlServerCache(opts =>
{
    //Change to your own connection, set to localDB
    opts.ConnectionString = builder.Configuration.GetConnectionString("LocalDB");
    opts.SchemaName = "dbo";
    opts.TableName = "AppCache";
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddIdentity<ApiUsers,IdentityRole>(opts =>
{
    //Password Requirements
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
    opts.Password.RequiredLength = 10;
}).AddEntityFrameworkStores<ApplicationDbContext>(); //Uses DbContext for storage

//Prevent forgery tokens
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultScheme =
    opts.DefaultAuthenticateScheme =
    opts.DefaultSignInScheme =
    opts.DefaultSignOutScheme =
    opts.DefaultChallengeScheme =
    opts.DefaultForbidScheme =
    JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
        ClockSkew = TimeSpan.Zero // No extra buffer time
    };
    opts.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AuthToken"];
            return Task.CompletedTask;
        }
    };
}).AddCookie(opts =>
{
    opts.Cookie.Name = "AuthToken";
    opts.Cookie.HttpOnly = true; // Ensure the cookie is not accessible via JavaScript
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only send over HTTPS
    opts.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Expire cookies after 30 mins
});



builder.Services.AddSwaggerGen(opts =>
{
    // Authorization and endpoints testing for client side
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description ="Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme ="bearer"
    });

    //Adjusted Endpoint that Require Authorization and/or login 
    opts.OperationFilter<AuthRequirementFilter>();
    opts.EnableAnnotations();
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserServices, UserService>();
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddHttpsRedirection(opts =>
{
    opts.HttpsPort = 44350;
});

builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();
app.UseHttpsRedirection();

//Global No cache 
app.Use((context, next) =>
{
    context.Response.Headers["cache-control"] = "no-cache, no-store";
    return next.Invoke();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

    app.MapGet("/auth/test/1",
    [Authorize,ResponseCache(NoStore = true)]
    () =>
    {
       return Results.Ok("You are authorized!");
    });

    app.MapGet("/auth/test/rbac",
    [ResponseCache(CacheProfileName = "NoCache")]
    [Authorize]
    (HttpContext httpContext) =>
    {
        var user = httpContext.User;
        if (user.IsInRole(RoleNames.Administrator))
        {
            return Results.Ok("You are Authorized as Admin!");
        }
        else if (user.IsInRole(RoleNames.User))
        {
            return Results.Ok("You are not an Admin! Go back!");
        }
        return Results.Forbid();
    });
}

else
{
    app.UseExceptionHandler("/error");
}

app.MapControllers();

app.Map("/",()=> "Add \"/swagger/index.html\" to the end of the URL to access Swagger Testing ");

app.MapGet("/error", () => Results.Problem());

app.Run();


