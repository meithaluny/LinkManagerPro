namespace LinkManagerPro.Models
{
    public class Click
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public DateTime ClickedAt { get; set; }
        public string? UserAgent { get; set; }  // ← أضف ?
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public Link? Link { get; set; }
    }

}
