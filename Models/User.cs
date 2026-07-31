namespace LinkManagerPro.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }  // ← أضف ?
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Link>? Links { get; set; }
    }

}
