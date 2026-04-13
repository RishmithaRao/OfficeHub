using System.ComponentModel.DataAnnotations;

namespace OfficeHub.Data.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    public string Status { get; set; } = "Active";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
