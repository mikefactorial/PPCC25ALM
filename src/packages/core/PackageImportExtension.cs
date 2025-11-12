using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Newtonsoft.Json;
using PPCC.ALM.Packages.Core.Models;
using PPCC.ALM.Packages.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PPCC.ALM.Packages.Core
{
    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageImportExtension : ImportExtension
    {
        #region Metadata

        /// <summary>
        /// Folder name where package assets are located in the final output package zip.
        /// </summary>
        public override string GetImportPackageDataFolderName => "PkgAssets";

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        public override string GetNameOfImport(bool plural) => "PPCC.ALM.Packages.Core";

        /// <summary>
        /// Long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport => "PPCC.ALM.Packages.Core";

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText => "PPCC.ALM.Packages.Core";

        #endregion

        private EnvironmentVariableService environmentVariableService;
        private ConnectionReferenceService connectionReferenceService;
        private ManagedIdentityConfigService managedIdentityConfigService;
        private EnvironmentSettingsService environmentSettingsService;
        private WorkflowService workflowService;
        private List<SolutionConfig> targetSolutions;

        private IDictionary<string, string> ConnectionReferences => GetSettings(Constants.ConnectionReferencePrefix);
        private IDictionary<string, string> EnvironmentVariables => GetSettings(Constants.DataverseEnvironmentVariablePrefix);
        private IDictionary<string, string> EnvironmentSettings => GetSettings(Constants.EnvironmentSettingsPrefix);


        protected EnvironmentVariableService EnvironmentVariableService
        {
            get
            {
                if (environmentVariableService == null)
                {
                    environmentVariableService = new EnvironmentVariableService(ServiceClient, PackageLog);
                }
                return environmentVariableService;
            }
        }

        protected ConnectionReferenceService ConnectionReferenceService
        {
            get
            {
                if (connectionReferenceService == null)
                {
                    connectionReferenceService = new ConnectionReferenceService(ServiceClient, PackageLog);
                }
                return connectionReferenceService;
            }
        }

        protected WorkflowService WorkflowService
        {
            get
            {
                if (workflowService == null)
                {
                    workflowService = new WorkflowService(ServiceClient, PackageLog);
                }
                return workflowService;
            }
        }

        protected ManagedIdentityConfigService ManagedIdentityService
        {
            get
            {
                if (managedIdentityConfigService == null)
                {
                    managedIdentityConfigService = new ManagedIdentityConfigService(PackageLog);

                }
                return managedIdentityConfigService;
            }
        }

        protected EnvironmentSettingsService EnvironmentSettingService
        {
            get
            {
                if (environmentSettingsService == null)
                {
                    environmentSettingsService = new EnvironmentSettingsService(ServiceClient, PackageLog);
                }
                return environmentSettingsService;
            }
        }

        public List<SolutionConfig> TargetSolutions
        {
            get
            {
                if (targetSolutions == null)
                {
                    targetSolutions = new List<SolutionConfig>();

                    IEnumerable<string> zipFiles = Directory.GetFiles(PackageAssetsPath).Where(x => x.EndsWith(".zip"));

                    zipFiles.ToList().ForEach(
                        x =>
                        {
                            using (ZipArchive zipArchive = new ZipArchive(new MemoryStream(File.ReadAllBytes(x))))
                            {
                                ZipArchiveEntry solutionXmlFile = zipArchive.Entries.FirstOrDefault(y => y.Name == Constants.SolutionXmlFile);
                                if (solutionXmlFile != null && solutionXmlFile != default(ZipArchiveEntry))
                                {

                                    XDocument solutionXml = XDocument.Load(solutionXmlFile.Open());
                                    string solutionUniqueName = solutionXml.Root.Descendants(Constants.SolutionUniqueNameElement).FirstOrDefault()?.Value;
                                    string solutionVersion = solutionXml.Root.Descendants(Constants.SolutionVersionElement).FirstOrDefault()?.Value;

                                    // x is already a full path from Directory.GetFiles, no need to concatenate with PackageAssetsPath
                                    targetSolutions.Add(new SolutionConfig() { ZipPath = x, UniqueName = solutionUniqueName, Version = solutionVersion });
                                }

                            }
                        });           
                }

                return targetSolutions;
            }
        }

        private string PackageAssetsPath => Path.Combine(CurrentPackageLocation, GetImportPackageDataFolderName);

        /// <summary>
        /// Called to Initialize any functions in the Custom Extension.
        /// </summary>
        /// <see cref="ImportExtension.InitializeCustomExtension"/>
        public override void InitializeCustomExtension()
        {
            PackageLog.Log("[InitializeCustomExtension] running");

            // Apply Managed Identity Configurations (must run before import)
            PackageLog.Log("Applying Managed Identity Configurations");
            ApplyManagedIdentityConfigurations();

            // Apply Environment Settings
            PackageLog.Log("Applying Environment Settings");
            EnvironmentSettingService.UpdateOrganizationSettings(EnvironmentSettings);
        }

        /// <summary>
        /// Called before the Main Import process begins, after solutions and data.
        /// </summary>
        /// <see cref="ImportExtension.BeforeImportStage"/>
        /// <returns></returns>
        public override bool BeforeImportStage()
        {
            PackageLog.Log("[BeforeImportStage] running");
            return true;
        }

        /// <summary>
        /// Raised before the named solution is imported to allow for any configuration settings to be made to the import process
        /// </summary>
        /// <see cref="ImportExtension.PreSolutionImport"/>
        /// <param name="solutionName">name of the solution about to be imported</param>
        /// <param name="solutionOverwriteUnmanagedCustomizations">Value of this field from the solution configuration entry</param>
        /// <param name="solutionPublishWorkflowsAndActivatePlugins">Value of this field from the solution configuration entry</param>
        /// <param name="overwriteUnmanagedCustomizations">If set to true, imports the Solution with Override Customizations enabled</param>
        /// <param name="publishWorkflowsAndActivatePlugins">If set to true, attempts to auto publish workflows and activities as part of solution deployment</param>
        public override void PreSolutionImport(string solutionName, bool solutionOverwriteUnmanagedCustomizations, bool solutionPublishWorkflowsAndActivatePlugins, out bool overwriteUnmanagedCustomizations, out bool publishWorkflowsAndActivatePlugins)
        {
            PackageLog.Log($"[PreSolutionImport] running for solution: {solutionName}");
            base.PreSolutionImport(solutionName, solutionOverwriteUnmanagedCustomizations, solutionPublishWorkflowsAndActivatePlugins, out overwriteUnmanagedCustomizations, out publishWorkflowsAndActivatePlugins);
        }

        /// <summary>
        /// Called during a solution upgrade when both solutions, old and new, are present in the system.
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring.
        /// </summary>
        /// <see cref="ImportExtension.RunSolutionUpgradeMigrationStep"/>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            PackageLog.Log($"[RunSolutionUpgradeMigrationStep] running for solution: {solutionName} from version {oldVersion} to {newVersion}");
            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }

        /// <summary>
        /// Called After all Import steps are complete, allowing for final customizations or tweaking of the instance.
        /// </summary>
        /// <see cref="ImportExtension.AfterPrimaryImport"/>
        /// <returns></returns>
        public override bool AfterPrimaryImport()
        {
            PackageLog.Log("[AfterPrimaryImport] running");

            // Set environment variables
            foreach (var envVar in EnvironmentVariables)
            {
                PackageLog.Log($"Setting environment variable: {envVar.Key} to {envVar.Value}");
                EnvironmentVariableService.SetEnvironmentVariable(envVar.Key, envVar.Value);
            }

            // Set connection references
            PackageLog.Log($"Setting connection references");
            ConnectionReferenceService.SetConnectionReferences(ConnectionReferences);

            PackageLog.Log($"Calling GetWorkflowStatesBySolution - PackageAssetsPath: {PackageAssetsPath}");
            var workflowStates = WorkflowService.GetWorkflowStatesFromSolutions(PackageAssetsPath);

            if (workflowStates != null && workflowStates.Count > 0)
            {
                PackageLog.Log($"Workflows found: {workflowStates.Count}");
                // Process workflow status changes using batching
                WorkflowService.ProcessWorkflowStates(workflowStates);
            }
            else
            {
                PackageLog.Log("No workflows found to process.");
            }


            return true;
        }

        /// <summary>
        /// Applies managed identity configurations by deserializing base64-encoded JSON settings and updating solution files.
        /// Expected settings format: {solutionname}_managedidentities={base64_json_array}
        /// JSON structure: [{"name":"...", "applicationId":"...", "tenantId":"...", "solutionName":"..."}]
        /// </summary>
        private void ApplyManagedIdentityConfigurations()
        {
            PackageLog.Log("Looking for managed identity configurations...");

            // Get all settings that end with "_managedidentities"
            var managedIdentitySettings = GetAllManagedIdentitySettings();

            if (managedIdentitySettings == null || managedIdentitySettings.Count == 0)
            {
                PackageLog.Log("No managed identity configurations to apply.");
                return;
            }

            PackageLog.Log($"Found {managedIdentitySettings.Count} managed identity configuration setting(s)");

            int processedCount = 0;
            int skippedCount = 0;

            foreach (var setting in managedIdentitySettings)
            {
                try
                {
                    string solutionName = setting.Key;
                    string base64Json = setting.Value;

                    PackageLog.Log($"Processing managed identities for solution: {solutionName}");

                    // Decode base64 to JSON
                    byte[] bytes = Convert.FromBase64String(base64Json);
                    string json = Encoding.UTF8.GetString(bytes);

                    PackageLog.Log($"Decoded JSON: {json}");

                    // Deserialize JSON array to list of ManagedIdentityConfig
                    var managedIdentities = JsonConvert.DeserializeObject<List<ManagedIdentityConfig>>(json);

                    if (managedIdentities == null || managedIdentities.Count == 0)
                    {
                        PackageLog.Log($"No managed identities found in configuration for solution: {solutionName}");
                        continue;
                    }

                    PackageLog.Log($"Found {managedIdentities.Count} managed identity/identities for solution: {solutionName}");

                    // Process each managed identity
                    foreach (var config in managedIdentities)
                    {
                        try
                        {
                            // Validate the configuration has all required fields
                            if (string.IsNullOrWhiteSpace(config.Name))
                            {
                                PackageLog.Log($"Warning: Missing Name in managed identity config. Skipping.");
                                skippedCount++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(config.ApplicationId))
                            {
                                PackageLog.Log($"Warning: Missing ApplicationId for managed identity '{config.Name}'. Skipping.");
                                skippedCount++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(config.TenantId))
                            {
                                PackageLog.Log($"Warning: Missing TenantId for managed identity '{config.Name}'. Skipping.");
                                skippedCount++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(config.SolutionName))
                            {
                                PackageLog.Log($"Warning: Missing SolutionName for managed identity '{config.Name}'. Skipping.");
                                skippedCount++;
                                continue;
                            }

                            // Find the solution in the target solutions list
                            var solution = TargetSolutions.FirstOrDefault(s =>
                                s.UniqueName.Equals(config.SolutionName, StringComparison.OrdinalIgnoreCase));

                            if (solution == null)
                            {
                                PackageLog.Log($"Warning: Solution '{config.SolutionName}' not found in package for managed identity '{config.Name}'. Available solutions: {string.Join(", ", TargetSolutions.Select(s => s.UniqueName))}");
                                skippedCount++;
                                continue;
                            }

                            PackageLog.Log($"Updating managed identity '{config.Name}' in solution '{solution.UniqueName}' at path: {solution.ZipPath}");

                            // Update the managed identity in the solution using the service
                            ManagedIdentityService.UpdateManagedIdentity(solution.ZipPath, config.Name, config.ApplicationId, config.TenantId);

                            processedCount++;
                            PackageLog.Log($"Successfully updated managed identity '{config.Name}'");
                        }
                        catch (Exception ex)
                        {
                            PackageLog.Log($"Error applying managed identity configuration for '{config.Name}': {ex.Message}");
                            PackageLog.Log($"Stack trace: {ex.StackTrace}");
                            skippedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    PackageLog.Log($"Error processing managed identity setting '{setting.Key}': {ex.Message}");
                    PackageLog.Log($"Stack trace: {ex.StackTrace}");
                    skippedCount++;
                }
            }

            PackageLog.Log($"Managed identity configuration completed. Processed: {processedCount}, Skipped: {skippedCount}");
        }

        /// <summary>
        /// Gets all managed identity settings from runtime settings.
        /// Settings are expected to be in the format: {solutionname}_managedidentities={base64_json}
        /// </summary>
        /// <returns>Dictionary where key is solution name and value is base64 encoded JSON</returns>
        private IDictionary<string, string> GetAllManagedIdentitySettings()
        {
            var managedIdentitySettings = new Dictionary<string, string>();

            if (this.RuntimeSettings == null)
            {
                return managedIdentitySettings;
            }

            // Look for settings ending with "_managedidentities"
            const string suffix = "_managedidentities";

            foreach (var setting in this.RuntimeSettings)
            {
                if (setting.Key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    // Extract solution name from the key
                    string solutionName = setting.Key.Substring(0, setting.Key.Length - suffix.Length);
                    managedIdentitySettings[solutionName] = setting.Value.ToString();

                    PackageLog.Log($"Found managed identity setting for solution: {solutionName}");
                }
            }

            return managedIdentitySettings;
        }

        private IDictionary<string, string> GetSettings(string prefix)
        {
            this.PackageLog.Log($"Getting {prefix} settings");

            var environmentVariables = Environment.GetEnvironmentVariables();
            var mappings = environmentVariables.Keys
                .Cast<string>()
                .Where(k => k.StartsWith($"{Constants.EnvironmentVariablePrefix}{prefix}_", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    k => k.Remove(0, Constants.EnvironmentVariablePrefix.Length + prefix.Length + 1).ToLower(),
                    v => environmentVariables[v].ToString());

            this.PackageLog.Log($"{mappings.Count} matching settings found in environment variables");

            if (this.RuntimeSettings == null)
            {
                return mappings;
            }

            var runtimeSettingMappings = this.RuntimeSettings
                .Where(s => s.Key.StartsWith($"{prefix}:"))
                .ToDictionary(kvp => kvp.Key.Remove(0, prefix.Length + 1).ToLower(), kvp => kvp.Value.ToString());

            this.PackageLog.Log($"{mappings.Count} matching settings found in runtime settings");

            foreach (var runtimeSettingsMapping in runtimeSettingMappings)
            {
                if (mappings.ContainsKey(runtimeSettingsMapping.Key))
                {
                    this.PackageLog.Log($"Overriding environment variable setting with runtime setting for {runtimeSettingsMapping.Key}.");
                }

                mappings[runtimeSettingsMapping.Key] = runtimeSettingsMapping.Value;
            }

            return mappings;
        }

    }
}
