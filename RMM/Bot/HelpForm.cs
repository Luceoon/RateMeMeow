using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using RMM.Models;

namespace RMM.Bot;

public class HelpForm : FormBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public HelpForm(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    public override async Task Load(MessageResult message)
    {
        var nameStr = "Not Specified";
        var praiseStr = "Not Specified";
            
        var user = await _context.Users.FindAsync(message.DeviceId);
        if (user != null)
        {
            nameStr = user.Name;
            praiseStr = user.PreferredPraise.ToString();
        }
        
        var msg = $"RMM (Rate Me Meow) [{_configuration["Version"]}]\n" +
                  "Created by Luceoon\n\n" +
                  "This bot allows you to request ratings from other people whetar were you good today or not.\n" +
                  "Use /rate to create a new rating link (including a handy qr code to use right away).\n" +
                  "Use /myreviews to see a list of all the reviews you already received.\n" +
                  "Use /settings to customize your profile like setting your name and preferred praise.\n" +
                  $"Currently I know you as \"{nameStr}\" and I will use the praise \"{praiseStr}\".";
        
        
        await Device.Send(msg);
        await this.NavigateToStart();
    }
}