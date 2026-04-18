using TelegramBotBase;

namespace RMM.Services;

public interface ITelegramBotService
{
    BotBase? GetBot();
}

public class TelegramBotService : ITelegramBotService
{
    private BotBase? _bot;

    public void SetBot(BotBase bot)
    {
        _bot = bot;
    }

    public BotBase? GetBot() => _bot;
}
