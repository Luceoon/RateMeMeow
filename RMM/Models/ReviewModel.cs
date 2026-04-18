using System.ComponentModel.DataAnnotations;

namespace RMM.Models;

public class Review
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // State management
    public bool WasRated { get; set; } = false;

    // Payload (null until WasRated is true)
    public string? ReviewerName { get; set; }
    public bool? WasGood { get; set; } 
    public string? Comment { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

