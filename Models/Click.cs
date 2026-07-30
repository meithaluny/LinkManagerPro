namespace LinkManagerPro.Models
{
    public class Click
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public DateTime ClickedAt { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }

        // Navigation property
        public Link Link { get; set; }
    }
}
