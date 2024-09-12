using DataAccess_Layer.Models;
using Hl7.Fhir.Model;


namespace Infrastructure_Layer
{
    public class FHIRConverter
    {
        public static Patient ConvertToFhirPatient(PatientData data)
        {
            var nameParts = data.Name?.Split(' ', 2);
            var patient = new Patient
            {
                Id = data.PatientId,
                Name = new List<HumanName>
            {
                new HumanName
                {
                    Use = HumanName.NameUse.Official,
                    Family = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                    Given = new List<string> { nameParts.Length > 0 ? nameParts[0] : string.Empty }

                }
            },

                Gender = GetGender(data.Gender.ToLower()),
                BirthDate = data.Dob?.ToString("yyyy-MM-dd"),
                Telecom = new List<ContactPoint>
            {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = data.PhoneNumber,
                    Use=ContactPoint.ContactPointUse.Mobile
                }
            },
                Address = new List<Address>
                {
                new Address
                {
                    LineElement= new List<FhirString>
                    {
                        new FhirString { ObjectValue = data.Palika + ", " + data.Ward }
                    },
                    District = data.Palika,
                    State = data.Province,
                }
            }
            };


            return patient;
        }

        public static AdministrativeGender GetGender(string gender)
        {

            switch (gender.ToLower())
            {
                case "male":
                    return AdministrativeGender.Male; // Male
                case "female":
                    return AdministrativeGender.Female; // Female
                case "other":
                    return AdministrativeGender.Other; // Other
                default:
                    return AdministrativeGender.Unknown; // UNKNOWN
            }
        }
    }
}
