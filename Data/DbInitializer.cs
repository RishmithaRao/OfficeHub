using Microsoft.EntityFrameworkCore;
using OfficeHub.Data.Models;

namespace OfficeHub.Data;

public static class DbInitializer
{
    public static async Task EnsureSeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Departments.AnyAsync(cancellationToken))
            return;

        var engineering = new Department { Name = "Engineering" };
        var sales = new Department { Name = "Sales" };
        var operations = new Department { Name = "Operations" };
        db.Departments.AddRange(engineering, sales, operations);
        await db.SaveChangesAsync(cancellationToken);

        var employees = new[]
        {
            new Employee
            {
                FullName = "Alex Morgan",
                Email = "alex.morgan@officehub.local",
                JobTitle = "Engineering Manager",
                DepartmentId = engineering.Id,
                HiredOn = new DateTime(2021, 3, 15)
            },
            new Employee
            {
                FullName = "Jordan Lee",
                Email = "jordan.lee@officehub.local",
                JobTitle = "Senior Developer",
                DepartmentId = engineering.Id,
                HiredOn = new DateTime(2022, 1, 10)
            },
            new Employee
            {
                FullName = "Sam Rivera",
                Email = "sam.rivera@officehub.local",
                JobTitle = "Account Executive",
                DepartmentId = sales.Id,
                HiredOn = new DateTime(2023, 6, 1)
            },
            new Employee
            {
                FullName = "Taylor Chen",
                Email = "taylor.chen@officehub.local",
                JobTitle = "Operations Lead",
                DepartmentId = operations.Id,
                HiredOn = new DateTime(2020, 11, 20)
            }
        };
        db.Employees.AddRange(employees);
        await db.SaveChangesAsync(cancellationToken);

        var portal = new Project
        {
            Name = "Customer Portal Refresh",
            Code = "POR-1042",
            Status = "Active",
            StartDate = DateTime.UtcNow.AddMonths(-2),
            Budget = 185_000m
        };
        var crm = new Project {
            Name = "CRM Integration",
            Code = "CRM-882",
            Status = "Active",
            StartDate = DateTime.UtcNow.AddMonths(-5),
            EndDate = DateTime.UtcNow.AddMonths(1),
            Budget = 92_500m
        };
        db.Projects.AddRange(portal, crm);
        await db.SaveChangesAsync(cancellationToken);

        var alex = employees[0];
        var jordan = employees[1];
        var sam = employees[2];

        db.WorkItems.AddRange(
            new WorkItem
            {
                ProjectId = portal.Id,
                AssigneeId = jordan.Id,
                Title = "Implement SSO with Entra ID",
                Description = "Wire up OIDC and role claims for internal users.",
                Priority = "High",
                State = "Active",
                DueDate = DateTime.UtcNow.AddDays(14)
            },
            new WorkItem
            {
                ProjectId = portal.Id,
                AssigneeId = alex.Id,
                Title = "Define release checklist",
                Description = "Document rollout, rollback, and support handoff.",
                Priority = "Medium",
                State = "New",
                DueDate = DateTime.UtcNow.AddDays(21)
            },
            new WorkItem
            {
                ProjectId = crm.Id,
                AssigneeId = sam.Id,
                Title = "Validate lead sync mapping",
                Description = "Confirm field mapping with sales ops.",
                Priority = "Medium",
                State = "Done",
                DueDate = DateTime.UtcNow.AddDays(-3)
            },
            new WorkItem
            {
                ProjectId = crm.Id,
                AssigneeId = jordan.Id,
                Title = "Build webhook retry policy",
                Description = "Exponential backoff and dead-letter queue.",
                Priority = "High",
                State = "Active",
                DueDate = DateTime.UtcNow.AddDays(7)
            });

        db.Meetings.AddRange(
            new Meeting
            {
                Title = "Quarterly planning",
                StartsAt = DateTime.UtcNow.AddDays(3).Date.AddHours(15),
                DurationMinutes = 60,
                Location = "Conference Room A / Teams",
                OrganizerEmail = alex.Email
            },
            new Meeting
            {
                Title = "Project checkpoint — Portal",
                StartsAt = DateTime.UtcNow.AddDays(1).Date.AddHours(16),
                DurationMinutes = 45,
                Location = "Teams",
                OrganizerEmail = jordan.Email
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
