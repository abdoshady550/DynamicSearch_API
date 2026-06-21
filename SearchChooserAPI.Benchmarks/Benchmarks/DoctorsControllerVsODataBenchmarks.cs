using BenchmarkDotNet.Attributes;
using Meccano.DynamicQuery;
using Microsoft.EntityFrameworkCore;
using SearchChooserAPI.Models.Req;

namespace SearchChooserAPI.Benchmarks.Benchmarks;

public class DoctorsControllerVsODataBenchmarks : BenchmarkBase
{
    [Benchmark(Description = "Dynamic - Get all doctors")]
    public async Task SearchDoctors_GetAll()
    {
        await RunQuery(Req());
    }

    [Benchmark(Description = "OData - Get all doctors")]
    public async Task OData_GetAll()
    {
        var items = await _service.GetDoctorsQuery().ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Dynamic - Search by name")]
    public async Task SearchDoctors_SearchByName()
    {
        var r = Req();
        r.Search = "John";
        await RunQuery(r);
    }

    [Benchmark(Description = "OData - Search by name")]
    public async Task OData_SearchByName()
    {
        var items = await _service.GetDoctorsQuery()
            .Where(d => (d.DoctorName != null && d.DoctorName.Contains("John")) ||
                        (d.SpecialtyName != null && d.SpecialtyName.Contains("John")) ||
                        (d.Degree != null && d.Degree.Contains("John")))
            .ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Dynamic - Filter by experience")]
    public async Task SearchDoctors_FilterByExperience()
    {
        var r = Req();
        r.Filters =
        [
            new FilterCriteria
            {
                ColumnName = "YearsOfExperience",
                Operator = FilterOperator.Gt,
                Value = "10"
            }
        ];
        await RunQuery(r);
    }

    [Benchmark(Description = "OData - Filter by experience")]
    public async Task OData_FilterByExperience()
    {
        var items = await _service.GetDoctorsQuery()
            .Where(d => d.YearsOfExperience > 10)
            .ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Dynamic - Complex A (Projection + Multi-sort + Paging)")]
    public async Task SearchDoctors_ComplexA()
    {
        var r = new DoctorSearchRequest
        {
            PageSize = 10,
            PageNumber = 2,
            Columns = ["DoctorId", "DoctorName", "SpecialtyName", "YearsOfExperience", "Rating"],
            Mode = ColumnMode.Include,
            SortOptions =
            [
                new SortOption { PropertyName = "SpecialtyName", IsDescending = false },
                new SortOption { PropertyName = "Rating", IsDescending = true }
            ]
        };
        await RunQuery(r);
    }

    [Benchmark(Description = "OData - Complex A (Projection + Multi-sort + Paging)")]
    public async Task OData_ComplexA()
    {
        var items = await _service.GetDoctorsQuery()
            .OrderBy(d => d.SpecialtyName)
            .ThenByDescending(d => d.Rating)
            .Skip(10)
            .Take(10)
            .Select(d => new { d.DoctorId, d.DoctorName, d.SpecialtyName, d.YearsOfExperience, d.Rating })
            .ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Dynamic - Complex B (Multi-filter + Projection + Sort + Paging)")]
    public async Task SearchDoctors_ComplexB()
    {
        var r = new DoctorSearchRequest
        {
            PageSize = 5,
            PageNumber = 1,
            Columns = ["DoctorId", "DoctorName", "YearsOfExperience", "Rating"],
            Mode = ColumnMode.Include,
            Filters =
            [
                new FilterCriteria
                {
                    ColumnName = "YearsOfExperience",
                    Operator = FilterOperator.Gt,
                    Value = "5"
                },
                new FilterCriteria
                {
                    ColumnName = "Rating",
                    Operator = FilterOperator.Gt,
                    Value = "3.0"
                }
            ],
            SortOptions =
            [
                new SortOption { PropertyName = "Rating", IsDescending = true }
            ]
        };
        await RunQuery(r);
    }

    [Benchmark(Description = "OData - Complex B (Multi-filter + Projection + Sort + Paging)")]
    public async Task OData_ComplexB()
    {
        var items = await _service.GetDoctorsQuery()
            .Where(d => d.YearsOfExperience > 5 && d.Rating > 3.0m)
            .OrderByDescending(d => d.Rating)
            .Skip(0)
            .Take(5)
            .Select(d => new { d.DoctorId, d.DoctorName, d.YearsOfExperience, d.Rating })
            .ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }

    [Benchmark(Description = "Dynamic - Complex C (Search + Filter + Pagination)")]
    public async Task SearchDoctors_ComplexC()
    {
        var r = new DoctorSearchRequest
        {
            PageSize = 10,
            PageNumber = 1,
            Search = "Cardiology",
            Filters =
            [
                new FilterCriteria
                {
                    ColumnName = "YearsOfExperience",
                    Operator = FilterOperator.Gt,
                    Value = "10"
                }
            ]
        };
        await RunQuery(r);
    }

    [Benchmark(Description = "OData - Complex C (Search + Filter + Pagination)")]
    public async Task OData_ComplexC()
    {
        var items = await _service.GetDoctorsQuery()
            .Where(d => d.YearsOfExperience > 10 &&
                ((d.DoctorName != null && d.DoctorName.Contains("Cardiology")) ||
                 (d.SpecialtyName != null && d.SpecialtyName.Contains("Cardiology")) ||
                 (d.Degree != null && d.Degree.Contains("Cardiology"))))
            .Skip(0)
            .Take(10)
            .ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Expected at least one result");
    }
}
