using System.ComponentModel.DataAnnotations;

namespace OfficeHub.Data.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string JobTitle { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public DateTime HiredOn { get; set; }
}
