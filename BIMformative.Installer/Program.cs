
using System.Xml.Linq;
using WixSharp;
using File = WixSharp.File;

namespace BIMformative.Installer
{
    internal class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
           try
            {
                var installerProjectRoot = ResolveInstallerProjectRoot();
                                
                var productVersion = GetRequestedVersion(args);
                var targets = GetRequestedTargets(args);

                Console.WriteLine("Installer project root: " + installerProjectRoot);
                Console.WriteLine("Product version: " + productVersion);

                foreach (var target in targets)
                {
                    Console.WriteLine($"Building installer for {target.HostProduct} {target.HostYear}...");

                    var profile = InstallerProfile.Create(
                        installerProjectRoot,
                        target.HostProduct,
                        target.HostYear,
                        productVersion);

                    ValidateInputs(profile);
                    GenerateViewExtensionXml(profile);

                    var msiPath = BuildMsi(profile, installerProjectRoot);

                    Console.WriteLine("MSI created: " + msiPath);
                }

                Console.WriteLine("Done.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Installer build failed:");
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static List<(string HostProduct, string HostYear)> GetRequestedTargets(string[] args)
        {
            if (args == null || args.Length == 0 || string.Equals(args[0], "all", StringComparison.OrdinalIgnoreCase))
            {
                return new List<(string, string)>
                {
                    ("Revit", "2023"),
                    ("Revit", "2024"),
                    ("Revit", "2025"),
                    ("Revit", "2026"),
                    ("Civil3D", "2023"),
                    ("Civil3D", "2024"),
                    ("Civil3D", "2025"),
                    ("Civil3D", "2026")
                };
            }

            if (args.Length < 2)
                throw new ArgumentException("Usgae: <HostProduct> <HostYear> [Version] OR all [Version]");

            return new List<(string, string)>
            {
                (args[0].Trim(), args[1].Trim())
            };
        }

        private static object BuildMsi(InstallerProfile profile, string installerProjectRoot)
        {
            var payloadDir = BuildPayloadDirectory(profile.PayloadSourceDir, profile.InstallDir);
            var xmlDir = new Dir(
                profile.ViewExtensionDir,
                new File(profile.ViewExtensionXmlPath));

            var project = new Project(
                profile.ProductName,
                payloadDir,
                xmlDir)
            {
                Name = profile.ProductName,
                GUID = profile.ProductGuid,
                Version = new Version(profile.ProductVersion),
                OutDir = Path.Combine(installerProjectRoot, "Build", profile.HostProduct, profile.HostYear),
                OutFileName = profile.InstallerFileName,
                SourceBaseDir = installerProjectRoot,
                Platform = Platform.x64,
                Scope = InstallScope.perMachine,
            };

            var licenseFile = Path.Combine(installerProjectRoot, "Assets", "LICENSE.rtf");
            if (System.IO.File.Exists(licenseFile))
            {
                project.LicenceFile = licenseFile;
            }

            project.ControlPanelInfo.Manufacturer = "BIMformative";
            project.ControlPanelInfo.HelpLink = "https://www.bimformative.com/contact";

            var iconFile = Path.Combine(installerProjectRoot, "Assets", "bimformative.ico");
            if (System.IO.File.Exists(iconFile))
            {
                project.ControlPanelInfo.ProductIcon = iconFile;
            }

            project.MajorUpgradeStrategy = new MajorUpgradeStrategy
            {
                UpgradeVersions = VersionRange.ThisAndOlder,
                PreventDowngradingVersions = VersionRange.NewerThanThis,
                NewerProductInstalledErrorMessage = "A newer version is already installed."
            };
            

            WixSharp.MSBuild.EmitAutoGenFiles = true;

            return Compiler.BuildMsi(project);
        }

        private static Dir BuildPayloadDirectory(string sourceDir, string targetDir)
        {
            var entities = BuildDirectoryEntities(sourceDir);
            return new Dir(targetDir, entities.ToArray());
;        }

        private static List<WixEntity> BuildDirectoryEntities(string sourceDir)
        {
            var entities = new List<WixEntity>();

            foreach (var directory in Directory.GetDirectories(sourceDir).OrderBy(x => x))
            {
                var childEntities = BuildDirectoryEntities(directory);
                entities.Add(new Dir(Path.GetFileName(directory), childEntities.ToArray()));
            } 

            foreach (var file in Directory.GetFiles(sourceDir).OrderBy(x => x))
            {
                if (!ShouldIncludeFile(file))
                    continue;

                entities.Add(new File(file));
            }

            return entities;
        }

        private static bool ShouldIncludeFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            if (string.Equals(extension, ".pdb", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private static void GenerateViewExtensionXml(InstallerProfile profile)
        {
            Directory.CreateDirectory(profile.GeneratedFilesDir);

            var installedAssemblyPath = string.Format(
                @"C:\ProgramData\BIMformative\DynamoExtension\{0}\{1}\BIMformative.DynamoExtension.dll",
                profile.HostProduct,
                profile.HostYear);

            var doc = new XDocument(
                new XElement("ViewExtensionDefinition",
                    new XElement("AssemblyPath", installedAssemblyPath),
                    new XElement("TypeName", "BIMformative.DynamoExtension.BIMformativeViewExtension")
                )
            );

            doc.Save(profile.ViewExtensionXmlPath);
        }

        private static void ValidateInputs(InstallerProfile profile)
        {
            if (!Directory.Exists(profile.PayloadSourceDir))
            {
                throw new DirectoryNotFoundException(
                    "Payload source directory not found: " + profile.PayloadSourceDir);
            }

            var mainDll = Path.Combine(profile.PayloadSourceDir, "BIMformative.DynamoExtension.dll");
            if (!System.IO.File.Exists(mainDll))
            {
                throw new FileNotFoundException(
                    "Main extension DLL not found in payload folder.",
                    mainDll);
            }
        }

        private static string ResolveInstallerProjectRoot()
        {
            var current = AppContext.BaseDirectory;
            var dir = new DirectoryInfo(current);

            while (dir != null)
            {
                var projectFile = Path.Combine(dir.FullName, "BIMformative.Installer.csproj");
                if (System.IO.File.Exists(projectFile))
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException(
                "Could not locate BIMformative.Installer.csproj from " + current);
        }

        private static List<string> GetRequestedYears(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return new List<string> { "2023", "2024", "2025", "2026" };
            }

            var first = args[0].Trim();

            if (string.Equals(first, "all", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string> { "2023", "2024", "2025", "2026" };
            }

            return new List<string> { first };
        }

        private static string GetRequestedVersion(string[] args)
        {
            if (args == null || args.Length == 0)
                return "1.0.0";

            if (string.Equals(args[0], "all", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
                    return args[1].Trim();

                return "1.0.0";
            }

            if (args.Length >= 3 && !string.IsNullOrWhiteSpace(args[2]))
                return args[2].Trim();

            return "1.0.0";
        }
    }
}
