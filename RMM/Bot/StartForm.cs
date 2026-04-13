using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace RMM.Bot;

public class StartForm : FormBase
{
    public override async Task Load(MessageResult message)
    {
        await this.Device.Send("Meow");
    }

    public override async Task Action(MessageResult message)
    {
        // Commands are handled in TelegramBotWorker for now to simplify DI access
    }
}
