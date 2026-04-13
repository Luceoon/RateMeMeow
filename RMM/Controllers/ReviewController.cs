using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMM.Data;
using RMM.Models;
using RMM.Utils;

namespace RMM.Controllers;

[Route("Review")]
[Route("r")]
public class ReviewController : Controller
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
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
        return View("Success");
    }
}
