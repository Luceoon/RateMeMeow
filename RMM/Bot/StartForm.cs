using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace RMM.Bot;

public class StartForm : FormBase
{
    public override async Task Load(MessageResult message)
    {
        if (message.Handled) return;

        if (message.MessageText?.ToLower() == "hello")
        {
            await Device.Send("Hello there! 🐾");
            message.Handled = true;
            return;
        }
        
        if (message.MessageText?.ToLower() == "meow")
        {
            await Device.Send("MEOW! 🐾");
            message.Handled = true;
            return;
        }
        
        if (message.MessageText?.ToLower() == "good boy" || message.MessageText?.ToLower() == "good girl")
        {
            await Device.Send("nyaaaa~ >~<");
            message.Handled = true;
            return;
        }

        await Device.Send("Meow");
    }

    public override async Task Action(MessageResult message)
    {
        // Commands are handled in TelegramBotWorker for now to simplify DI access
    }
}
