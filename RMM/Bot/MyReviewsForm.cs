using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace RMM.Bot;

public class MyReviewsForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;

    public MyReviewsForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        await Device.SetAction(Telegram.Bot.Types.Enums.ChatAction.Typing);
        
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reviews = await context.Reviews
            .Where(r => r.UserId == Device.DeviceId && r.WasRated)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        var user = await context.Users.FindAsync(Device.DeviceId);

        if (!reviews.Any())
        {
            await Device.Send("You haven't received any reviews yet.");
            await this.NavigateToStart();
            return;
        }

        var praiseGenderText =
            $"Were you a good {(user.PreferredPraise == RMM.Models.Praise.GoodBoy ? "boy" : "girl")}?";
        var messageText =
            $"Well let's see... {(user.PreferredPraise == RMM.Models.Praise.GoodJob ? "Did you do a good job?" : praiseGenderText)}:\n\n";
        foreach (var r in reviews)
        {
            var wasGood = r.WasGood ?? false;
            var result = wasGood ? "✅ Yes" : "❌ No";
            messageText += $"{result} from {r.ReviewerName}: {r.Comment} ({r.SubmittedAt:yyyy-MM-dd})\n";
        }

        await Device.Send(messageText);

        await this.NavigateToStart();
    }
}
