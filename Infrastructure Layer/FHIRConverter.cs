using DataAccess_Layer.Models;
using Hl7.Fhir.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace Infrastructure_Layer
{
    public class FHIRConverter
    {
        private readonly ApplicationDbContext dbContext;

        public FHIRConverter(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
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

        public static Practitioner ConvertToFhirPractitioner(User data)
        {
            var practitioner = new Practitioner
            {
                Id = Convert.ToString(data.Id),
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        FamilyElement = new FhirString
                        {
                            Value = data.LastName
                        },
                        UseElement = new Code<HumanName.NameUse>
                        {
                            Value = HumanName.NameUse.Official
                        },
                        GivenElement = new List<FhirString>
                        {
                            new FhirString
                            {
                                Value = data.FirstName
                            }
                        }
                    }
                },
                Active = null,
                Telecom = new List<ContactPoint>
                {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = data.PhoneNumber,
                },
                new ContactPoint
                {
                     System= ContactPoint.ContactPointSystem.Email,
                    Value= data.Email
                }
            }
            };

            return practitioner;
        }



        public DiagnosticReport ConvertToFhirDiagnosticReport(ConfirmDiagnosis diagnosis)
        {
            var encounter = dbContext.Encounters.FirstOrDefault(e => e.Id == diagnosis.EncounterId);
            var patient = dbContext.Patients.FirstOrDefault(p => p.Id == diagnosis.PatientId);

            var diagnosisreport = new DiagnosticReport
            {
                Id = Convert.ToString(diagnosis.Id),
                Encounter = new ResourceReference
                {
                    Reference = $"Encounter/{diagnosis.EncounterId}",
                    Display = encounter?.Reason
                },
                Conclusion = diagnosis.Description,
                Subject = new ResourceReference
                {
                    Reference = $"Patient/{diagnosis.PatientId}",
                    Display = patient?.Name
                }
            };
            return diagnosisreport;
        }

        public Medication ConvertToFhirMedication(MedicationData meds)
        {
            var patient = dbContext.Patients.FirstOrDefault(p => p.Id == meds.PatientId);
            var encounter = dbContext.Encounters.FirstOrDefault(e => e.Id == meds.EncounterId);


            int numericPart = 0;
            string characterPart = string.Empty;

            string dosage = meds.Dosage;


            var numericMatch = Regex.Match(dosage, @"\d+").Value;
            var characterMatch = Regex.Match(dosage, @"[a-zA-Z]+").Value;

            if (!string.IsNullOrEmpty(numericMatch))
            {
                numericPart = int.Parse(numericMatch);
            }

            characterPart = characterMatch;

            /*string dosage = meds.Dosage;
            int index = 0;
            while(index<dosage.Length && char.IsDigit(dosage[index]))
            {
                index++;
            }

            int number = Convert.ToInt(dosage.Substring(0, index));
            string character = dosage.Substring(index);*/
            var medication = new Medication
            {
                Id = Convert.ToString(meds.Id),
                Form = new CodeableConcept
                {
                    Coding = new List<Coding>
            {
                new Coding{
                    Display = meds.Type
                }
            }
                },
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
            {
                new Coding
                {
                    Display = meds.MedicineName
                }
            }
                },
                Ingredient = new List<Medication.IngredientComponent>
        {
            new Medication.IngredientComponent
            {
                Strength = new Ratio
                {
                    Numerator = new Quantity
                    {
                        Value = numericPart,
                        Code = characterPart
                    }
                }
            }
           }
            };

            return medication;
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
