using CallPatternAnalyzer.Models;
using CallPatternAnalyzer.Services;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CallPatternAnalyzer.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

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


        List<CallRecord> records;

        try
        {
            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            records = csv.GetRecords<CallRecord>()
                .Where(record =>
                    !string.IsNullOrWhiteSpace(record.Name) &&
                    !string.IsNullOrWhiteSpace(record.StatusName) &&
                    record.CallMinutes != null &&
                    record.CreatedOn != null)
                .ToList();
        }
        catch (HeaderValidationException)
        {
            ViewBag.ErrorMessage = "The CSV file is missing one or more required columns: Name, StatusName, CallMinutes, CreatedOn.";
            return View();
        }
        catch (TypeConverterException)
        {
            ViewBag.ErrorMessage = "The CSV file contains invalid values. Please check that CallMinutes is numeric and CreatedOn is a valid date.";
            return View();
        }
        catch (CsvHelperException)
        {
            ViewBag.ErrorMessage = "The CSV file could not be read. Please confirm it is a valid CSV file.";
            return View();
        }


        if (!records.Any())
        {
            ViewBag.ErrorMessage = "No valid records were found in the uploaded CSV.";
            return View();
        }


        ViewBag.Message = $"Uploaded {csvFile.FileName}. Found {records.Count} rows.";


        var clusteringService = new CallClusteringService();
        var clusterSummaries = clusteringService.CreateClusters(records);

        var busiestTimeOfDay = records
            .GroupBy(record => record.TimeOfDayBucket)
            .OrderByDescending(group => group.Count())
            .First();

        var longestTimeOfDay = records
            .GroupBy(record => record.TimeOfDayBucket)
            .OrderByDescending(group => group.Average(record => (double)(record.CallMinutes ?? 0)))
            .First();

        var longestStatus = records
            .GroupBy(record => record.StatusName)
            .OrderByDescending(group => group.Average(record => (double)(record.CallMinutes ?? 0)))
            .First();

        var largestCluster = clusterSummaries
            .OrderByDescending(cluster => cluster.RecordCount)
            .FirstOrDefault();

        var keyInsights = new List<string>
        {
            $"{busiestTimeOfDay.Key} has the highest call volume with {busiestTimeOfDay.Count()} records.",
            $"{longestTimeOfDay.Key} has the highest average call duration at {longestTimeOfDay.Average(record => (double)(record.CallMinutes ?? 0)):0.00} minutes.",
            $"{longestStatus.Key} has the highest average call duration by status at {longestStatus.Average(record => (double)(record.CallMinutes ?? 0)):0.00} minutes."
        };

        if (largestCluster != null)
        {
            keyInsights.Add($"The largest ML.NET cluster contains {largestCluster.RecordCount} records with an average duration of {largestCluster.AverageCallMinutes:0.00} minutes.");
        }


        var results = new UploadResultsViewModel
        {
            Records = records,
            TotalRecords = records.Count,
            AverageCallMinutes = records.Average(record => record.CallMinutes ?? 0),
            KeyInsights = keyInsights,
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

        var analysisDocument = new AnalysisResultDocument
        {
            FileName = csvFile.FileName,
            TotalRecords = results.TotalRecords,
            AverageCallMinutes = results.AverageCallMinutes,
            CallsByTimeOfDay = results.CallsByTimeOfDay,
            CallsByStatus = results.CallsByStatus,
            AverageMinutesByTimeOfDay = results.AverageMinutesByTimeOfDay,
            AverageMinutesByStatus = results.AverageMinutesByStatus,
            ClusterSummaries = results.ClusterSummaries,
            KeyInsights = results.KeyInsights
        };

        try
        {
            var cosmosService = new CosmosAnalysisResultService(_configuration);
            await cosmosService.SaveAnalysisResultAsync(analysisDocument);

            ViewBag.SavedToCosmos = true;
            ViewBag.CosmosDocumentId = analysisDocument.Id;
        }
        catch (CosmosException)
        {
            ViewBag.SaveWarning = "The analysis completed, but the result could not be saved to Cosmos DB.";
        }
        catch (InvalidOperationException)
        {
            ViewBag.SaveWarning = "The analysis completed, but Cosmos DB configuration is missing.";
        }

        return View(results);
    }



}
