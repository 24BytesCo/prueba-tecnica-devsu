namespace Bancalite.Persitence.Model
{
    /// <summary>
    /// Refresh token persistido para rotación y revocación.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;
    }
}

