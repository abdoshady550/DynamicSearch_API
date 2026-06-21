using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Data;
using SearchChooserAPI.Models;

namespace SearchChooserAPI.Benchmarks.Business;

public static class BenchmarkDataSeeder
{
    private static readonly string[] FirstNames =
    [
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Lisa", "Daniel", "Nancy",
        "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley",
        "Steven", "Dorothy", "Andrew", "Kimberly", "Paul", "Emily", "Joshua", "Donna",
        "Kenneth", "Michelle", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa",
        "Timothy", "Deborah", "Ronald", "Stephanie", "Edward", "Rebecca", "Jason", "Sharon",
        "Jeffrey", "Laura", "Ryan", "Cynthia", "Jacob", "Kathleen", "Gary", "Angela",
        "Nicholas", "Amy", "Eric", "Irene", "Jonathan", "Anna", "Stephen", "Brenda",
        "Larry", "Pamela", "Justin", "Nicole", "Scott", "Emma", "Brandon", "Samantha",
        "Benjamin", "Katherine", "Samuel", "Christine", "Raymond", "Debra", "Gregory", "Rachel",
        "Frank", "Carolyn", "Alexander", "Janet", "Patrick", "Catherine", "Jack", "Maria",
        "Dennis", "Heather", "Jerry", "Diane", "Ahmed", "Priya", "Wei", "Olga", "Carlos"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
        "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker",
        "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell",
        "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker",
        "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy",
        "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey",
        "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson",
        "Watson", "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza",
        "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers",
        "Long", "Ross", "Foster", "Jimenez", "Hassan", "Dubois", "Schmidt", "Johansson"
    ];

    private static readonly (string Name, string Snomed)[] SpecialtyDefs =
    [
        ("Cardiology", "394579002"),
        ("Dermatology", "394582007"),
        ("Neurology", "394587006"),
        ("Orthopedics", "394588001"),
        ("Pediatrics", "394589009"),
        ("Radiology", "394590005"),
        ("Ophthalmology", "394591009"),
        ("Pulmonology", "394594001"),
        ("Gastroenterology", "394595000"),
        ("Endocrinology", "394596004"),
        ("Rheumatology", "394597008"),
        ("Urology", "394598003")
    ];

    private static readonly string[] DegreeNames =
    [
        "MD (Doctor of Medicine)",
        "PhD in Immunology",
        "DO (Doctor of Osteopathy)",
        "MBBS (Bachelor of Medicine)",
        "DDS (Doctor of Dental Surgery)"
    ];

    public static async Task SeedAsync(SearchDbContext context, int doctorCount, bool addIndexes = false)
    {
        context.Database.EnsureCreated();

        if (await context.Doctors.AnyAsync())
            return;

        var rng = new Random(42);

        var specialties = CreateSpecialties();
        var degrees = CreateDegrees();

        context.Specialties.AddRange(specialties);
        context.Degrees.AddRange(degrees);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var batchSize = Math.Min(1000, Math.Max(100, doctorCount / 10));
        var doctors = new List<Doctor>(batchSize);

        for (var i = 0; i < doctorCount; i++)
        {
            doctors.Add(CreateDoctor(i, specialties, degrees, rng));
            if (doctors.Count >= batchSize)
            {
                context.Doctors.AddRange(doctors);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                doctors.Clear();
            }
        }

        if (doctors.Count > 0)
        {
            context.Doctors.AddRange(doctors);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        if (addIndexes)
        {
            await TryCreateIndexAsync(context, "Doctors", "IX_Doctor_YearsOfExperience", "YearsOfExperience");
            await TryCreateIndexAsync(context, "Doctors", "IX_Doctor_Rating", "Rating");
            await TryCreateIndexAsync(context, "Doctors", "IX_Doctor_SpecialtyId", "SpecialtyId");
            await TryCreateIndexAsync(context, "Doctors", "IX_Doctor_DegreeId", "DegreeId");
            await TryCreateIndexAsync(context, "DoctorTranslations", "IX_DoctorTranslation_DoctorId", "DoctorId");
            await TryCreateIndexAsync(context, "DoctorTranslations", "IX_DoctorTranslation_Language", "Language");
        }
    }

    private static async Task TryCreateIndexAsync(SearchDbContext context, string table, string indexName, string column)
    {
        try
        {
#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync(
                $"""
                 IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = '{indexName}')
                     CREATE INDEX [{indexName}] ON [{table}] ([{column}])
                 """);
#pragma warning restore EF1002
        }
        catch
        {
        }
    }

    private static List<Specialty> CreateSpecialties()
    {
        return SpecialtyDefs.Select(s =>
        {
            var id = Guid.NewGuid();
            return new Specialty
            {
                Id = id,
                SnomedCode = s.Snomed,
                SpecialtyTranslations = new List<SpecialtyTranslation>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        SpecialtyId = id,
                        Name = s.Name,
                        Language = "en"
                    }
                }
            };
        }).ToList();
    }

    private static List<Degree> CreateDegrees()
    {
        return DegreeNames.Select(d =>
        {
            var id = Guid.NewGuid();
            return new Degree
            {
                Id = id,
                DegreeTranslations = new List<DegreeTranslation>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        DegreeId = id,
                        Name = d,
                        Language = "en"
                    }
                }
            };
        }).ToList();
    }

    private static Doctor CreateDoctor(int index, List<Specialty> specialties, List<Degree> degrees, Random rng)
    {
        var id = Guid.NewGuid();
        var specialty = specialties[index % specialties.Count];
        var degree = degrees[index % degrees.Count];
        var firstName = FirstNames[rng.Next(FirstNames.Length)];
        var lastName = LastNames[rng.Next(LastNames.Length)];

        var joinDate = new DateTime(2000 + rng.Next(25), rng.Next(1, 13), rng.Next(1, 29));

        return new Doctor
        {
            Id = id,
            SpecialtyId = specialty.Id,
            DegreeId = degree.Id,
            YearsOfExperience = rng.Next(0, 41),
            Rating = Math.Round((decimal)(rng.NextDouble() * 4.0 + 1.0), 1),
            JoinDate = joinDate,
            LastActive = joinDate.AddYears(rng.Next(1, 5)).AddDays(rng.Next(1, 365)),
            DoctorTranslations = new List<DoctorTranslation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DoctorId = id,
                    Name = $"Dr. {firstName} {lastName}",
                    Language = "en"
                }
            }
        };
    }
}
