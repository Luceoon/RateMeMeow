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
    private readonly TelegramBotService _botService;
    private BotBase? _bot;

    public TelegramBotWorker(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<TelegramBotWorker> logger, ITelegramBotService botService)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
        _botService = (TelegramBotService)botService;
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

        _botService.SetBot(_bot);

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
                var onboardingForm = _serviceProvider.GetRequiredService<OnboardingForm>();
                await e.Device.ActiveForm.NavigateTo(onboardingForm);
                e.Handled = true;
                break;
            case "/rate":
                var rateForm = _serviceProvider.GetRequiredService<RateForm>();
                if (e.Parameters != null && e.Parameters.Count > 0)
                    rateForm.ReviewerName = string.Join(" ", e.Parameters);
                await e.Device.ActiveForm.NavigateTo(rateForm);
                e.Handled = true;
                break;
            case "/myreviews":
                var reviewsForm = _serviceProvider.GetRequiredService<MyReviewsForm>();
                await e.Device.ActiveForm.NavigateTo(reviewsForm);
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

}
