using Azure;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

// https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/appconfiguration/Azure.Data.AppConfiguration

namespace AppConfigurationSync;

class Program
{
    static async Task Main(string[] args)
    {
        // add app configuration with user secrets
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();

        var configuration = builder.Build();

        var sourceAppConfig = configuration["ConnectionStrings:SourceAppConfig"];
        var destinationAppConfig = configuration["ConnectionStrings:DestinationAppConfig"];
        
        AnsiConsole.Write(
            new FigletText("App Config Sync Tool")
                .LeftAligned()
                .Color(Color.Teal));
        
        var destinationClient = new ConfigurationClient(destinationAppConfig);

        var createDestinationSnapshot = AnsiConsole
            .Prompt(
                new SelectionPrompt<bool> { Converter = value => value ? "Yes" : "No" }
                    .Title($"Create snapshot of destination configuration?")
                    .AddChoices(true, false));

        if (createDestinationSnapshot)
        {
            await CreateSnapshot(destinationClient);
        }
        
        var sourceClient = new ConfigurationClient(sourceAppConfig);
        var selector = new SettingSelector()
        {
        };

        var sourceSettings = new List<ConfigurationSetting>(); 
            
        await foreach (var setting in sourceClient.GetConfigurationSettingsAsync(selector))
        {
            sourceSettings.Add(setting);
        }
        
        var destinationSettings = new List<ConfigurationSetting>();
        
        await foreach (var setting in destinationClient.GetConfigurationSettingsAsync(selector))
        {
            destinationSettings.Add(setting);
        }

        AnsiConsole.MarkupLine("[bold]Keys in [red]red[/] are missing in the destination[/]");
        AnsiConsole.MarkupLine("[bold]Keys in [green]green[/] are present in the destination but with a different value[/]");
        AnsiConsole.WriteLine();
        
        var labelsToIgnore = new[] { "Development", "Staging" };
        
        foreach (var setting in sourceSettings.Where(s => !labelsToIgnore.Contains(s.Label)))
        {
            var existsInDestination = destinationSettings.Any(s => s.Key == setting.Key
                && s.Label == setting.Label);
            
            var valuesMatch = destinationSettings.Any(s => s.Key == setting.Key
                && s.Label == setting.Label
                && s.Value == setting.Value);

            if (existsInDestination && valuesMatch)
            {
                continue;
            }
            
            var color = existsInDestination ? "[green]" : "[red]";
            var valueColor = valuesMatch ? "[green]" : "[red]";
            
            AnsiConsole.MarkupLine(string.IsNullOrEmpty(setting.Label)
                ? $"[bold]{color}{setting.Key}[/][/]: {valueColor}{setting.Value}[/]"
                : $"[bold]{color}{setting.Key}[/][/]: {valueColor}{setting.Value}[/] [grey]({setting.Label})[/]");
        }
    }

    private static async Task CreateSnapshot(ConfigurationClient sourceClient)
    {
        var snapshotNamePrefix = $"{DateTime.Now:yyyy-MM-dd}_";
        var existingSnapshots = sourceClient
            .GetSnapshotsAsync(new SnapshotSelector()
            {
                NameFilter = $"{snapshotNamePrefix}*"
            });
        
        var listOfSnapshots = new List<ConfigurationSnapshot>();
        
        await foreach (var ss in existingSnapshots)
        {
            listOfSnapshots.Add(ss);
        }
        
        var snapshotName = $"{snapshotNamePrefix}{listOfSnapshots.Count + 1}";
        while(listOfSnapshots.Any(s => s.Name == snapshotName))
        {
            snapshotName = $"{snapshotNamePrefix}{listOfSnapshots.Count + 1}";
        }
        
        var configurationSnapshot = new ConfigurationSnapshot(new[]
        {
            new ConfigurationSettingsFilter("*") { Label = "*"}
        })
        {
            SnapshotComposition = SnapshotComposition.KeyLabel
        };

        await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Clock)
            .StartAsync(
                $"Creating snapshot {snapshotName}...",
                _ => sourceClient.CreateSnapshotAsync(WaitUntil.Completed, snapshotName, configurationSnapshot));
    }
}