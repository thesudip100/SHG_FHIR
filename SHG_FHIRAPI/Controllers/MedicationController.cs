using DataAccess_Layer.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Infrastructure_Layer;

namespace SHG_FHIRAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicationController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("medication")]
        public async Task<IActionResult> GetAllMedication()
        {
            var fhirConverterObj = new FHIRConverter(_context);
            var allMedications = await _context.Medications.ToListAsync();
            // var allencountersData = await _context.Encounters.ToListAsync();
            if (allMedications == null || allMedications.Count == 0)
            {
                return NotFound();
            }

            var fhirMedications = new List<Medication>();
            foreach (var medication in allMedications)
            {
                var fhirMedication = fhirConverterObj.ConvertToFhirMedication(medication);
                fhirMedications.Add(fhirMedication);
            }

            var fhirJsonSerializer = new FhirJsonSerializer();
            var serializedMedications = new List<string>();

            foreach (var med in fhirMedications)
            {
                var serializedMed = fhirJsonSerializer.SerializeToString(med);
                serializedMedications.Add(serializedMed);
            }
            string jsonArray = "[" + string.Join(",", serializedMedications) + "]";

            return Content(jsonArray, "application/fhir+json");

        }
    }
}
