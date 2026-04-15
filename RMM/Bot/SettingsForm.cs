using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Controls.Hybrid;
using RMM.Data;
using RMM.Models;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Args;
using System.Collections.Concurrent;

namespace RMM.Bot;

public class SettingsForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;
    private User? _user;

    public SettingsForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        if (_user == null)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _user = await context.Users.FindAsync(this.Device.DeviceId);

            if (_user == null)
            {
                var defaultName = "Unknown";
                var chatUser = await this.Device.GetChatUser(this.Device.DeviceId);
                if (chatUser != null) defaultName = chatUser.User.FirstName;

                _user = new User
                {
                    TelegramId = this.Device.DeviceId,
                    Name = defaultName,
                    PreferredPraise = Praise.NotSpecified
                };
                context.Users.Add(_user);
                await context.SaveChangesAsync();
            }
        }

        if (message.Handled || message.RawData != null) return;

        await ShowMainMenu();
    }

    private async Task ShowMainMenu()
    {
        var bf = new ButtonForm();
        bf.AddButtonRow("Edit Name", "edit_name");
        bf.AddButtonRow("Edit Preferred Praise", "edit_gender");
        bf.AddButtonRow("Back to Start", "back");

        await this.Device.Send(
            $"Settings for {_user!.Name}\nGender: {_user.PreferredPraise}\n\nWhat would you like to change?", bf);
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled) return;

        switch (message.RawData)
        {
            case "edit_name":
                var editNameForm = _serviceProvider.GetRequiredService<EditNameForm>();
                editNameForm.ReturnForm = this;
                await this.NavigateTo(editNameForm);
                break;
            case "edit_gender":
                var selectPraiseForm = _serviceProvider.GetRequiredService<SelectPraiseForm>();
                selectPraiseForm.ReturnForm = this;
                await this.NavigateTo(selectPraiseForm);
                break;
            case "back":
                var startForm = _serviceProvider.GetRequiredService<StartForm>();
                await this.NavigateTo(startForm);
                break;
        }
    }
}
