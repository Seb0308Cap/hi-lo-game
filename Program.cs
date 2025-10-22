using HiLoGame.Hubs;
using HiLoGame.Repositories;
using HiLoGame.Services;
using HiLoGame.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Render.com deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();
builder.Services.AddSingleton<ILoggerService, BrowserLoggerService>();
builder.Services.AddScoped<IGameRepository, SessionGameRepository>();
builder.Services.AddSingleton<IGameHistoryRepository, MemoryCacheGameHistoryRepository>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IRoomRepository, MemoryCacheRoomRepository>();
builder.Services.AddSingleton<IMultiplayerGameService, MultiplayerGameService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<GameHub>("/gamehub");

app.Run();

