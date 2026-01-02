using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Okuyanlar.Web.Controllers
{
    public class BooksController : Controller
    {
        // Şimdilik demo data (backend'e bağlamadan UI'yı görmek için)
        private static readonly List<Book> DemoBooks = new()
        {
            new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", Stock = 3, IsActive = true },
            new Book { Id = 2, Title = "Design Patterns", Author = "GoF", ISBN = "978-0201633610", Stock = 0, IsActive = true },
            new Book { Id = 9, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "978-0201616224", Stock = 1, IsActive = true },
        };

        // /Books veya /Books/Index
        public IActionResult Index()
        {
            return View(DemoBooks);
        }

        // /Books/Details/9
        public IActionResult Details(int id)
        {
            var book = DemoBooks.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound();

            return View(book);
        }
    }
}
