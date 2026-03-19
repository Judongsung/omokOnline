using Server.Hubs;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomManager>();

var policyName = builder.Configuration["CorsSettings:PolicyName"] ?? "DefaultPolicy";
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
var allowCredentials = builder.Configuration.GetValue<bool>("CorsSettings:AllowCredentials");

builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
    {
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        { 
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

var app = builder.Build();

app.UseCors(policyName);

app.UseAuthorization();
app.MapControllers();
app.MapHub<OmokHub>("/omokHub");

app.Run();
