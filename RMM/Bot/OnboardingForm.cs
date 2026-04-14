using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using RMM.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RMM.Bot;

public class OnboardingForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;

    public OnboardingForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await context.Users.FindAsync(Device.DeviceId);

            if (user == null)
            {
                var chatUser = await Device.GetChatUser(Device.DeviceId);
                var defaultName = chatUser?.User.FirstName ?? "Unknown";

                user = new User
                {
                    TelegramId = Device.DeviceId,
                    Name = defaultName,
                    PreferredPraise = Praise.NotSpecified
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                await Device.Send("Welcome! Let's get you set up. 🐾");

                var startForm = new StartForm();
                var selectPraiseForm = _serviceProvider.GetRequiredService<SelectPraiseForm>();
                selectPraiseForm.ReturnForm = startForm;

                var editNameForm = _serviceProvider.GetRequiredService<EditNameForm>();
                editNameForm.ReturnForm = selectPraiseForm;

                await NavigateTo(editNameForm);
            }
            else
            {
                // If user already exists, just go to StartForm
                var startForm = new StartForm();
                await NavigateTo(startForm);
            }
        }
    }
}
