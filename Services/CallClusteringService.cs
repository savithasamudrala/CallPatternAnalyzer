using CallPatternAnalyzer.Models;
using Microsoft.ML;
using System.Collections.Generic;

namespace CallPatternAnalyzer.Services;

public class CallClusteringService
{
    public List<CallClusterSummary> CreateClusters(List<CallRecord> records)
    {
        var validRecords = records
            .Where(record => record.CallMinutes != null && record.CreatedOn != null)
            .ToList();

        if (validRecords.Count == 0)
        {
            return new List<CallClusterSummary>();
        }

        var mlContext = new MLContext(seed: 1);

        var inputs = validRecords.Select(record => new CallClusterInput
        {
            Name = record.Name,
            StatusName = record.StatusName,
            CallMinutes = record.CallMinutes ?? 0,
            HourOfDay = record.HourOfDay
        }).ToList();

        var data = mlContext.Data.LoadFromEnumerable(inputs);

        var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "StatusNameEncoded",
                inputColumnName: nameof(CallClusterInput.StatusName))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                outputColumnName: "NameEncoded",
                inputColumnName: nameof(CallClusterInput.Name)))
            .Append(mlContext.Transforms.Concatenate(
                "Features",
                nameof(CallClusterInput.CallMinutes),
                nameof(CallClusterInput.HourOfDay),
                "StatusNameEncoded",
                "NameEncoded"))
            .Append(mlContext.Clustering.Trainers.KMeans(
                featureColumnName: "Features",
                numberOfClusters: 4));

        var model = pipeline.Fit(data);

        var predictions = model.Transform(data);

        var predictedRows = mlContext.Data
            .CreateEnumerable<CallClusterPrediction>(predictions, reuseRowObject: false)
            .ToList();

        var recordsWithClusters = validRecords
            .Zip(predictedRows, (record, prediction) => new
            {
                Record = record,
                prediction.ClusterId
            })
            .ToList();

        var summaries = recordsWithClusters
            .GroupBy(item => item.ClusterId)
            .Select(group => new CallClusterSummary
            {
                ClusterId = group.Key,
                RecordCount = group.Count(),
                AverageCallMinutes = group.Average(item => item.Record.CallMinutes ?? 0),
                AverageHourOfDay = group.Average(item => item.Record.HourOfDay),
                MostCommonStatus = group
                    .GroupBy(item => item.Record.StatusName)
                    .OrderByDescending(statusGroup => statusGroup.Count())
                    .First()
                    .Key,
                MostCommonTimeOfDay = group
                    .GroupBy(item => item.Record.TimeOfDayBucket)
                    .OrderByDescending(timeGroup => timeGroup.Count())
                    .First()
                    .Key
            })
            .OrderBy(summary => summary.ClusterId)
            .ToList();

        return summaries;
    }
}