using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class PatientsModel : PageModel
{
    private readonly EhrDbContext _db;
    public PatientsModel(EhrDbContext db) => _db = db;

    public List<Patient> Patients { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var q = _db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            q = q.Where(p => p.FirstName.ToLower().Contains(s) || p.LastName.ToLower().Contains(s) || (p.MRN != null && p.MRN.ToLower().Contains(s)));
        }
        Patients = await q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync();
    }
}
