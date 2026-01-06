using Microsoft.AspNetCore.Mvc;
using Okuyanlar.Core.Entities;
using Okuyanlar.Service.Services;
using System.Collections.Generic;
using System.Linq;

namespace Okuyanlar.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        // /Books or /Books/Index
        public IActionResult Index()
        {
            var books = _bookService.GetAllBooks();
            return View(books);
        }

        // /Books/Details/9
        public IActionResult Details(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
                return NotFound();

            return View(book);
        }

        // /Books/Top10
        public IActionResult Top10()
        {
            var topBooks = _bookService.GetTopRatedBooks(10);
            return View(topBooks);
        }

        // POST /Books/Rate
        [HttpPost]
        public IActionResult Rate(int bookId, decimal rating)
        {
            try
            {
                _bookService.RateBook(bookId, rating);
                return Ok(new { success = true, message = "Book rated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

