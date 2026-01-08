using Microsoft.EntityFrameworkCore;
using Okuyanlar.Core.Entities;
using Okuyanlar.Core.Interfaces;

namespace Okuyanlar.Data.Repositories
{
    /// <summary>
    /// Implementation of IBookReservationRepository using Entity Framework Core.
    /// </summary>
    public class BookReservationRepository : IBookReservationRepository
    {
        private readonly OkuyanlarDbContext _context;

        public BookReservationRepository(OkuyanlarDbContext context)
        {
            _context = context;
        }

        public BookReservation? GetById(int id)
        {
            return _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == id);
        }

        public IEnumerable<BookReservation> GetByUserId(int userId)
        {
            return _context.BookReservations
                .Include(r => r.Book)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservedAt)
                .ToList();
        }

        public IEnumerable<BookReservation> GetActiveReservationsByUserId(int userId)
        {
            return _context.BookReservations
                .Include(r => r.Book)
                .Where(r => r.UserId == userId && r.Status == "Active")
                .OrderByDescending(r => r.ReservedAt)
                .ToList();
        }

        public IEnumerable<BookReservation> GetByBookId(int bookId)
        {
            return _context.BookReservations
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.ReservedAt)
                .ToList();
        }

        public IEnumerable<BookReservation> GetAllActiveReservations()
        {
            return _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == "Active")
                .OrderByDescending(r => r.ReservedAt)
                .ToList();
        }

        public IEnumerable<BookReservation> GetCheckInRequests()
        {
            return _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == "Active" && r.CheckInRequested)
                .OrderBy(r => r.CheckInRequestedAt)
                .ToList();
        }

        public void Add(BookReservation reservation)
        {
            _context.BookReservations.Add(reservation);
        }

        public void Update(BookReservation reservation)
        {
            _context.BookReservations.Update(reservation);
        }

        public void Delete(BookReservation reservation)
        {
            _context.BookReservations.Remove(reservation);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public int GetActiveReservationCountByUserId(int userId)
        {
            return _context.BookReservations
                .Count(r => r.UserId == userId && r.Status == "Active");
        }

        public bool HasActiveReservation(int bookId)
        {
            return _context.BookReservations
                .Any(r => r.BookId == bookId && r.Status == "Active");
        }
    }
}
