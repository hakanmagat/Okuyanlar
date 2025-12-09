using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Web.Models;

namespace Okuyanlar.Web.Controllers;

/// <summary>
/// Manages the landing page and general application views.
/// Acts as the default entry point for the application.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the main landing page.
    /// </summary>
    public IActionResult Index()
    {
        return View();
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
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
