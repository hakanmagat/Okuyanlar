using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Interfaces;
using Okuyanlar.Web.Models;

namespace Okuyanlar.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IBookRepository _bookRepository;

        public CatalogController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        // /Catalog
        [HttpGet]
        public IActionResult Index(string? q, string? category, string? sort)
        {
            // Note: Repository names may differ in your implementation.
            // Adapt the lines below to match your repository interface.
            var books = _bookRepository.GetAll(); // IEnumerable<Book>

            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.Trim().ToLowerInvariant();
                books = books.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.Title) && b.Title.ToLower().Contains(query)) ||
                    (!string.IsNullOrWhiteSpace(b.Author) && b.Author.ToLower().Contains(query))
                );
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var cat = category.Trim().ToLowerInvariant();
                books = books.Where(b => (b.Category ?? "").ToLower() == cat);
            }

            // sort: "title", "new", "rating"
            books = sort switch
            {
                "title" => books.OrderBy(b => b.Title),
                "new" => books.OrderByDescending(b => b.CreatedAt),
                _ => books.OrderBy(b => b.Title)
            };

            var model = new CatalogIndexViewModel
            {
                Query = q ?? "",
                Category = category ?? "",
                Sort = sort ?? "",
                Items = books.Select(b => new CatalogBookCardVm
                {
                    Id = b.Id,
                    Title = b.Title ?? "",
                    Author = b.Author ?? "",
                    Category = b.Category ?? "",
                    CoverUrl = string.IsNullOrWhiteSpace(b.CoverUrl) ? "/images/book-placeholder.jpg" : b.CoverUrl!,
                    IsAvailable = b.Stock > 0,
                    ISBN = b.ISBN ?? "",
                    Stock = b.Stock,
                }).ToList()
            };

            model.Categories = model.Items
                .Select(x => x.Category)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return View(model);
        }

        // /Catalog/Top10
        [HttpGet]
        public IActionResult Top10()
        {
            var books = _bookRepository.GetAll();

            var top = books
                .OrderByDescending(b => b.Category)
                .ThenBy(b => b.Title)
                .Take(10)
                .Select(b => new CatalogBookCardVm
                {
                    Id = b.Id,
                    Title = b.Title ?? "",
                    Author = b.Author ?? "",
                    Category = b.Category ?? "",
                    CoverUrl = string.IsNullOrWhiteSpace(b.CoverUrl) ? "/images/book-placeholder.jpg" : b.CoverUrl!,
                    IsAvailable = b.Stock > 0,
                    ISBN = b.ISBN ?? "",
                    Stock = b.Stock,
                })
                .ToList();

            var model = new Top10ViewModel
            {
                Items = top
            };

            return View(model);
        }
    }
}
