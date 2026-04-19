using TelegramBotBase.Base;
using TelegramBotBase.Form;
using RMM.Data;
using RMM.Utils;
using QRCoder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace RMM.Bot;

public class RateForm : FormBase
{
    private readonly IServiceProvider _serviceProvider;

    public string? ReviewerName { get; set; }

    public RateForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task Load(MessageResult message)
    {
        await Device.Send("Ok, give me a moment to prepare your review link... 🐾");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure user exists
        var user = await context.Users.FindAsync(Device.DeviceId);
        if (user == null)
        {
            user = new Models.User
            {
                TelegramId = Device.DeviceId,
                Name = $"User {Device.DeviceId}",
                PreferredPraise = Models.Praise.NotSpecified
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var emptyReview = new Models.Review()
        {
            UserId = user.TelegramId,
            ReviewerName = string.IsNullOrWhiteSpace(ReviewerName) ? null : ReviewerName.Trim(),
        };

        context.Reviews.Add(emptyReview);
        await context.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5012";
        var encodedId = Base62Converter.Encode(emptyReview.Id);
        var reviewUrl = $"{baseUrl}/r/{encodedId}";

        await Device.Send($"Your unique review link:\n{reviewUrl}");

        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(reviewUrl, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            using var ms = new MemoryStream(qrCodeBytes);
            await Device.SendPhoto(ms, "Scan this QR Code to rate.");
        }

        await this.NavigateToStart();
    }
}
