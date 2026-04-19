using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;

namespace RMM.Bot;

public class HelpForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;

    public HelpForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override async Task Load(MessageResult message)
    {
        await Device.SetAction(Telegram.Bot.Types.Enums.ChatAction.Typing);
        
        var nameStr = "Not Specified";
        var praiseStr = "Not Specified";

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            
        var user = await context.Users.FindAsync(message.DeviceId);
        if (user != null)
        {
            nameStr = user.Name;
            praiseStr = user.PreferredPraise.ToString();
        }
        
        var msg = $"RMM (Rate Me Meow) v[{configuration["Version"]}]\n" +
                  "Created by Luceoon\n\n" +
                  "This bot allows you to request ratings from other people whether you were good today or not.\n" +
                  "Use /rate to create a new rating link (including a handy QR code to use right away).\n" +
                  "Use /myreviews to see a list of all the reviews you already received.\n" +
                  "Use /settings to customize your profile like setting your name and preferred praise.\n" +
                  $"Currently I know you as \"{nameStr}\" and I will use the praise \"{praiseStr}\".";
        
        
        await Device.Send(msg);
        await this.NavigateToStart();
    }
}