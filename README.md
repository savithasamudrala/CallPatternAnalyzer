# Call Pattern Analyzer

Call Pattern Analyzer is an ASP.NET Core MVC application that uploads call data from a CSV file, analyzes call timing patterns, runs ML.NET clustering, renders a simple dashboard, and stores each analysis result in Azure Cosmos DB.

## Features

- ASP.NET Core MVC site with no authentication
- CSV upload for files with these required columns: `Name`, `StatusName`, `CallMinutes`, `CreatedOn`
- CSV parsing and cleanup with CsvHelper
- Summary analysis for call volume, call status, time of day, and average call duration
- ML.NET K-Means clustering to group similar call records
- Dashboard-style result page with key insights, stat cards, tables, and visual bars
- Cosmos DB persistence for each analysis run
- Friendly validation messages for missing columns, invalid CSV data, and Cosmos DB save issues

## Tech Stack

- C# / ASP.NET Core MVC
- .NET 10
- ML.NET
- CsvHelper
- Azure Cosmos DB SDK
- Azure Cosmos DB Emulator for local development
- HTML, CSS, Bootstrap

## Input Data

The uploaded CSV must include these headers:

```csv
Name,StatusName,CallMinutes,CreatedOn
```

Example row:

```csv
Alyssa Shelton,Timed Out,1,9/29/2025 7:51 AM
```

The app derives additional features from the CSV, including hour of day and a time-of-day bucket: Morning, Afternoon, Evening, or Night.

## Analysis Approach

The application uses two analysis approaches:

1. Summary analysis compares call volume and average call duration across time of day, status, and staff member.
2. ML.NET K-Means clustering groups records using call duration, hour of day, status, and name.

The clustering output is summarized into readable cluster groups showing record count, average call minutes, average hour, most common status, and most common time of day.

## Cosmos DB Storage

Each upload creates an analysis result document in Cosmos DB. The saved document includes:

- File name
- Upload timestamp
- Total valid records
- Average call minutes
- Calls by time of day
- Calls by status
- Average minutes by time of day
- Average minutes by status
- ML.NET cluster summaries
- Key insight sentences

Local development uses the Azure Cosmos DB Emulator.

Expected local Cosmos DB setup:

```text
Database Id: CallPatternAnalyzerDb
Container Id: AnalysisResults
Partition Key: /id
```

Local settings are stored in `appsettings.Development.json` under:

```json
"CosmosDb": {
  "ConnectionString": "AccountEndpoint=https://localhost:8081/;AccountKey=...;",
  "DatabaseName": "CallPatternAnalyzerDb",
  "ContainerName": "AnalysisResults"
}
```

For Azure deployment, the Cosmos DB values should be configured as App Service application settings instead of hardcoded in source code.

## Run Locally

1. Start the Azure Cosmos DB Emulator.
2. Confirm the emulator data explorer opens:

```text
https://localhost:8081/_explorer/index.html
```

3. Create the database and container listed above.
4. Run the MVC app:

```powershell
dotnet run
```

5. Open the local URL shown in the terminal and go to:

```text
/Home/Upload
```

6. Upload a CSV file with the required columns.

## Project Structure

```text
Controllers/
  HomeController.cs

Models/
  AnalysisResultDocument.cs
  CallClusterInput.cs
  CallClusterPrediction.cs
  CallClusterSummary.cs
  CallRecord.cs
  UploadResultsViewModel.cs

Services/
  CallClusteringService.cs
  CosmosAnalysisResultService.cs

Views/Home/
  Upload.cshtml

wwwroot/css/
  site.css
```

## Notes

The ML.NET clustering results are exploratory. The dashboard also includes direct relationship summaries, such as average call duration by time of day and by status, so the output remains interpretable even when clusters are broad.
