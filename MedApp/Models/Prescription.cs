using System.Text.Json.Serialization;

public class Prescription
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int IdPatient { get; set; }
    [JsonIgnore]
    public Patient Patient { get; set; }
    public int IdDoctor { get; set; }
    public Doctor Doctor { get; set; }
    public ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; }
}