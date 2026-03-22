using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class PatientCreateModel : PageModel
{
    private readonly EhrDbContext _db;
    public PatientCreateModel(EhrDbContext db) => _db = db;

    [BindProperty] public Patient Patient { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Auto-generate MRN
        var maxId = _db.Patients.Any() ? _db.Patients.Max(p => p.Id) : 100000;
        Patient.MRN = $"MRN-{maxId + 1:D6}";
        Patient.CreatedAt = DateTime.UtcNow;
        Patient.UpdatedAt = DateTime.UtcNow;

        _db.Patients.Add(Patient);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Patients/Detail", new { id = Patient.Id });
    }
}
