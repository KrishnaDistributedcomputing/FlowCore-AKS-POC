using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class PatientEditModel : PageModel
{
    private readonly EhrDbContext _db;
    public PatientEditModel(EhrDbContext db) => _db = db;

    [BindProperty] public Patient Patient { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p == null) return NotFound();
        Patient = p;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _db.Patients.FindAsync(Patient.Id);
        if (existing == null) return NotFound();

        existing.FirstName = Patient.FirstName;
        existing.LastName = Patient.LastName;
        existing.DateOfBirth = Patient.DateOfBirth;
        existing.Gender = Patient.Gender;
        existing.Email = Patient.Email;
        existing.Phone = Patient.Phone;
        existing.Address = Patient.Address;
        existing.City = Patient.City;
        existing.PostalCode = Patient.PostalCode;
        existing.BloodType = Patient.BloodType;
        existing.InsuranceProvider = Patient.InsuranceProvider;
        existing.InsurancePolicyNumber = Patient.InsurancePolicyNumber;
        existing.Allergies = Patient.Allergies;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToPage("/Patients/Detail", new { id = Patient.Id });
    }
}
