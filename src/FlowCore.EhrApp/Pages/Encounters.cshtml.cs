using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class EncountersModel : PageModel
{
    private readonly EhrDbContext _db;
    public EncountersModel(EhrDbContext db) => _db = db;

    public List<Encounter> Encounters { get; set; } = new();

    public async Task OnGetAsync()
    {
        Encounters = await _db.Encounters.Include(e => e.Patient).OrderByDescending(e => e.EncounterDate).ToListAsync();
    }
}
