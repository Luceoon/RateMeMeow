using System.ComponentModel.DataAnnotations;

namespace RMM.Models;

public class Review
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReviewLinkId { get; set; }
    public ReviewLink ReviewLink { get; set; } = null!;
    
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string ReviewerName { get; set; } = string.Empty;

    public bool WasGood { get; set; } // Binary rating (Yes/No)

    public string Comment { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class ReviewLink
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}


