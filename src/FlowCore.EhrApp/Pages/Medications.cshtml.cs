using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class MedicationsModel : PageModel
{
    private readonly EhrDbContext _db;
    public MedicationsModel(EhrDbContext db) => _db = db;

    public List<Medication> Medications { get; set; } = new();
    public int ActiveCount { get; set; }
    public int DiscontinuedCount { get; set; }
    public int UniquePatients { get; set; }

    public async Task OnGetAsync()
    {
        Medications = await _db.Medications.Include(m => m.Patient).OrderBy(m => m.Patient.LastName).ThenBy(m => m.Name).ToListAsync();
        ActiveCount = Medications.Count(m => m.Status == "Active");
        DiscontinuedCount = Medications.Count(m => m.Status == "Discontinued");
        UniquePatients = Medications.Select(m => m.PatientId).Distinct().Count();
    }
}
