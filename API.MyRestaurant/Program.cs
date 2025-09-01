using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ?? Kestrel tuning: listen on localhost + all interfaces (for mobile access)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Localhost binding
    serverOptions.ListenLocalhost(7297);

    // AnyIP binding (for mobile / external devices)
    serverOptions.ListenAnyIP(7297, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    // Improve stability for mobile clients
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// ?? JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ?? Controllers, Swagger, CORS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ?? CORS
app.UseCors("AllowAll");

// ?? Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyRestaurant API V1");
    c.RoutePrefix = string.Empty; // Swagger opens at http://<IP>:7297
});

// ?? Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// ?? Map controllers
app.MapControllers();

// ?? Run the API
app.Run();

#if DEBUG
try
{
    var swaggerUrl = "http://localhost:7297/swagger";
    Process.Start(new ProcessStartInfo
    {
        FileName = swaggerUrl,
        UseShellExecute = true
    });
}
catch (Exception ex)
{
    Console.WriteLine($"[??] Could not launch browser automatically: {ex.Message}");
}
# endif