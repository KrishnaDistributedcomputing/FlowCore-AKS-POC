using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class LabResultsModel : PageModel
{
    private readonly EhrDbContext _db;
    public LabResultsModel(EhrDbContext db) => _db = db;

    public List<LabResult> Labs { get; set; } = new();
    public int TotalLabs { get; set; }
    public int PendingCount { get; set; }
    public int AbnormalCount { get; set; }
    public int CompletedCount { get; set; }

    public async Task OnGetAsync()
    {
        Labs = await _db.LabResults.Include(l => l.Patient).OrderByDescending(l => l.OrderDate).ToListAsync();
        TotalLabs = Labs.Count;
        PendingCount = Labs.Count(l => l.Status == "Pending");
        AbnormalCount = Labs.Count(l => l.Status == "Abnormal");
        CompletedCount = Labs.Count(l => l.Status == "Completed");
    }
}
