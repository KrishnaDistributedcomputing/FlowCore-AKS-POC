using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowCore.EhrApp.Models;

public class Patient
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required, MaxLength(10)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? MRN { get; set; } // Medical Record Number

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? InsuranceProvider { get; set; }

    [MaxLength(50)]
    public string? InsurancePolicyNumber { get; set; }

    [MaxLength(20)]
    public string? BloodType { get; set; }

    public string? Allergies { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<Encounter> Encounters { get; set; } = new();
    public List<Medication> Medications { get; set; } = new();
    public List<LabResult> LabResults { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<VitalSign> VitalSigns { get; set; } = new();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    [NotMapped]
    public int Age => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);
}

public class Encounter
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required, MaxLength(50)]
    public string EncounterType { get; set; } = string.Empty; // Office Visit, Emergency, Telehealth, Inpatient

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

    [MaxLength(100)]
    public string? Provider { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(200)]
    public string? ChiefComplaint { get; set; }

    public string? ClinicalNotes { get; set; }

    public string? Diagnosis { get; set; }

    public string? TreatmentPlan { get; set; }

    public DateTime EncounterDate { get; set; } = DateTime.UtcNow;
    public DateTime? DischargeDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Medication
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Dosage { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Frequency { get; set; } = string.Empty; // Once daily, Twice daily, etc.

    [MaxLength(50)]
    public string? Route { get; set; } // Oral, IV, Topical, etc.

    [MaxLength(100)]
    public string? PrescribedBy { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Discontinued, Completed

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LabResult
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required, MaxLength(100)]
    public string TestName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Hematology, Chemistry, Microbiology, etc.

    [MaxLength(50)]
    public string? Result { get; set; }

    [MaxLength(30)]
    public string? Unit { get; set; }

    [MaxLength(50)]
    public string? ReferenceRange { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Abnormal

    [MaxLength(100)]
    public string? OrderedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResultDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Appointment
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Provider { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string AppointmentType { get; set; } = string.Empty; // Follow-up, New Patient, Annual Physical, etc.

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Checked-In, In Progress, Completed, Cancelled, No Show

    public DateTime ScheduledDate { get; set; }

    [MaxLength(200)]
    public string? Reason { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VitalSign
{
    [Key]
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public decimal? Temperature { get; set; }        // °F
    public int? HeartRate { get; set; }               // bpm
    public int? SystolicBP { get; set; }              // mmHg
    public int? DiastolicBP { get; set; }             // mmHg
    public int? RespiratoryRate { get; set; }         // breaths/min
    public decimal? OxygenSaturation { get; set; }    // %
    public decimal? Weight { get; set; }              // kg
    public decimal? Height { get; set; }              // cm

    [MaxLength(100)]
    public string? RecordedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
