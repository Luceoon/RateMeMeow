using System.ComponentModel.DataAnnotations;

namespace RMM.Models;

public enum Praise
{
    NotSpecified,
    GoodBoy,
    GoodGirl,
    GoodJob
}

public class User
{
    [Key] public long TelegramId { get; set; } // Using TelegramId as primary key or unique identifier

    public string Name { get; set; } = string.Empty;

    public Praise PreferredPraise { get; set; } = Praise.NotSpecified;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Review> Reviews { get; set; } = new();
}