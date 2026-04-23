using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.Installer
{
    internal sealed class InstallerProfile
    {
        public string HostProduct { get; private set; }
        public string HostYear { get; private set; }
        public string ProductVersion { get; private set; }

        public string ProductName { get; private set; }
        public string InstallerTitle { get; private set; }
        public string InstallerFileName { get; private set; }

        public Guid ProductGuid { get; private set; }
        public Guid UpgradeCode { get; private set; }

        public string PayloadSourceDir { get; private set; }
        public string InstallDir { get; private set; }
        public string ViewExtensionDir { get; private set; }

        public string GeneratedFilesDir { get; private set; }
        public string ViewExtensionXmlPath { get; private set; }

        private InstallerProfile()
        {   
        }

        public static InstallerProfile Create(string installerProjectRoot, string hostProduct, string hostYear, string productVersion)
        {
            if (string.IsNullOrWhiteSpace(installerProjectRoot))
                throw new ArgumentNullException(nameof(installerProjectRoot));

            if (string.IsNullOrWhiteSpace(hostProduct))
                throw new ArgumentNullException(nameof(hostProduct));

            if (string.IsNullOrWhiteSpace(hostYear))
                throw new ArgumentNullException(nameof(hostYear));

            if (string.IsNullOrWhiteSpace(productVersion))
                throw new ArgumentNullException(nameof(productVersion));

            var normalizedHost = NormalizeHostProduct(hostProduct);
            var normalizedYear = hostYear.Trim();

            var guids = GetGuids(normalizedHost, normalizedYear);

            var configurationName = GetConfigurationName(normalizedHost, normalizedYear);

            var payloadSourceDir = Path.GetFullPath(Path.Combine(
                installerProjectRoot,
                "..",
                "BIMformative.DynamoExtension",
                "artifacts",
                configurationName));

            var generatedFilesDir = Path.Combine(
                installerProjectRoot,
                "Build",
                "Generated",
                normalizedHost,
                normalizedYear);

            var installDir = string.Format(
                @"%CommonAppDataFolder%\BIMformative\DynamoExtension\{0}\{1}", 
                normalizedHost, 
                normalizedYear);

            var viewExtensionDir = GetViewExtensionDir(normalizedHost, normalizedYear);

            var profile = new InstallerProfile
            {
                HostProduct = normalizedHost,
                HostYear = normalizedYear,
                ProductVersion = productVersion,

                ProductName = $"BIMformative DynamoExtension for {GetDisplayHostName(normalizedHost)} {normalizedYear}",
                InstallerTitle = $"BIMformative DynamoExtension for {GetDisplayHostName(normalizedHost)} {normalizedYear} Installer",
                InstallerFileName = $"BIMformative.DynamoExtension.{normalizedHost}{normalizedYear}.v{productVersion}",

                ProductGuid = guids.ProductGuid,
                UpgradeCode = guids.UpgradeCode,

                PayloadSourceDir = payloadSourceDir,
                InstallDir = installDir,
                ViewExtensionDir = viewExtensionDir,

                GeneratedFilesDir = generatedFilesDir,
                ViewExtensionXmlPath = Path.Combine(generatedFilesDir, "BIMformative_ViewExtensionDefinition.xml")
            };

            return profile;
        }

        private static string GetDisplayHostName(string hostProduct)
        {
            return hostProduct == "Civil3D" ? "Civil 3D" : hostProduct;
        }

        private static string GetViewExtensionDir(string hostProduct, string hostYear)
        {
            if (hostProduct == "Revit")
            {
                return string.Format(
                    @"%ProgramFiles64Folder%\Autodesk\Revit {0}\AddIns\DynamoForRevit\viewExtensions",
                    hostYear);
            }

            if (hostProduct == "Civil3D")
            {
                return string.Format(
                    @"%ProgramFiles64Folder%\Autodesk\AutoCAD {0}\C3D\Dynamo\Core\viewExtensions",
                    hostYear);
            }

            return string.Format(
                    @"%ProgramFiles64Folder%\Autodesk\AutoCAD {0}\C3D\Dynamo\Core\viewExtensions",
                    hostYear);
        }

        private static string GetConfigurationName(string hostProduct, string hostYear)
        {
            if (hostProduct == "Revit")
                return "Release-Revit" + hostYear;

            if (hostProduct == "Civil3D")
                return "Release-C3D" + hostYear;

            return "Unreleased";
        }

        private static string NormalizeHostProduct(string hostProduct)
        {
            var value = hostProduct.Trim();

            if (value.Equals("Revit", StringComparison.OrdinalIgnoreCase))
                return "Revit";

            if (value.Equals("Civil3D", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("C3D", StringComparison.OrdinalIgnoreCase))
                return "Civil3D";

            throw new NotSupportedException("Unsupported host product: " + hostProduct);
        }

        private static (Guid ProductGuid, Guid UpgradeCode) GetGuids(string hostProduct, string hostYear)
        {
            if (hostProduct == "Revit")
            {
                switch (hostYear)
                {
                    case "2023":
                        return (
                            new Guid("7C13DDA0-BB0A-46A4-8C0F-5E3C533DDE23"),
                            new Guid("22E2384A-8B91-4D58-8D57-7B78FA03A123")
                        );

                    case "2024":
                        return (
                            new Guid("D53C63A6-35A4-4E36-8B84-1E5B2860B124"),
                            new Guid("BBAEEAF2-EF22-4C0B-9F86-6F0D09C0A124")
                        );

                    case "2025":
                        return (
                            new Guid("0DCC1B6E-C9D8-4D46-A4C8-1B85D682A125"),
                            new Guid("D3B53D6A-A6D4-44E8-9F10-56B8D822A125")
                        );

                    case "2026":
                        return (
                            new Guid("3A4F4A1B-D9A2-4E85-89A5-4A88D4C5A126"),
                            new Guid("47D0F736-1E5C-460E-8B1D-7E7A9F22A126")
                        );
                }
            }

            if (hostProduct == "Civil3D")
            {
                switch (hostYear)
                {
                    case "2023":
                        return (
                            new Guid("6B5E58B7-EA7E-4D9F-97F5-9D7A0D3E1123"),
                            new Guid("AA9A7177-43F6-4B5E-96C0-4E1123AA0001")
                        );

                    case "2024":
                        return (
                            new Guid("B1230F6D-FA65-4885-8D30-D74C51241234"),
                            new Guid("AA9A7177-43F6-4B5E-96C0-4E1123AA0002")
                        );

                    case "2025":
                        return (
                            new Guid("C4A6C8B5-0A35-4C61-9CE5-7716A2C51235"),
                            new Guid("AA9A7177-43F6-4B5E-96C0-4E1123AA0003")
                        );

                    case "2026":
                        return (
                            new Guid("D1F08A3B-1E4E-4B1A-8DA3-8E9A7DFA1236"),
                            new Guid("AA9A7177-43F6-4B5E-96C0-4E1123AA0004")
                        );
                }
            }

            throw new NotSupportedException("Unsupported host/version: " + hostProduct + " " + hostYear);

        }
    }
}
