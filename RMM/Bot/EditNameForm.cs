using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using Microsoft.Extensions.DependencyInjection;

namespace RMM.Bot;

public class EditNameForm : AutoCleanForm
{
    private readonly IServiceProvider _serviceProvider;

    public FormBase? ReturnForm { get; set; }

    private bool nameSubmited = false;

    public EditNameForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        if (message.Handled) return;

        if (message.MessageText.Trim() == "") return;

        if (!nameSubmited && message.MessageText != null && message.RawData == null)
        {
            await UpdateName(message.MessageText);
            nameSubmited = true;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (!nameSubmited)
        {
            await Device.Send("Please enter your preferred display name:");
            return;
        }

        var nextForm = ReturnForm ?? _serviceProvider.GetRequiredService<SettingsForm>();
        await this.NavigateTo(nextForm);
    }

    private async Task UpdateName(string newName)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.FindAsync(this.Device.DeviceId);

        if (user != null)
        {
            user.Name = newName;
            await context.SaveChangesAsync();
        }
    }
}
