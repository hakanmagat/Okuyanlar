using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Okuyanlar.Core.Entities;
using Okuyanlar.Data;
using Okuyanlar.Web.Controllers;

namespace Okuyanlar.Tests
{
    public class HomeControllerTests
    {
        private OkuyanlarDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OkuyanlarDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var ctx = new OkuyanlarDbContext(options);
            return ctx;
        }

        [Fact]
        public async Task Index_ReturnsView_WithActiveBooks_Filtered()
        {
            var ctx = CreateContext();
            ctx.Books.AddRange(new[]
            {
                new Book { Id = 1, Title = "Clean Code", Author = "Uncle Bob", ISBN = "111", IsActive = true },
                new Book { Id = 2, Title = "Some Book", Author = "Author", ISBN = "222", IsActive = false },
                new Book { Id = 3, Title = "Clean Architecture", Author = "Uncle Bob", ISBN = "333", IsActive = true }
            });
            ctx.SaveChanges();

            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<HomeController>();
            var controller = new HomeController(logger, ctx);

            var result = await controller.Index("clean") as ViewResult;
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(result!.Model);
            // Only active books with 'clean' in title should be included, ordered by Id desc
            Assert.Equal(new[] { 3, 1 }, model.Select(b => b.Id).ToArray());
            Assert.Equal("clean", controller.ViewData["CurrentFilter"]);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            var controller = new HomeController(new Microsoft.Extensions.Logging.Abstractions.NullLogger<HomeController>(), CreateContext());
            var result = controller.Privacy();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsView_WithModel()
        {
            var controller = new HomeController(new Microsoft.Extensions.Logging.Abstractions.NullLogger<HomeController>(), CreateContext());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            var result = controller.Error() as ViewResult;
            Assert.NotNull(result);
            Assert.NotNull(result!.Model);
        }
    }
}
