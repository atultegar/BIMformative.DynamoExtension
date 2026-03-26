//css_reference %userprofile%\source\repos\COWIToolsDesktopApp\src\CW.DesktopApp.Core\bin\Debug\netstandard2.0\CW.DesktopApp.Core.dll;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CW.DesktopApp.Core.Contracts;
using System.Xml.Linq;
using System.IO.Compression;
using System.Text;

public class Program : IInstallScriptV1
{
   static async Task Main(string[] args)
    {
        var script = new Program();
        var file = File.OpenRead(args[0]);
        var zip = new ZipArchive(file);
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        zip.ExtractToDirectory(tempDir);
        try
        {
            var requireAdminToInstall = script.RequiresAdminToInstall(tempDir);
            var requireAdminToUninstall = script.RequiresAdminToUninstall();
            await script.InstallAsync(tempDir);
            //await script.UninstallAsync();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static readonly string _extensionXmlName = @"COWI-Tools_ExtensionDefinition.xml";
    private static readonly string _viewExtensionXmlName = @"COWI-Tools_ViewExtensionDefinition.xml";
    private static readonly string _extensionAssemblyName = @"CW.Dynamo.Extensions.dll";
    private static readonly string _dynamoCoreDir = @"C:\Program Files\Dynamo\Dynamo Core\2";
    private static readonly string _dynamoRevit2020Dir = @"C:\Program Files\Autodesk\Revit 2020\AddIns\DynamoForRevit";
    private static readonly string _dynamoRevit2021Dir = @"C:\Program Files\Autodesk\Revit 2021\AddIns\DynamoForRevit";
    private static readonly string _dynamoRevit2022Dir = @"C:\Program Files\Autodesk\Revit 2022\AddIns\DynamoForRevit";
    private static readonly string _dynamoRevit2023Dir = @"C:\Program Files\Autodesk\Revit 2023\AddIns\DynamoForRevit";
    private static readonly string _dynamoRevit2024Dir = @"C:\Program Files\Autodesk\Revit 2024\AddIns\DynamoForRevit";
    private static readonly string _dynamoRevit2025Dir = @"C:\Program Files\Autodesk\Revit 2025\AddIns\DynamoForRevit";
    private static readonly string _dynamoAutoCAD2020Dir = @"C:\Program Files\Autodesk\AutoCAD 2020\C3D\Dynamo\Core";
    private static readonly string _dynamoAutoCAD2021Dir = @"C:\Program Files\Autodesk\AutoCAD 2021\C3D\Dynamo\Core";
    private static readonly string _dynamoAutoCAD2022Dir = @"C:\Program Files\Autodesk\AutoCAD 2022\C3D\Dynamo\Core";
    private static readonly string _dynamoAutoCAD2023Dir = @"C:\Program Files\Autodesk\AutoCAD 2023\C3D\Dynamo\Core";
    private static readonly string _dynamoAutoCAD2024Dir = @"C:\Program Files\Autodesk\AutoCAD 2024\C3D\Dynamo\Core";

    private static readonly string _appRootPath = @$"C:\ProgramData\COWI Tools\apps\{_appName}";
    private static string _appName => "CW.COWI-Tools.Dynamo";

    private static string CalculateAppDir(string version) => Path.Combine(_appRootPath, version);

    public List<string> GetFilesToEdit()
    {
        if (!Directory.Exists(_appRootPath))
            return new List<string>();

        return Directory.GetFiles(_appRootPath, "*", SearchOption.AllDirectories).ToList();
    }

    public bool RequiresAdminToInstall(string extractedPackageDir)
    {
        return !(IsXmlUpToDate(extractedPackageDir, "2.0", _dynamoCoreDir)
                 && IsXmlUpToDate(extractedPackageDir, "2.3", _dynamoRevit2020Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.6", _dynamoRevit2021Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.12", _dynamoRevit2022Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.16", _dynamoRevit2023Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.17", _dynamoRevit2024Dir)
                 && IsXmlUpToDate(extractedPackageDir, "3.0", _dynamoRevit2025Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.12", _dynamoAutoCAD2020Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.12", _dynamoAutoCAD2021Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.12", _dynamoAutoCAD2022Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.15", _dynamoAutoCAD2023Dir)
                 && IsXmlUpToDate(extractedPackageDir, "2.16", _dynamoAutoCAD2024Dir)
                 );
    }

    public bool RequiresAdminToUninstall()
    {
        var dynamoXmlFiles = new List<string>();
        var coreXml = GetDynamoXmlPaths(_dynamoCoreDir);
        var R2020Xml = GetDynamoXmlPaths(_dynamoRevit2020Dir);
        var R2021Xml = GetDynamoXmlPaths(_dynamoRevit2021Dir);
        var R2022Xml = GetDynamoXmlPaths(_dynamoRevit2022Dir);
        var R2023Xml = GetDynamoXmlPaths(_dynamoRevit2023Dir);
        var R2024Xml = GetDynamoXmlPaths(_dynamoRevit2024Dir);
        var R2025Xml = GetDynamoXmlPaths(_dynamoRevit2025Dir);

        var Acad2020Xml = GetDynamoXmlPaths(_dynamoAutoCAD2020Dir);
        var Acad2021Xml = GetDynamoXmlPaths(_dynamoAutoCAD2021Dir);
        var Acad2022Xml = GetDynamoXmlPaths(_dynamoAutoCAD2022Dir);
        var Acad2023Xml = GetDynamoXmlPaths(_dynamoAutoCAD2023Dir);
        var Acad2024Xml = GetDynamoXmlPaths(_dynamoAutoCAD2024Dir);

        dynamoXmlFiles.AddRange(coreXml);
        dynamoXmlFiles.AddRange(R2020Xml);
        dynamoXmlFiles.AddRange(R2021Xml);
        dynamoXmlFiles.AddRange(R2022Xml);
        dynamoXmlFiles.AddRange(R2023Xml);
        dynamoXmlFiles.AddRange(R2024Xml);
        dynamoXmlFiles.AddRange(R2025Xml);

        dynamoXmlFiles.AddRange(Acad2020Xml);
        dynamoXmlFiles.AddRange(Acad2021Xml);
        dynamoXmlFiles.AddRange(Acad2022Xml);
        dynamoXmlFiles.AddRange(Acad2023Xml);
        dynamoXmlFiles.AddRange(Acad2024Xml);

        var exist = dynamoXmlFiles.Any(x => File.Exists(x));
        return exist;
    }

    public Task InstallAsync(string extractedPackageDir)
    {
        InstallDynamo(extractedPackageDir, "2.0", _dynamoCoreDir);
        InstallDynamo(extractedPackageDir, "2.3", _dynamoRevit2020Dir);
        InstallDynamo(extractedPackageDir, "2.6", _dynamoRevit2021Dir);
        InstallDynamo(extractedPackageDir, "2.12", _dynamoRevit2022Dir);
        InstallDynamo(extractedPackageDir, "2.16", _dynamoRevit2023Dir);
        InstallDynamo(extractedPackageDir, "2.17", _dynamoRevit2024Dir);
        InstallDynamo(extractedPackageDir, "3.0", _dynamoRevit2025Dir);

        InstallDynamo(extractedPackageDir, "2.12", _dynamoAutoCAD2020Dir);
        InstallDynamo(extractedPackageDir, "2.12", _dynamoAutoCAD2021Dir);
        InstallDynamo(extractedPackageDir, "2.12", _dynamoAutoCAD2022Dir);
        InstallDynamo(extractedPackageDir, "2.15", _dynamoAutoCAD2023Dir);
        InstallDynamo(extractedPackageDir, "2.16", _dynamoAutoCAD2024Dir);

        return Task.CompletedTask;
    }

    public Task UninstallAsync()
    {
        if (Directory.Exists(_appRootPath))
            Directory.Delete(_appRootPath, true);

        RemoveDynamoExtensionXml(_dynamoCoreDir);
        RemoveDynamoExtensionXml(_dynamoRevit2020Dir);
        RemoveDynamoExtensionXml(_dynamoRevit2021Dir);
        RemoveDynamoExtensionXml(_dynamoRevit2022Dir);
        RemoveDynamoExtensionXml(_dynamoRevit2023Dir);
        RemoveDynamoExtensionXml(_dynamoRevit2024Dir);
        RemoveDynamoExtensionXml(_dynamoRevit2025Dir);

        RemoveDynamoExtensionXml(_dynamoAutoCAD2020Dir);
        RemoveDynamoExtensionXml(_dynamoAutoCAD2021Dir);
        RemoveDynamoExtensionXml(_dynamoAutoCAD2022Dir);
        RemoveDynamoExtensionXml(_dynamoAutoCAD2023Dir);
        RemoveDynamoExtensionXml(_dynamoAutoCAD2024Dir);
        return Task.CompletedTask;
    }

    private void RemoveDynamoExtensionXml(string dynamoDir)
    {
        if (!Directory.Exists(dynamoDir))
            return;

        var extensionsFile = GetDynamoExtensionXmlPath(dynamoDir);
        if (File.Exists(extensionsFile))
            File.Delete(extensionsFile);

        var viewExtensionFile = GetDynamoViewExtensionXmlPath(dynamoDir);
        if (File.Exists(viewExtensionFile))
            File.Delete(viewExtensionFile);
    }


    /* #region Required admin methods */

    private bool IsXmlUpToDate(string extractedPackageDir, string version, string dynamoDir)
    {
        if (!Directory.Exists(dynamoDir))
            return false;

        var assetsDir = Path.Combine(extractedPackageDir, "assets");
        var contentsDir = Path.Combine(extractedPackageDir, "Contents");

        var newFilesDir = Path.Combine(contentsDir, version);
        var installDir = CalculateAppDir(version);
        var isExtensionXmlUpToDate = IsExtensionXmlUpToDate(installDir, dynamoDir, assetsDir);
        return isExtensionXmlUpToDate;
    }

    private static List<string> GetDynamoXmlPaths(string dynamoDir)
    {
        var paths = new List<string> { GetDynamoExtensionXmlPath(dynamoDir), GetDynamoViewExtensionXmlPath(dynamoDir) };
        return paths;
    }

    private static string GetDynamoExtensionXmlPath(string dynamoDir)
    {
        var extenstionDirectory = Path.Combine(dynamoDir, @"extensions");
        var extensionsFile = Path.Combine(extenstionDirectory, _extensionXmlName);
        return extensionsFile;
    }

    private static string GetDynamoViewExtensionXmlPath(string dynamoDir)
    {
        var extenstionDirectory = Path.Combine(dynamoDir, @"viewExtensions");
        var extensionsFile = Path.Combine(extenstionDirectory, _viewExtensionXmlName);
        return extensionsFile;
    }

    /* #endregion */

    /* #region Installation methods */

    private void InstallDynamo(string extractedPackageDir, string version, string dynamoDir)
    {
        if (!Directory.Exists(dynamoDir))
            Directory.CreateDirectory(dynamoDir);

        var assetsDir = Path.Combine(extractedPackageDir, "assets");
        var contentsDir = Path.Combine(extractedPackageDir, "Contents");

        var newFilesDir = Path.Combine(contentsDir, version);
        var installDir = CalculateAppDir(version);

        Console.WriteLine("Installing to: {0}", installDir);

        OverwriteDirectory(newFilesDir, installDir);
        InstallExtensionXmls(installDir, dynamoDir, assetsDir);
    }

    /* #endregion */

    /* #region  Helper Methods */

    private static void OverwriteDirectory(string extractedDir, string appDir)
    {
        if (Directory.Exists(appDir))
            Directory.Delete(appDir, true);

        var files = Directory.GetFiles(extractedDir, "*", SearchOption.AllDirectories).ToList();

        Directory.CreateDirectory(appDir);
        foreach (var file in files)
        {
            var target = file.Replace(extractedDir, appDir);

            var directory = Path.GetDirectoryName(target);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            File.Copy(file, target);
        }
    }

    private static bool IsExtensionXmlUpToDate(string installationDir, string dynamoDir, string assetsFolder)
    {
        var extensionAssemblyPath = Path.Combine(installationDir, _extensionAssemblyName);

        var existingExtensionsFile = GetDynamoExtensionXmlPath(dynamoDir);
        var extensionTemplatePath = Path.Combine(assetsFolder, _extensionXmlName);
        if (!IsXmlEqual(extensionAssemblyPath, existingExtensionsFile, extensionTemplatePath))
            return false;

        var existingViewExtensionsFile = GetDynamoViewExtensionXmlPath(dynamoDir);
        var viewExtensionTemplatePath = Path.Combine(assetsFolder, _viewExtensionXmlName);
        var isXmlEqual = IsXmlEqual(extensionAssemblyPath, existingViewExtensionsFile, viewExtensionTemplatePath);
        return isXmlEqual;
    }

    private static void InstallExtensionXmls(string extractedDir, string dynamoDir, string assetsFolder)
    {
        if (IsExtensionXmlUpToDate(extractedDir, dynamoDir, assetsFolder))
            return;

        var extensionAssemblyPath = Path.Combine(extractedDir, _extensionAssemblyName);

        var extensionTemplatePath = Path.Combine(assetsFolder, _extensionXmlName);
        var existingExtensionsFile = GetDynamoExtensionXmlPath(dynamoDir);
        CreateOrUpdateExtensionXml(extensionAssemblyPath, existingExtensionsFile, extensionTemplatePath);

        var existingViewExtensionsFile = GetDynamoViewExtensionXmlPath(dynamoDir);
        var ViewExtensionTemplatePath = Path.Combine(assetsFolder, _viewExtensionXmlName);
        CreateOrUpdateExtensionXml(extensionAssemblyPath, existingViewExtensionsFile, ViewExtensionTemplatePath);
    }


    private static bool IsXmlEqual(string extensionAssemblyPath, string existingExtensionsFile, string templateXmlPath)
    {
        if (!File.Exists(existingExtensionsFile))
            return false;

        var xml = GenerateXmlExtension(extensionAssemblyPath, templateXmlPath);

        var existingXml = File.ReadAllText(existingExtensionsFile);
        if (existingXml != xml)
            return false;

        return true;
    }


    private static void CreateOrUpdateExtensionXml(string extensionAssemblyPath, string existingExtensionsFile, string templateXmlPath)
    {
        var xml = GenerateXmlExtension(extensionAssemblyPath, templateXmlPath);
        if (File.Exists(existingExtensionsFile))
        {
            var existingXml = File.ReadAllText(existingExtensionsFile);
            if (existingXml == xml)
                return;

            File.Delete(existingExtensionsFile);
        }
        var dir = Path.GetDirectoryName(existingExtensionsFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
            
        File.WriteAllText(existingExtensionsFile, xml, Encoding.UTF8);
        Console.WriteLine("Creating extension xml: {0}", existingExtensionsFile);
    }

    private static string GenerateXmlExtension(string extensionFilePath, string templateXmlPath)
    {
        var xml = File.ReadAllText(templateXmlPath, System.Text.Encoding.UTF8);
        var doc = XDocument.Parse(xml);
        var assemblyPath = GetAssemblyPathFromXDoc(doc);
        assemblyPath.Value = extensionFilePath;
        return doc.ToString();
    }

    private static XElement GetAssemblyPathFromXDoc(XDocument doc)
    {
        return doc.Descendants("AssemblyPath").First();
    }
}