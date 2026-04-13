using Serilog;
using Microsoft.EntityFrameworkCore;
using RMM.Data;
using RMM.Bot;
using RMM.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Database Setup (via ENV)
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. MVC Setup
builder.Services.AddControllersWithViews();

// 4. Telegram Bot as Background Service
builder.Services.AddHostedService<TelegramBotWorker>();
// The forms must be registered in DI for TelegramBotBase to resolve them
builder.Services.AddTransient<StartForm>();
builder.Services.AddTransient<SettingsForm>();
builder.Services.AddTransient<EditNameForm>();

var app = builder.Build();

// Auto-Migrations on start (useful for Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/health", () => "OK");

app.Run();
