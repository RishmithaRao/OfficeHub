using System.ComponentModel.DataAnnotations;

namespace OfficeHub.Data.Models;

public class WorkItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int? AssigneeId { get; set; }
    public Employee? Assignee { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Medium";

    [Required]
    [StringLength(20)]
    public string State { get; set; } = "New";
    public DateTime DueDate { get; set; }
}
