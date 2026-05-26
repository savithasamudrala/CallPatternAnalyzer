using CallPatternAnalyzer.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CallPatternAnalyzer.Services;

public class CosmosAnalysisResultService
{
    private readonly Microsoft.Azure.Cosmos.Container _container;

    public CosmosAnalysisResultService(IConfiguration configuration)
    {
        var connectionString = configuration["CosmosDb:ConnectionString"];
        var databaseName = configuration["CosmosDb:DatabaseName"];
        var containerName = configuration["CosmosDb:ContainerName"];

        if (string.IsNullOrWhiteSpace(connectionString) ||
            string.IsNullOrWhiteSpace(databaseName) ||
            string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException("Cosmos DB configuration is missing.");
        }

        var client = new CosmosClient(connectionString);

        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task SaveAnalysisResultAsync(AnalysisResultDocument document)
    {
        await _container.CreateItemAsync(document, new PartitionKey(document.Id));
    }
}