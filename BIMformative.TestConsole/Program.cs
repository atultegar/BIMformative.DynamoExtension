// See https://aka.ms/new-console-template for more information
using BIMformative.Infrastructure;
using BIMformative.Infrastructure.Db;
using BIMformative.TestConsole.Services;

class Program
{
    static async Task Main(string[] args)
    {
        using var db = DatabaseBootstrapper.Initialize();

        var importer = new DataImportService(db);

        if (args.Contains("import"))
        {
            await importer.ImportDownloadedScriptsAsync("data/oldData.json");
            Console.WriteLine("Import completed.");
        }
    }
}