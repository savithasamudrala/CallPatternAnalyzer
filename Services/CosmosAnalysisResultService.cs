using CallPatternAnalyzer.Models;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace CallPatternAnalyzer.Services;

public class CosmosAnalysisResultService
{
    private const string ConnectionString =
        "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;";

    private const string DatabaseName = "CallPatternAnalyzerDb";

    private const string ContainerName = "AnalysisResults";

    public async Task SaveAnalysisResultAsync(AnalysisResultDocument document)
    {
        using var client = new CosmosClient(ConnectionString);

        var container = client.GetContainer(DatabaseName, ContainerName);

        await container.CreateItemAsync(document, new PartitionKey(document.Id));
    }
}