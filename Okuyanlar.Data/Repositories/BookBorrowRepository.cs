using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Data.Repositories
{
    /// <summary>
    /// Implementation of IBookBorrowRepository using Entity Framework Core.
    /// </summary>
    public class BookBorrowRepository : IBookBorrowRepository
    {
        private readonly OkuyanlarDbContext _context;

        public BookBorrowRepository(OkuyanlarDbContext context)
        {
            _context = context;
        }

        public BookBorrow? GetById(int id)
        {
            return _context.BookBorrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<BookBorrow> GetByUserId(int userId)
        {
            return _context.BookBorrows
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BorrowedAt)
                .ToList();
        }

        public IEnumerable<BookBorrow> GetActiveBorrowsByUserId(int userId)
        {
            return _context.BookBorrows
                .Include(b => b.Book)
                .Where(b => b.UserId == userId && (b.Status == "Active" || b.Status == "Overdue"))
                .OrderByDescending(b => b.BorrowedAt)
                .ToList();
        }

        public IEnumerable<BookBorrow> GetByBookId(int bookId)
        {
            return _context.BookBorrows
                .Include(b => b.User)
                .Where(b => b.BookId == bookId)
                .OrderByDescending(b => b.BorrowedAt)
                .ToList();
        }

        public IEnumerable<BookBorrow> GetAllActiveBorrows()
        {
            return _context.BookBorrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.Status == "Active" || b.Status == "Overdue")
                .OrderByDescending(b => b.BorrowedAt)
                .ToList();
        }

        public IEnumerable<BookBorrow> GetReturnRequests()
        {
            return _context.BookBorrows
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(b => b.ReturnRequested && (b.Status == "Active" || b.Status == "ReturnRequested"))
                .OrderBy(b => b.ReturnRequestedAt)
                .ToList();
        }

        public void Add(BookBorrow borrow)
        {
            _context.BookBorrows.Add(borrow);
        }

        public void Update(BookBorrow borrow)
        {
            _context.BookBorrows.Update(borrow);
        }

        public void Delete(BookBorrow borrow)
        {
            _context.BookBorrows.Remove(borrow);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public int GetActiveBorrowCountByUserId(int userId)
        {
            return _context.BookBorrows
                .Count(b => b.UserId == userId && (b.Status == "Active" || b.Status == "Overdue"));
        }
    }
}
