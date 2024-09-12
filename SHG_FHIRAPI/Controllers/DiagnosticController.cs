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
    public class DiagnosticController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public DiagnosticController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("diagnostic-report")]
        public async Task<IActionResult> GetAllDiagnosticReport()
        {
            var fhirConverterObj = new FHIRConverter(_context);      //since the method is not static, we need to create object of the class and call the method.Can be sone similarly if made static.
            var alldiagnosticReport = await _context.ConfirmDiagnoses.ToListAsync();
            if (alldiagnosticReport == null || alldiagnosticReport.Count == 0)
            {
                return NotFound();
            }

            var fhirDiagnoses = new List<DiagnosticReport>();
            foreach (var diagnosticReport in alldiagnosticReport)
            {
                var fhirDiagnosis = fhirConverterObj.ConvertToFhirDiagnosticReport(diagnosticReport);
                fhirDiagnoses.Add(fhirDiagnosis);
            }

            var fhirJsonSerializer = new FhirJsonSerializer();
            var serializedDiagnosticReports = new List<string>();

            foreach (var report in fhirDiagnoses)
            {
                var serializedReport = fhirJsonSerializer.SerializeToString(report);
                serializedDiagnosticReports.Add(serializedReport);
            }
            string jsonArray = "[" + string.Join(",", serializedDiagnosticReports) + "]";

            return Content(jsonArray, "application/fhir+json");
        }


    }
}
