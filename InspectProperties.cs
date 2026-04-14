using TelegramBotBase;
using TelegramBotBase.Builder;
using RMM.Bot;

var botToken = "dummy";
var bot = BotBaseBuilder.Create()
    .WithAPIKey(botToken)
    .DefaultMessageLoop()
    .WithStartForm<StartForm>()
    .Build();

foreach (var property in bot.GetType().GetProperties())
{
    Console.WriteLine($"Property: {property.Name} ({property.PropertyType})");
}
