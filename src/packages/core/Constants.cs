using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPCC.ALM.Packages.Core
{
    public class Constants
    {
        public const string EnvironmentVariablePrefix = "PD_";
        public const string DataverseEnvironmentVariablePrefix = "ENVVAR";
        public const string ConnectionReferencePrefix = "CONNREF";
        public const string ManagedIdentityConfigPrefix = "MANAGEDIDENTITY";
        public const string EnvironmentSettingsPrefix = "ENVSETTING";
        
        // Solution XML
        public const string SolutionXmlFile = "solution.xml";
        public const string SolutionUniqueNameElement = "UniqueName";
        public const string SolutionVersionElement = "Version";

        // Customizations XML
        public const string CustomizationsXmlFile = "customizations.xml";

    }
}
