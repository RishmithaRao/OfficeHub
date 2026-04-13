using Microsoft.EntityFrameworkCore;
using OfficeHub.Data;

namespace OfficeHub.Services;

public sealed class DashboardService(AppDbContext db)
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var activeProjects = await db.Projects.CountAsync(p => p.Status == "Active", cancellationToken);
        var openTasks = await db.WorkItems.CountAsync(w => w.State != "Done", cancellationToken);
        var overdueTasks = await db.WorkItems.CountAsync(
            w => w.State != "Done" && w.DueDate.Date < today,
            cancellationToken);
        var headcount = await db.Employees.CountAsync(cancellationToken);
        var meetingsThisWeek = await db.Meetings.CountAsync(
            m => m.StartsAt >= today && m.StartsAt < today.AddDays(7),
            cancellationToken);

        var tasksByState = await db.WorkItems
            .GroupBy(w => w.State)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new DashboardSummary(
            activeProjects,
            openTasks,
            overdueTasks,
            headcount,
            meetingsThisWeek,
            tasksByState.ToDictionary(x => x.State, x => x.Count));
    }
}

public sealed record DashboardSummary(
    int ActiveProjects,
    int OpenTasks,
    int OverdueTasks,
    int Headcount,
    int MeetingsThisWeek,
    IReadOnlyDictionary<string, int> TasksByState);
