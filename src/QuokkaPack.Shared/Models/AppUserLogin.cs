namespace QuokkaPack.Shared.Models
{
    public class AppUserLogin
    {
        public int Id { get; set; }
        public string Provider { get; set; } = string.Empty; // e.g., "entra", "google"
        public string ProviderUserId { get; set; } = string.Empty; // e.g., sub claim
        public string Issuer { get; set; } = string.Empty; // e.g., iss claim
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public Guid MasterUserId { get; set; }
        public MasterUser MasterUser { get; set; } = default!;
    }
}
