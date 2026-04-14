using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using RMM.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RMM.Bot;

public class SelectPraiseForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;

    public FormBase? ReturnForm { get; set; }

    public SelectPraiseForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        if (message.Handled || message.RawData != null) return;

        var bf = new ButtonForm();
        bf.AddButtonRow("Good Boy", "gender_boy");
        bf.AddButtonRow("Good Girl", "gender_girl");
        bf.AddButtonRow("Good Job", "gender_job");
        await this.Device.Send("Select your preferred praise:", bf);
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled) return;

        switch (message.RawData)
        {
            case "gender_boy":
                await UpdateGender(Praise.GoodBoy);
                break;
            case "gender_girl":
                await UpdateGender(Praise.GoodGirl);
                break;
            case "gender_job":
                await UpdateGender(Praise.GoodJob);
                break;
            case "cancel_gender":
                var nextForm = ReturnForm ?? _serviceProvider.GetRequiredService<SettingsForm>();
                await this.NavigateTo(nextForm);
                break;
        }
    }

    private async Task UpdateGender(Praise praise)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.FindAsync(this.Device.DeviceId);
        if (user != null)
        {
            user.PreferredPraise = praise;
            await context.SaveChangesAsync();
            await this.Device.Send($"Preferred gender updated to: {praise}");
        }

        var nextForm = ReturnForm ?? _serviceProvider.GetRequiredService<SettingsForm>();
        await this.NavigateTo(nextForm);
    }
}
