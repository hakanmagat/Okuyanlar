namespace Okuyanlar.Web.Models
{
    public class CatalogBookCardVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public string CoverUrl { get; set; } = "/images/book-placeholder.jpg";
        public double Rating { get; set; }
        public bool IsAvailable { get; set; }
        public string ISBN { get; set; } = "";
        public int Stock { get; set; }
    }

    public class CatalogIndexViewModel
    {
        public string Query { get; set; } = "";
        public string Category { get; set; } = "";
        public string Sort { get; set; } = "";

        public List<string> Categories { get; set; } = new();
        public List<CatalogBookCardVm> Items { get; set; } = new();
    }

    public class Top10ViewModel
    {
        public List<CatalogBookCardVm> Items { get; set; } = new();
    }
}
