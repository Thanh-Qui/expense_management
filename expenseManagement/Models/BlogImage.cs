namespace expenseManagement.Models
{
    public class BlogImage
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Blog? Blog { get; set; }
    }
}
