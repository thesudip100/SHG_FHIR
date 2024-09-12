using DataAccess_Layer.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Infrastructure_Layer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SHG_FHIRAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PractitionerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PractitionerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("practitioners")]
        public async Task<IActionResult> GetAllFhirPractitioners()
        {
            var allPractitionerData = await _context.Users.ToListAsync();

            if (allPractitionerData == null || allPractitionerData.Count == 0)
            {
                return NotFound();
            }

            var fhirPractitioners = new List<Practitioner>();
            foreach (var practitionerData in allPractitionerData)
            {
                var fhirPatient = FHIRConverter.ConvertToFhirPractitioner(practitionerData);
                fhirPractitioners.Add(fhirPatient);
            }

            var fhirJsonSerializer = new FhirJsonSerializer();
            var serializedPractitioners = new List<string>();

            foreach (var practitioner in fhirPractitioners)
            {
                var serializedPractitioner = fhirJsonSerializer.SerializeToString(practitioner);
                serializedPractitioners.Add(serializedPractitioner);
            }

            string jsonArray = "[" + string.Join(",", serializedPractitioners) + "]";

            return Content(jsonArray, "application/fhir+json");
        }
    }
}
