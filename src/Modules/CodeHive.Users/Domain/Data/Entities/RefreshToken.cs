using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Domain.Data.Entities;

public sealed class RefreshToken
{
    public Guid Id  { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash  { get; set; } = string.Empty; // SHA-256 hash of raw token
    public DateTime ExpiresAt  { get; set; }
    public DateTime CreatedAt  { get; init; } = DateTime.UtcNow;
    public bool IsRevoked  { get; set; } = false;
    public Guid? ReplacedBy { get; set; } // Id of the new token that replaced this one
}
