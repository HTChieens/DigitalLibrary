using DigitalLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserOtpCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string UserId { get; set; } = null!;

    [Required]
    [StringLength(6)]
    public string OtpCode { get; set; } = null!;

    [Required]
    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UsedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}