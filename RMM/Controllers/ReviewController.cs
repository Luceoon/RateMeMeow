using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMM.Data;
using RMM.Models;
using RMM.Utils;

using RMM.Services;
using Serilog;
using Telegram.Bot;
using TelegramBotBase.Base;

namespace RMM.Controllers;

[Route("Review")]
[Route("r")]
public class ReviewController : Controller
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITelegramBotService _botService;

    public ReviewController(AppDbContext context, IServiceProvider serviceProvider, ITelegramBotService botService)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _botService = botService;
    }

    [HttpGet("{id}")]
    [HttpGet("Submit/{id}")]
    public async Task<IActionResult> Submit(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        Guid reviewId;
        try
        {
            reviewId = Base62Converter.Decode(id);
        }
        catch
        {
            return BadRequest();
        }

        var review = await _context.Reviews.Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == reviewId);
        if (review == null || review.WasRated) return View("Invalid");
        return View(review);
    }

    [HttpPost("SaveReview")]
    [HttpPost("save")]
    public async Task<IActionResult> SaveReview(Guid reviewId, string reviewerName, bool rating, string comment)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null || review.WasRated) return BadRequest();
        
        review.WasRated = true;
        
        review.ReviewerName = reviewerName ?? "Anonymous";
        review.WasGood = rating;
        review.Comment = comment ?? string.Empty;
        review.SubmittedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        var bot = _botService.GetBot();
        if (bot != null)
        {
            var praise = Praise.NotSpecified;
            
            var user = await _context.Users.FindAsync(review.UserId);
            if (user != null) praise = user.PreferredPraise;

            var ratingStr = "";

            switch (praise)
            {
                case Praise.GoodBoy:
                    ratingStr = review.WasGood == true ? "You were a good boy~ 😸" : "You were no good boy 😿";
                    break;
                case Praise.GoodGirl:
                    ratingStr = review.WasGood == true ? "You were a good girl~ 😸" : "You were no good girl 😿";
                    break;
                default:
                    ratingStr = review.WasGood == true ? "You did a good job! 😸 " : "You did a bad job! 😿";
                    break;
            }
            
            var message = $"Review from {review.ReviewerName} received!\n" +
                          $"{ratingStr}\n" +
                          $"Comment: {review.Comment}";

            try
            {
                await bot.Client.TelegramClient.SendMessage(review.UserId, message);
            }
            catch (Exception e)
            {
                Log.Error("Failed to send message to user. Review is still saved! Error Message: " + e.Message);
            }
        }

        return View("Success");
    }
}
