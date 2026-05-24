using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CallPatternAnalyzer.Models;
using CsvHelper;
using System.Globalization;
using System.ComponentModel.Design;
using System.IO;
using CallPatternAnalyzer.Services;

namespace CallPatternAnalyzer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ViewBag.Message = "Please choose a CSV file.";
            return View();
        }

        using var reader = new StreamReader(csvFile.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<CallRecord>()
            .Where(record =>
                !string.IsNullOrWhiteSpace(record.Name) &&
                !string.IsNullOrWhiteSpace(record.StatusName) &&
                record.CallMinutes != null &&
                record.CreatedOn != null)
            .ToList();


        ViewBag.Message = $"Uploaded {csvFile.FileName}. Found {records.Count} rows.";


        var clusteringService = new CallClusteringService();
        var clusterSummaries = clusteringService.CreateClusters(records);


        var results = new UploadResultsViewModel
        {
            Records = records,
            TotalRecords = records.Count,
            AverageCallMinutes = records.Average(record => record.CallMinutes ?? 0),
            CallsByTimeOfDay = records
                    .GroupBy(record => record.TimeOfDayBucket)
                    .OrderBy(group => group.Key)
                    .ToDictionary(group => group.Key, group => group.Count()),
            CallsByStatus = records
                    .GroupBy(record => record.StatusName)
                    .OrderByDescending(group => group.Count())
                    .ToDictionary(group => group.Key, group => group.Count()),
            TopPeopleByCallCount = records
                    .GroupBy(record => record.Name)
                    .OrderByDescending(group => group.Count())
                    .Take(10)
                    .ToDictionary(group => group.Key, group => group.Count()),
            ClusterSummaries = clusterSummaries,
            AverageMinutesByTimeOfDay = records
                .GroupBy(record => record.TimeOfDayBucket)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.Average(record => (double)(record.CallMinutes ?? 0))),
            AverageMinutesByStatus = records
                .GroupBy(record => record.StatusName)
                .OrderByDescending(group => group.Average(record => (double)(record.CallMinutes ?? 0)))
                .ToDictionary(
                    group => group.Key,
                    group => group.Average(record => (double)(record.CallMinutes ?? 0))),
        };

        return View(results);
    }



}
