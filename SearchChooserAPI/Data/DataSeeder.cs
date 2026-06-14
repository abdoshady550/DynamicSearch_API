using SearchChooserAPI.Models;

namespace SearchChooserAPI.Data
{
    public static class DataSeeder
    {
        public static void SeedData(SearchDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Doctors.Any()) return;

            // Specialties
            var cardioId = Guid.NewGuid();
            var dermId = Guid.NewGuid();
            var neuroId = Guid.NewGuid();
            var orthoId = Guid.NewGuid();
            var pedsId = Guid.NewGuid();
            var radioId = Guid.NewGuid();

            var cardio = new Specialty
            {
                Id = cardioId,
                SnomedCode = "394579002",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = cardioId, Name = "Cardiology", Language = "en" }
                }
            };
            var derm = new Specialty
            {
                Id = dermId,
                SnomedCode = "394582007",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = dermId, Name = "Dermatology", Language = "en" }
                }
            };
            var neuro = new Specialty
            {
                Id = neuroId,
                SnomedCode = "394587006",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = neuroId, Name = "Neurology", Language = "en" }
                }
            };
            var ortho = new Specialty
            {
                Id = orthoId,
                SnomedCode = "394588001",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = orthoId, Name = "Orthopedics", Language = "en" }
                }
            };
            var peds = new Specialty
            {
                Id = pedsId,
                SnomedCode = "394589009",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = pedsId, Name = "Pediatrics", Language = "en" }
                }
            };
            var radio = new Specialty
            {
                Id = radioId,
                SnomedCode = "394590005",
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new() { Id = Guid.NewGuid(), SpecialtyId = radioId, Name = "Radiology", Language = "en" }
                }
            };

            context.Specialties.AddRange(cardio, derm, neuro, ortho, peds, radio);

            // Degrees
            var mdId = Guid.NewGuid();
            var phdId = Guid.NewGuid();
            var doId = Guid.NewGuid();
            var mbbsId = Guid.NewGuid();

            var md = new Degree
            {
                Id = mdId,
                DegreeTranslations = new List<DegreeTranslation>
                {
                    new() { Id = Guid.NewGuid(), DegreeId = mdId, Name = "MD (Doctor of Medicine)", Language = "en" }
                }
            };
            var phd = new Degree
            {
                Id = phdId,
                DegreeTranslations = new List<DegreeTranslation>
                {
                    new() { Id = Guid.NewGuid(), DegreeId = phdId, Name = "PhD in Immunology", Language = "en" }
                }
            };
            var doDeg = new Degree
            {
                Id = doId,
                DegreeTranslations = new List<DegreeTranslation>
                {
                    new() { Id = Guid.NewGuid(), DegreeId = doId, Name = "DO (Doctor of Osteopathy)", Language = "en" }
                }
            };
            var mbbs = new Degree
            {
                Id = mbbsId,
                DegreeTranslations = new List<DegreeTranslation>
                {
                    new() { Id = Guid.NewGuid(), DegreeId = mbbsId, Name = "MBBS (Bachelor of Medicine)", Language = "en" }
                }
            };

            context.Degrees.AddRange(md, phd, doDeg, mbbs);

            // Doctors
            var doctors = new List<Doctor>
            {
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = cardioId, DegreeId = mdId,
                    YearsOfExperience = 15, Rating = 4.8m,
                    JoinDate = new DateTime(2010, 5, 20), LastActive = new DateTime(2026, 6, 10, 14, 30, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Sarah Jenkins", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = dermId, DegreeId = phdId,
                    YearsOfExperience = 5, Rating = 3.2m,
                    JoinDate = new DateTime(2021, 8, 1), LastActive = new DateTime(2026, 5, 22, 9, 15, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. John Doe", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = neuroId, DegreeId = mdId,
                    YearsOfExperience = 22, Rating = 4.9m,
                    JoinDate = new DateTime(2004, 3, 10), LastActive = new DateTime(2026, 6, 11, 8, 0, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Emily Chen", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = orthoId, DegreeId = doId,
                    YearsOfExperience = 10, Rating = 4.5m,
                    JoinDate = new DateTime(2016, 1, 15), LastActive = new DateTime(2026, 5, 30, 16, 45, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Michael Torres", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = pedsId, DegreeId = mbbsId,
                    YearsOfExperience = 8, Rating = 4.7m,
                    JoinDate = new DateTime(2018, 9, 5), LastActive = new DateTime(2026, 6, 9, 12, 0, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Amanda Foster", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = radioId, DegreeId = mdId,
                    YearsOfExperience = 12, Rating = 4.3m,
                    JoinDate = new DateTime(2014, 11, 20), LastActive = new DateTime(2026, 6, 8, 10, 30, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. James Whitfield", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = cardioId, DegreeId = phdId,
                    YearsOfExperience = 18, Rating = 4.6m,
                    JoinDate = new DateTime(2008, 7, 12), LastActive = new DateTime(2026, 6, 11, 6, 0, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Priya Sharma", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = dermId, DegreeId = mdId,
                    YearsOfExperience = 3, Rating = 4.1m,
                    JoinDate = new DateTime(2023, 4, 1), LastActive = new DateTime(2026, 6, 1, 15, 0, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. David Kim", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = neuroId, DegreeId = doId,
                    YearsOfExperience = 14, Rating = 4.4m,
                    JoinDate = new DateTime(2012, 2, 28), LastActive = new DateTime(2026, 6, 7, 11, 20, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Lisa Nguyen", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = orthoId, DegreeId = mbbsId,
                    YearsOfExperience = 7, Rating = 4.0m,
                    JoinDate = new DateTime(2019, 6, 17), LastActive = new DateTime(2026, 5, 25, 13, 10, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Robert Okafor", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = pedsId, DegreeId = mdId,
                    YearsOfExperience = 20, Rating = 4.9m,
                    JoinDate = new DateTime(2006, 10, 8), LastActive = new DateTime(2026, 6, 10, 7, 45, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Margaret O'Brien", Language = "en" }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(), SpecialtyId = radioId, DegreeId = phdId,
                    YearsOfExperience = 9, Rating = 4.2m,
                    JoinDate = new DateTime(2017, 3, 22), LastActive = new DateTime(2026, 6, 2, 8, 30, 0),
                    DoctorTranslations = new List<DoctorTranslation>
                    {
                        new() { Id = Guid.NewGuid(), DoctorId = Guid.NewGuid(), Name = "Dr. Ahmed Hassan", Language = "en" }
                    }
                }
            };

            // Fix DoctorId references in translations
            for (int i = 0; i < doctors.Count; i++)
            {
                doctors[i].DoctorTranslations[0].DoctorId = doctors[i].Id;
            }

            context.Doctors.AddRange(doctors);
            context.SaveChanges();
        }
    }
}
