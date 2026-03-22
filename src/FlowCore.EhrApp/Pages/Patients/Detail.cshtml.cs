using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class PatientDetailModel : PageModel
{
    private readonly EhrDbContext _db;
    public PatientDetailModel(EhrDbContext db) => _db = db;

    public Patient? Patient { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Patient = await _db.Patients
            .Include(p => p.Encounters)
            .Include(p => p.Medications)
            .Include(p => p.LabResults)
            .Include(p => p.Appointments)
            .Include(p => p.VitalSigns)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (Patient == null) return NotFound();
        return Page();
    }
}
