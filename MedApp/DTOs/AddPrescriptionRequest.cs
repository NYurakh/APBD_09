namespace MedApp.DTOs;

public class AddPrescriptionRequest
{
    public int PatientId { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public DateTime PatientBirthdate { get; set; }
    public int DoctorId { get; set; }
    public DateTime DueDate { get; set; }
    public List<MedicamentRequest> Medicaments { get; set; }
}