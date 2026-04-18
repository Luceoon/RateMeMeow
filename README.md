




For development only:

To test locally you need to set the bot token. You can get one from [@BotFather](https://t.me/BotFather) onm Telegram.

```bash
cd .\RMM\

# only needed once
dotnet user-secrets init

# set the bot token (or change it)
dotnet user-secrets set "BotToken" "[YourBotToken]"
```