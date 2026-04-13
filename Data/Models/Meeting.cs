using System.ComponentModel.DataAnnotations;

namespace OfficeHub.Data.Models;

public class Meeting
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime StartsAt { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; } = 30;

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string OrganizerEmail { get; set; } = string.Empty;
}
