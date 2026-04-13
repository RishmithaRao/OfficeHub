using System.Text;
using Microsoft.EntityFrameworkCore;
using OfficeHub.Data;

namespace OfficeHub.Services;

public sealed class ReportExportService(AppDbContext db)
{
    public async Task<byte[]> BuildTasksCsvAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.WorkItems
            .AsNoTracking()
            .Include(w => w.Project)
            .Include(w => w.Assignee)
            .OrderBy(w => w.Project!.Code)
            .ThenBy(w => w.DueDate)
            .Select(w => new
            {
                w.Id,
                Project = w.Project!.Code,
                w.Title,
                w.State,
                w.Priority,
                w.DueDate,
                Assignee = w.Assignee != null ? w.Assignee.Email : ""
            })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Project,Title,State,Priority,DueDate,AssigneeEmail");
        foreach (var r in rows)
            sb.AppendLine(
                $"{r.Id},\"{r.Project}\",\"{r.Title.Replace("\"", "\"\"")}\",{r.State},{r.Priority},{r.DueDate:yyyy-MM-dd},{r.Assignee}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
