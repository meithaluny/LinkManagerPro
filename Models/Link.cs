namespace LinkManagerPro.Models
{
    public class Link
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string Slug { get; set; }
        public string Store { get; set; }
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ICollection<Click> Clicks { get; set; } = new List<Click>();
    }
}
