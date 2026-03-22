using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Data;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Pages;

public class IndexModel : PageModel
{
    private readonly EhrDbContext _db;
    public IndexModel(EhrDbContext db) => _db = db;

    public int PatientCount { get; set; }
    public int UpcomingAppointments { get; set; }
    public int ActiveEncounters { get; set; }
    public int ActiveMedications { get; set; }
    public int PendingLabs { get; set; }
    public List<Appointment> TodayAppointments { get; set; } = new();
    public List<Patient> RecentPatients { get; set; } = new();
    public List<AlertItem> RecentAlerts { get; set; } = new();

    public async Task OnGetAsync()
    {
        PatientCount = await _db.Patients.CountAsync();
        UpcomingAppointments = await _db.Appointments.CountAsync(a => a.ScheduledDate >= DateTime.UtcNow && a.Status != "Completed" && a.Status != "Cancelled");
        ActiveEncounters = await _db.Encounters.CountAsync(e => e.Status == "Active");
        ActiveMedications = await _db.Medications.CountAsync(m => m.Status == "Active");
        PendingLabs = await _db.LabResults.CountAsync(l => l.Status == "Pending");

        var today = DateTime.UtcNow.Date;
        TodayAppointments = await _db.Appointments
            .Include(a => a.Patient)
            .Where(a => a.ScheduledDate.Date == today)
            .OrderBy(a => a.ScheduledDate)
            .ToListAsync();

        RecentPatients = await _db.Patients
            .Include(p => p.Encounters)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Build recent activity
        var recentEncounters = await _db.Encounters.Include(e => e.Patient).OrderByDescending(e => e.CreatedAt).Take(3).ToListAsync();
        var recentLabs = await _db.LabResults.Include(l => l.Patient).OrderByDescending(l => l.CreatedAt).Take(3).ToListAsync();

        foreach (var e in recentEncounters)
            RecentAlerts.Add(new AlertItem { Message = $"{e.EncounterType} encounter for {e.Patient.FullName} — {e.Status}", DotColor = e.Status == "Active" ? "yellow" : "green", TimeAgo = FormatTimeAgo(e.CreatedAt) });
        foreach (var l in recentLabs)
            RecentAlerts.Add(new AlertItem { Message = $"Lab: {l.TestName} for {l.Patient.FullName} — {l.Status}", DotColor = l.Status == "Abnormal" ? "red" : l.Status == "Pending" ? "yellow" : "green", TimeAgo = FormatTimeAgo(l.CreatedAt) });

        RecentAlerts = RecentAlerts.OrderByDescending(a => a.TimeAgo).Take(6).ToList();
    }

    private static string FormatTimeAgo(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 30) return $"{(int)diff.TotalDays}d ago";
        return dt.ToString("MMM dd");
    }

    public class AlertItem
    {
        public string Message { get; set; } = "";
        public string DotColor { get; set; } = "green";
        public string TimeAgo { get; set; } = "";
    }
}
