using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMM.Data;
using RMM.Models;

namespace RMM.Controllers;

public class ReviewController : Controller
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Submit(Guid id)
    {
        var link = await _context.ReviewLinks.Include(l => l.Review).Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (link == null || link.IsUsed) return View("Invalid");
        return View(link);
    }

    [HttpPost]
    public async Task<IActionResult> SaveReview(Guid reviewLinkId, string reviewerName, bool rating, string comment)
    {
        var link = await _context.ReviewLinks.FindAsync(reviewLinkId);
        if (link == null || link.IsUsed) return BadRequest();
        var review = new Review
        {
            ReviewLinkId = reviewLinkId, ReviewerName = reviewerName ?? "Anonymous", WasGood = rating,
            Comment = comment ?? string.Empty
        };
        link.IsUsed = true;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return View("Success");
    }
}
