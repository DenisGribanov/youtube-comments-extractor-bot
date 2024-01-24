using BotApi.Database;
using BotApi.Services.MessageBroker;
using BotApi.Services.MessageBroker.RabbitClient;
using BotApi.Services.TelegramIntegration;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using BotApi.Services.TasksClient;
using Telegram.Bot;
using BotApi.Services;
using BotApi.Services.DataStore;
using BotApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
    //serverOptions.ListenAnyIP(8443, listenOptions => listenOptions.UseHttps());
});


builder.Services.AddControllers().AddNewtonsoftJson();

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables(prefix: "Development");
}

builder.Services.AddDbContext<YotubeCommentsExtractorBotContext>(options => options.UseNpgsql(builder.Configuration["DB_CONNECTION"]), ServiceLifetime.Scoped);
builder.Services.AddScoped<ITelegram>(p => new TelegramAdapter(new TelegramBotClient(builder.Configuration["BOT_TOKEN"])));
builder.Services.AddScoped<IMessageBrokerPub>(p => new RabbitClientPub(builder.Configuration["RABBIT_HOST"], builder.Configuration["RABBIT_USER"], builder.Configuration["RABBIT_PASS"]));

var app = builder.Build();

app.UseRouting();

var optionsBuilder = new DbContextOptionsBuilder<YotubeCommentsExtractorBotContext>().UseNpgsql(builder.Configuration["DB_CONNECTION"]).Options;
Variables.InitInstance(builder.Configuration);

app.SubscribeDownloadTask(new RabbitClientSub(builder.Configuration["RABBIT_HOST"],
                                                builder.Configuration["RABBIT_USER"],
                                                builder.Configuration["RABBIT_PASS"]),
                        new DataStoreImpl(new YotubeCommentsExtractorBotContext(optionsBuilder)),
                        new TelegramAdapter(new TelegramBotClient(builder.Configuration["BOT_TOKEN"])));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();