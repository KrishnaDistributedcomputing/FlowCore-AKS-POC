using Microsoft.EntityFrameworkCore;
using FlowCore.EhrApp.Models;

namespace FlowCore.EhrApp.Data;

public class EhrDbContext : DbContext
{
    public EhrDbContext(DbContextOptions<EhrDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasIndex(p => p.MRN).IsUnique();
            e.HasIndex(p => new { p.LastName, p.FirstName });
        });

        modelBuilder.Entity<Encounter>(e =>
        {
            e.HasOne(en => en.Patient).WithMany(p => p.Encounters).HasForeignKey(en => en.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(en => en.PatientId);
            e.HasIndex(en => en.EncounterDate);
        });

        modelBuilder.Entity<Medication>(e =>
        {
            e.HasOne(m => m.Patient).WithMany(p => p.Medications).HasForeignKey(m => m.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => m.PatientId);
        });

        modelBuilder.Entity<LabResult>(e =>
        {
            e.HasOne(l => l.Patient).WithMany(p => p.LabResults).HasForeignKey(l => l.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(l => l.PatientId);
            e.HasIndex(l => l.OrderDate);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => a.PatientId);
            e.HasIndex(a => a.ScheduledDate);
        });

        modelBuilder.Entity<VitalSign>(e =>
        {
            e.HasOne(v => v.Patient).WithMany(p => p.VitalSigns).HasForeignKey(v => v.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => v.PatientId);
            e.HasIndex(v => v.RecordedAt);
        });

        // Seed demo data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var patients = new[]
        {
            new Patient { Id = 1, FirstName = "Sarah", LastName = "Johnson", DateOfBirth = new DateTime(1985, 3, 15), Gender = "Female", MRN = "MRN-100001", Email = "sarah.johnson@email.com", Phone = "416-555-0101", Address = "123 Queen St W", City = "Toronto", PostalCode = "M5H 2N2", InsuranceProvider = "Sun Life", InsurancePolicyNumber = "SL-882991", BloodType = "O+", Allergies = "Penicillin, Sulfa drugs", CreatedAt = new DateTime(2024, 1, 10), UpdatedAt = new DateTime(2024, 1, 10) },
            new Patient { Id = 2, FirstName = "Michael", LastName = "Chen", DateOfBirth = new DateTime(1972, 8, 22), Gender = "Male", MRN = "MRN-100002", Email = "michael.chen@email.com", Phone = "604-555-0202", Address = "456 Robson St", City = "Vancouver", PostalCode = "V6B 2A8", InsuranceProvider = "Manulife", InsurancePolicyNumber = "ML-334455", BloodType = "A+", Allergies = "None", CreatedAt = new DateTime(2024, 1, 12), UpdatedAt = new DateTime(2024, 1, 12) },
            new Patient { Id = 3, FirstName = "Emily", LastName = "Williams", DateOfBirth = new DateTime(1990, 11, 5), Gender = "Female", MRN = "MRN-100003", Email = "emily.w@email.com", Phone = "514-555-0303", Address = "789 Rue Sainte-Catherine", City = "Montreal", PostalCode = "H3B 1A1", InsuranceProvider = "Blue Cross", InsurancePolicyNumber = "BC-667788", BloodType = "B-", Allergies = "Latex", CreatedAt = new DateTime(2024, 2, 1), UpdatedAt = new DateTime(2024, 2, 1) },
            new Patient { Id = 4, FirstName = "James", LastName = "Patel", DateOfBirth = new DateTime(1968, 5, 30), Gender = "Male", MRN = "MRN-100004", Email = "james.patel@email.com", Phone = "403-555-0404", Address = "321 Stephen Ave", City = "Calgary", PostalCode = "T2P 1E8", InsuranceProvider = "Great-West Life", InsurancePolicyNumber = "GW-112233", BloodType = "AB+", Allergies = "Aspirin, Iodine contrast", CreatedAt = new DateTime(2024, 2, 15), UpdatedAt = new DateTime(2024, 2, 15) },
            new Patient { Id = 5, FirstName = "Maria", LastName = "Rodriguez", DateOfBirth = new DateTime(1995, 7, 18), Gender = "Female", MRN = "MRN-100005", Email = "maria.r@email.com", Phone = "613-555-0505", Address = "55 Rideau St", City = "Ottawa", PostalCode = "K1N 9J7", InsuranceProvider = "Sun Life", InsurancePolicyNumber = "SL-998877", BloodType = "O-", Allergies = "None", CreatedAt = new DateTime(2024, 3, 1), UpdatedAt = new DateTime(2024, 3, 1) },
            new Patient { Id = 6, FirstName = "Robert", LastName = "Thompson", DateOfBirth = new DateTime(1958, 12, 3), Gender = "Male", MRN = "MRN-100006", Email = "rthompson@email.com", Phone = "780-555-0606", Address = "100 Jasper Ave", City = "Edmonton", PostalCode = "T5J 1S9", InsuranceProvider = "Manulife", InsurancePolicyNumber = "ML-556677", BloodType = "A-", Allergies = "Codeine", CreatedAt = new DateTime(2024, 3, 10), UpdatedAt = new DateTime(2024, 3, 10) },
            new Patient { Id = 7, FirstName = "Lisa", LastName = "Kim", DateOfBirth = new DateTime(2001, 4, 25), Gender = "Female", MRN = "MRN-100007", Email = "lisa.kim@email.com", Phone = "416-555-0707", Address = "200 Bay St", City = "Toronto", PostalCode = "M5J 2J5", InsuranceProvider = "Blue Cross", InsurancePolicyNumber = "BC-445566", BloodType = "B+", Allergies = "Shellfish (food allergy noted)", CreatedAt = new DateTime(2024, 3, 20), UpdatedAt = new DateTime(2024, 3, 20) },
            new Patient { Id = 8, FirstName = "David", LastName = "Nguyen", DateOfBirth = new DateTime(1980, 9, 12), Gender = "Male", MRN = "MRN-100008", Email = "david.n@email.com", Phone = "204-555-0808", Address = "300 Portage Ave", City = "Winnipeg", PostalCode = "R3C 0C4", InsuranceProvider = "Great-West Life", InsurancePolicyNumber = "GW-889900", BloodType = "O+", Allergies = "None", CreatedAt = new DateTime(2024, 4, 1), UpdatedAt = new DateTime(2024, 4, 1) },
        };
        modelBuilder.Entity<Patient>().HasData(patients);

        modelBuilder.Entity<Encounter>().HasData(
            new Encounter { Id = 1, PatientId = 1, EncounterType = "Office Visit", Status = "Completed", Provider = "Dr. Amanda Foster", Department = "Internal Medicine", ChiefComplaint = "Annual physical exam", Diagnosis = "Healthy, routine checkup", TreatmentPlan = "Continue current medications, follow up in 12 months", EncounterDate = new DateTime(2024, 6, 15), DischargeDate = new DateTime(2024, 6, 15), CreatedAt = new DateTime(2024, 6, 15) },
            new Encounter { Id = 2, PatientId = 1, EncounterType = "Telehealth", Status = "Completed", Provider = "Dr. Brian Lee", Department = "Dermatology", ChiefComplaint = "Skin rash on left arm", Diagnosis = "Contact dermatitis", TreatmentPlan = "Prescribed hydrocortisone cream, avoid irritant", EncounterDate = new DateTime(2024, 8, 20), DischargeDate = new DateTime(2024, 8, 20), CreatedAt = new DateTime(2024, 8, 20) },
            new Encounter { Id = 3, PatientId = 2, EncounterType = "Emergency", Status = "Completed", Provider = "Dr. Carol White", Department = "Emergency Medicine", ChiefComplaint = "Chest pain, shortness of breath", Diagnosis = "Acute anxiety episode, cardiac workup negative", TreatmentPlan = "Discharged with anxiolytic prescription, cardiology follow-up", EncounterDate = new DateTime(2024, 7, 3), DischargeDate = new DateTime(2024, 7, 3), CreatedAt = new DateTime(2024, 7, 3) },
            new Encounter { Id = 4, PatientId = 4, EncounterType = "Office Visit", Status = "Active", Provider = "Dr. David Park", Department = "Cardiology", ChiefComplaint = "Follow-up for hypertension management", Diagnosis = "Essential hypertension, improving", TreatmentPlan = "Adjust Lisinopril dosage to 20mg, recheck in 3 months", EncounterDate = new DateTime(2024, 9, 10), CreatedAt = new DateTime(2024, 9, 10) },
            new Encounter { Id = 5, PatientId = 6, EncounterType = "Inpatient", Status = "Completed", Provider = "Dr. Elena Vasquez", Department = "Orthopedics", ChiefComplaint = "Right hip replacement surgery", Diagnosis = "Severe osteoarthritis, right hip", TreatmentPlan = "Post-op rehab protocol, PT 3x/week for 8 weeks", EncounterDate = new DateTime(2024, 5, 1), DischargeDate = new DateTime(2024, 5, 5), CreatedAt = new DateTime(2024, 5, 1) },
            new Encounter { Id = 6, PatientId = 3, EncounterType = "Office Visit", Status = "Completed", Provider = "Dr. Frank Garcia", Department = "OB/GYN", ChiefComplaint = "Prenatal checkup - 20 weeks", Diagnosis = "Normal pregnancy, 20 weeks gestation", TreatmentPlan = "Continue prenatal vitamins, anatomy scan ordered", EncounterDate = new DateTime(2024, 8, 5), DischargeDate = new DateTime(2024, 8, 5), CreatedAt = new DateTime(2024, 8, 5) }
        );

        modelBuilder.Entity<Medication>().HasData(
            new Medication { Id = 1, PatientId = 1, Name = "Lisinopril", Dosage = "10mg", Frequency = "Once daily", Route = "Oral", PrescribedBy = "Dr. Amanda Foster", Status = "Active", StartDate = new DateTime(2024, 1, 15), Notes = "For blood pressure management", CreatedAt = new DateTime(2024, 1, 15) },
            new Medication { Id = 2, PatientId = 1, Name = "Metformin", Dosage = "500mg", Frequency = "Twice daily", Route = "Oral", PrescribedBy = "Dr. Amanda Foster", Status = "Active", StartDate = new DateTime(2024, 1, 15), Notes = "For Type 2 diabetes", CreatedAt = new DateTime(2024, 1, 15) },
            new Medication { Id = 3, PatientId = 2, Name = "Atorvastatin", Dosage = "20mg", Frequency = "Once daily at bedtime", Route = "Oral", PrescribedBy = "Dr. Carol White", Status = "Active", StartDate = new DateTime(2024, 7, 5), Notes = "Cholesterol management", CreatedAt = new DateTime(2024, 7, 5) },
            new Medication { Id = 4, PatientId = 4, Name = "Lisinopril", Dosage = "20mg", Frequency = "Once daily", Route = "Oral", PrescribedBy = "Dr. David Park", Status = "Active", StartDate = new DateTime(2024, 2, 20), Notes = "Hypertension, dose adjusted Sep 2024", CreatedAt = new DateTime(2024, 2, 20) },
            new Medication { Id = 5, PatientId = 4, Name = "Aspirin", Dosage = "81mg", Frequency = "Once daily", Route = "Oral", PrescribedBy = "Dr. David Park", Status = "Discontinued", StartDate = new DateTime(2024, 2, 20), EndDate = new DateTime(2024, 6, 1), Notes = "Discontinued due to allergy concern", CreatedAt = new DateTime(2024, 2, 20) },
            new Medication { Id = 6, PatientId = 6, Name = "Acetaminophen", Dosage = "500mg", Frequency = "Every 6 hours as needed", Route = "Oral", PrescribedBy = "Dr. Elena Vasquez", Status = "Active", StartDate = new DateTime(2024, 5, 5), Notes = "Post-surgical pain management", CreatedAt = new DateTime(2024, 5, 5) },
            new Medication { Id = 7, PatientId = 3, Name = "Prenatal Multivitamin", Dosage = "1 tablet", Frequency = "Once daily", Route = "Oral", PrescribedBy = "Dr. Frank Garcia", Status = "Active", StartDate = new DateTime(2024, 4, 1), Notes = "Prenatal care", CreatedAt = new DateTime(2024, 4, 1) },
            new Medication { Id = 8, PatientId = 5, Name = "Cetirizine", Dosage = "10mg", Frequency = "Once daily", Route = "Oral", PrescribedBy = "Dr. Amanda Foster", Status = "Active", StartDate = new DateTime(2024, 5, 10), Notes = "Seasonal allergies", CreatedAt = new DateTime(2024, 5, 10) }
        );

        modelBuilder.Entity<LabResult>().HasData(
            new LabResult { Id = 1, PatientId = 1, TestName = "Complete Blood Count (CBC)", Category = "Hematology", Result = "Normal", Unit = "", ReferenceRange = "See details", Status = "Completed", OrderedBy = "Dr. Amanda Foster", OrderDate = new DateTime(2024, 6, 15), ResultDate = new DateTime(2024, 6, 16), CreatedAt = new DateTime(2024, 6, 15) },
            new LabResult { Id = 2, PatientId = 1, TestName = "HbA1c", Category = "Chemistry", Result = "6.8", Unit = "%", ReferenceRange = "< 7.0", Status = "Completed", OrderedBy = "Dr. Amanda Foster", OrderDate = new DateTime(2024, 6, 15), ResultDate = new DateTime(2024, 6, 17), Notes = "Good glycemic control", CreatedAt = new DateTime(2024, 6, 15) },
            new LabResult { Id = 3, PatientId = 2, TestName = "Troponin I", Category = "Chemistry", Result = "< 0.01", Unit = "ng/mL", ReferenceRange = "< 0.04", Status = "Completed", OrderedBy = "Dr. Carol White", OrderDate = new DateTime(2024, 7, 3), ResultDate = new DateTime(2024, 7, 3), Notes = "Cardiac markers negative", CreatedAt = new DateTime(2024, 7, 3) },
            new LabResult { Id = 4, PatientId = 2, TestName = "Lipid Panel", Category = "Chemistry", Result = "LDL 145", Unit = "mg/dL", ReferenceRange = "< 100", Status = "Abnormal", OrderedBy = "Dr. Carol White", OrderDate = new DateTime(2024, 7, 3), ResultDate = new DateTime(2024, 7, 4), Notes = "Elevated LDL, started statin", CreatedAt = new DateTime(2024, 7, 3) },
            new LabResult { Id = 5, PatientId = 4, TestName = "Basic Metabolic Panel", Category = "Chemistry", Result = "Normal", Unit = "", ReferenceRange = "See details", Status = "Completed", OrderedBy = "Dr. David Park", OrderDate = new DateTime(2024, 9, 10), ResultDate = new DateTime(2024, 9, 11), CreatedAt = new DateTime(2024, 9, 10) },
            new LabResult { Id = 6, PatientId = 3, TestName = "Prenatal Panel", Category = "Hematology", Result = "Normal", Unit = "", ReferenceRange = "See details", Status = "Completed", OrderedBy = "Dr. Frank Garcia", OrderDate = new DateTime(2024, 4, 5), ResultDate = new DateTime(2024, 4, 7), CreatedAt = new DateTime(2024, 4, 5) },
            new LabResult { Id = 7, PatientId = 5, TestName = "Thyroid Panel (TSH, T4)", Category = "Chemistry", Result = "TSH 2.1", Unit = "mIU/L", ReferenceRange = "0.5 – 4.5", Status = "Completed", OrderedBy = "Dr. Amanda Foster", OrderDate = new DateTime(2024, 5, 10), ResultDate = new DateTime(2024, 5, 12), CreatedAt = new DateTime(2024, 5, 10) },
            new LabResult { Id = 8, PatientId = 7, TestName = "Complete Blood Count (CBC)", Category = "Hematology", Result = "Pending", Status = "Pending", OrderedBy = "Dr. Brian Lee", OrderDate = new DateTime(2024, 10, 1), CreatedAt = new DateTime(2024, 10, 1) }
        );

        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PatientId = 1, Provider = "Dr. Amanda Foster", Department = "Internal Medicine", AppointmentType = "Follow-up", Status = "Scheduled", ScheduledDate = new DateTime(2025, 6, 15, 10, 0, 0), Reason = "Annual physical, diabetes and BP follow-up", CreatedAt = new DateTime(2024, 6, 15) },
            new Appointment { Id = 2, PatientId = 2, Provider = "Dr. Carol White", Department = "Cardiology", AppointmentType = "Follow-up", Status = "Scheduled", ScheduledDate = new DateTime(2025, 1, 15, 14, 0, 0), Reason = "Lipid panel review, statin efficacy check", CreatedAt = new DateTime(2024, 7, 5) },
            new Appointment { Id = 3, PatientId = 3, Provider = "Dr. Frank Garcia", Department = "OB/GYN", AppointmentType = "Prenatal Visit", Status = "Completed", ScheduledDate = new DateTime(2024, 9, 5, 9, 30, 0), Reason = "28-week prenatal checkup", CreatedAt = new DateTime(2024, 8, 5) },
            new Appointment { Id = 4, PatientId = 4, Provider = "Dr. David Park", Department = "Cardiology", AppointmentType = "Follow-up", Status = "Scheduled", ScheduledDate = new DateTime(2025, 3, 10, 11, 0, 0), Reason = "Hypertension management review", CreatedAt = new DateTime(2024, 9, 10) },
            new Appointment { Id = 5, PatientId = 5, Provider = "Dr. Amanda Foster", Department = "Internal Medicine", AppointmentType = "Annual Physical", Status = "Scheduled", ScheduledDate = new DateTime(2025, 5, 20, 8, 30, 0), Reason = "Annual wellness exam", CreatedAt = new DateTime(2024, 5, 10) },
            new Appointment { Id = 6, PatientId = 6, Provider = "Dr. Elena Vasquez", Department = "Orthopedics", AppointmentType = "Post-Op", Status = "Completed", ScheduledDate = new DateTime(2024, 6, 15, 13, 0, 0), Reason = "6-week post-op hip replacement check", CreatedAt = new DateTime(2024, 5, 5) },
            new Appointment { Id = 7, PatientId = 7, Provider = "Dr. Brian Lee", Department = "Family Medicine", AppointmentType = "New Patient", Status = "Scheduled", ScheduledDate = new DateTime(2025, 4, 1, 10, 30, 0), Reason = "Establish care, review history", CreatedAt = new DateTime(2024, 10, 1) },
            new Appointment { Id = 8, PatientId = 8, Provider = "Dr. Amanda Foster", Department = "Internal Medicine", AppointmentType = "Sick Visit", Status = "Checked-In", ScheduledDate = new DateTime(2025, 3, 21, 9, 0, 0), Reason = "Persistent cough, 2 weeks duration", CreatedAt = new DateTime(2025, 3, 20) }
        );

        modelBuilder.Entity<VitalSign>().HasData(
            new VitalSign { Id = 1, PatientId = 1, Temperature = 98.6m, HeartRate = 72, SystolicBP = 128, DiastolicBP = 82, RespiratoryRate = 16, OxygenSaturation = 98.0m, Weight = 68.5m, Height = 165.0m, RecordedBy = "Nurse Practitioner J. Adams", RecordedAt = new DateTime(2024, 6, 15, 9, 30, 0) },
            new VitalSign { Id = 2, PatientId = 2, Temperature = 98.2m, HeartRate = 88, SystolicBP = 142, DiastolicBP = 90, RespiratoryRate = 20, OxygenSaturation = 97.0m, Weight = 82.0m, Height = 175.0m, RecordedBy = "ER Nurse K. Singh", RecordedAt = new DateTime(2024, 7, 3, 22, 15, 0) },
            new VitalSign { Id = 3, PatientId = 4, Temperature = 98.4m, HeartRate = 68, SystolicBP = 135, DiastolicBP = 85, RespiratoryRate = 14, OxygenSaturation = 99.0m, Weight = 90.0m, Height = 180.0m, RecordedBy = "Nurse M. Brown", RecordedAt = new DateTime(2024, 9, 10, 10, 45, 0) },
            new VitalSign { Id = 4, PatientId = 3, Temperature = 98.8m, HeartRate = 80, SystolicBP = 110, DiastolicBP = 70, RespiratoryRate = 16, OxygenSaturation = 99.0m, Weight = 72.0m, Height = 168.0m, RecordedBy = "Nurse L. Tremblay", RecordedAt = new DateTime(2024, 8, 5, 9, 0, 0) },
            new VitalSign { Id = 5, PatientId = 6, Temperature = 99.1m, HeartRate = 78, SystolicBP = 138, DiastolicBP = 88, RespiratoryRate = 18, OxygenSaturation = 96.0m, Weight = 85.0m, Height = 178.0m, RecordedBy = "Nurse P. Davis", RecordedAt = new DateTime(2024, 5, 1, 7, 0, 0) }
        );
    }
}
