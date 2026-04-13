using TelegramBotBase;
using TelegramBotBase.Builder;
using RMM.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Commands;
using RMM.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using RMM.Utils;
using User = RMM.Models.User;

namespace RMM.Services;

public class TelegramBotWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotWorker> _logger;
    private BotBase? _bot;

    public TelegramBotWorker(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<TelegramBotWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? _configuration["BotToken"];

        if (string.IsNullOrEmpty(botToken))
        {
            _logger.LogWarning("Telegram Bot Token is missing. Bot will not start.");
            return;
        }

        _logger.LogInformation("Starting Telegram Bot...");

        _bot = BotBaseBuilder.Create()
            .WithAPIKey(botToken)
            .DefaultMessageLoop()
            .WithStartForm<StartForm>()
            .NoProxy()
            .CustomCommands(cmd =>
            {
                cmd.Start("Starts the bot and triggers initial setup");
                cmd.Add("rate", "Creates a new rating request and sends the corresponding QR-Code");
                cmd.Add("myreviews", "Lists all received reviews");
                cmd.Help("Shows the help menu");
                cmd.Settings("Shows settings");
            })
            .NoSerialization()
            .UseEnglish()
            .UseThreadPool()
            .Build();

        // Register custom command handling
        _bot.BotCommand += HandleBotCommand;
       

        // Update Bot commands to botfather
        await _bot.UploadBotCommands();

        // Run the bot
        await _bot.Start();

        // Keep the service running until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await _bot.Stop();
        }
    }

    private async Task HandleBotCommand(object sender, BotCommandEventArgs e)
    {
        switch (e.Command)
        {
            case "/start":
                break;
            case "/rate":
                await HandleRateCommand(e.Device);
                e.Handled = true;
                break;
            case "/myreviews":
                await HandleMyReviewsCommand(e.Device);
                e.Handled = true;
                break;
            case "/help":
                await e.Device.Send("[Todo: Implement Help here!] :3");
                e.Handled = true;
                break;
            case "/settings":
                if (e.Device.ActiveForm is not SettingsForm)
                {
                    var settingsForm = _serviceProvider.GetRequiredService<SettingsForm>();
                    await e.Device.ActiveForm.NavigateTo(settingsForm);
                }

                e.Handled = true;
                break;
            default:
                if (e.Command != null && e.Command.StartsWith("/"))
                {
                    await e.Device.Send(
                        $"Sowwy, Me didn't get that. Try /help if you need an overview of the commands.");
                    e.Handled = true;
                }

                break;
        }
    }

    private async Task HandleRateCommand(IDeviceSession session)
    {
        await session.Send("Ok, give me a moment to prepare your review link... ??");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure user exists
        var user = await context.Users.FindAsync(session.DeviceId);
        if (user == null)
        {
            user = new Models.User
            {
                TelegramId = session.DeviceId,
                Name = $"User {session.DeviceId}",
                PreferredPraise = Models.Praise.NotSpecified
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var emptyReview = new Models.Review()
        {
            UserId = user.TelegramId,
        };

        context.Reviews.Add(emptyReview);
        await context.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5012";
        var encodedId = Base62Converter.Encode(emptyReview.Id);
        var reviewUrl = $"{baseUrl}/r/{encodedId}";

        await session.Send($"Your unique review link:\n{reviewUrl}");

        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(reviewUrl, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            using var ms = new MemoryStream(qrCodeBytes);
            await session.SendPhoto(ms, "Scan this QR Code to rate.");
        }
    }

    private async Task HandleMyReviewsCommand(IDeviceSession session)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reviews = await context.Reviews
            .Where(r => r.UserId == session.DeviceId && r.WasRated)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        var user = await context.Users.FindAsync(session.DeviceId);

        if (!reviews.Any())
        {
            await session.Send("You haven't received any reviews yet.");
            return;
        }

        var praiseGenderText =
            $"Were you a good {(user.PreferredPraise == RMM.Models.Praise.GoodBoy ? "boy" : "girl")}?";
        var messageText =
            $"Well let's see... {(user.PreferredPraise == RMM.Models.Praise.GoodJob ? "Did you do a good job?" : praiseGenderText)}:\n\n";
        foreach (var r in reviews)
        {
            var wasGood = r.WasGood ?? false;
            var result = wasGood ? "? Yes" : "? No";
            messageText += $"{result} from {r.ReviewerName}: {r.Comment} ({r.SubmittedAt:yyyy-MM-dd})\n";
        }

        await session.Send(messageText);
    }
}
