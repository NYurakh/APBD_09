namespace MedApp.DTOs;

public class PrescriptionDTO
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public int DoctorId { get; set; }
    public DoctorDTO Doctor { get; set; }
    public List<PrescriptionMedicamentDTO> PrescriptionMedicaments { get; set; }
    public List<MedicamentDTO> Medicaments { get; set; }
}