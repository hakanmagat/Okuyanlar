using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Okuyanlar.Data;
using Okuyanlar.Web.Models;
using Okuyanlar.Core.Entities;

namespace Okuyanlar.Web.Controllers;

/// <summary>
/// Manages the public-facing storefront (Home Page).
/// Handles book listing, searching, and general navigation logic.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly OkuyanlarDbContext _context;

    public HomeController(ILogger<HomeController> logger, OkuyanlarDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// The main landing page.
    /// Fetches and displays all active books from the database.
    /// Supports filtering by title, author, or ISBN via the search parameter.
    /// Note: Pagination is currently disabled; displays all matching results.
    /// </summary>
    /// <param name="searchString">Optional keyword to filter books by title, author, or ISBN.</param>
    /// <returns>A view containing the list of books.</returns>
    public async Task<IActionResult> Index(string searchString)
    {
        // 1. Base Query: Start with all 'Active' books. 
        // AsNoTracking is used to improve performance since this is a read-only operation.
        IQueryable<Book> booksQuery = _context.Books.AsNoTracking()
                                                    .Where(b => b.IsActive);

        // 2. Search Logic: Apply filter if a search term is provided.
        if (!string.IsNullOrEmpty(searchString))
        {
            string filter = searchString.ToLower();
            booksQuery = booksQuery.Where(s => s.Title.ToLower().Contains(filter) ||
                                               s.Author.ToLower().Contains(filter) ||
                                               s.ISBN.Contains(filter));
        }

        // 3. Ordering: Sort by ID descending (show newest books first).
        booksQuery = booksQuery.OrderByDescending(b => b.Id);

        // Keep the search term in the UI (e.g., in the search box).
        ViewData["CurrentFilter"] = searchString;

        // 4. Execution: Execute the query and retrieve all results as a list (No pagination).
        var allBooks = await booksQuery.ToListAsync();

        return View(allBooks);
    }

    /// <summary>
    /// Displays the privacy policy page.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Handles global error display using the standard ErrorViewModel.
    /// This is triggered by the exception handling middleware.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}