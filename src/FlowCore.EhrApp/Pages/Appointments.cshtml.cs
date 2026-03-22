using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class AppointmentsModel : PageModel
{
    private readonly EhrDbContext _db;
    public AppointmentsModel(EhrDbContext db) => _db = db;

    public List<Appointment> Appointments { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }

    public async Task OnGetAsync(string? status)
    {
        StatusFilter = status;
        var q = _db.Appointments.Include(a => a.Patient).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        Appointments = await q.OrderByDescending(a => a.ScheduledDate).ToListAsync();
    }
}
