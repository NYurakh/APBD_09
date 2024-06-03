namespace MedApp.DTOs;

public class PrescriptionMedicamentDTO
{
    public int IdPrescription { get; set; }
    public int IdMedicament { get; set; }
    public int Dose { get; set; }
    public MedicamentDTO Medicament { get; set; }
}