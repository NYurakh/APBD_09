using MedApp.Context;
using MedApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPrescription(AddPrescriptionRequest request)
        {
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            if (request.Medicaments.Count > 10)
            {
                return BadRequest("A prescription can include a maximum of 10 medications.");
            }

            var medicaments = await _context.Medicaments
                .Where(m => request.Medicaments.Select(rm => rm.MedicamentId).Contains(m.IdMedicament))
                .ToListAsync();

            if (medicaments.Count != request.Medicaments.Count)
            {
                return BadRequest("One or more medications listedon the prescription do not exist.");
            }

            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var prescription = new Prescription
            {
                Date = DateTime.Now,
                DueDate = request.DueDate,
                Patient = patient,
                Doctor = doctor,
                PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
                {
                    IdMedicament = m.MedicamentId,
                    Dose = m.Dose,
                    Medicament = medicaments.FirstOrDefault(md => md.IdMedicament == m.MedicamentId)
                }).ToList()
            };

            if (prescription.DueDate <= prescription.Date)
            {
                return BadRequest("DueDate must be greater than or equal to Date.");
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            var prescriptionDTO = new PrescriptionDTO
            {
                IdPrescription = prescription.IdPrescription,
                Date = prescription.Date,
                DueDate = prescription.DueDate,
                DoctorId = prescription.Doctor.IdDoctor,
                PrescriptionMedicaments = prescription.PrescriptionMedicaments.Select(pm => new PrescriptionMedicamentDTO
                {
                    IdPrescription = pm.IdPrescription,
                    IdMedicament = pm.IdMedicament,
                    Dose = pm.Dose,
                    Medicament = new MedicamentDTO
                    {
                        IdMedicament = pm.Medicament.IdMedicament,
                        Name = pm.Medicament.Name,
                        Description = pm.Medicament.Description
                    }
                }).ToList()
            };

            return Ok(prescriptionDTO);
        }
    }
}