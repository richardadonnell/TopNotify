using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopNotify.Common
{
    public enum AppDiscoveryMethod
    {
        MatchAlways, // Always Active
        MatchMSIX, // Checks If An MSIX Package Exists That Contains The Specified Search Term
        MatchCustomPath // Checks If A Specified Path Exists
    }

    [Serializable]
    public class AppDiscovery
    {
        /// <summary>
        /// Determines How TopNotify Will Check If The App Is Installed
        /// </summary>
        public AppDiscoveryMethod Method = AppDiscoveryMethod.MatchAlways;

        /// <summary>
        /// How This Search Term Is Used Depends On The App Discovery Method
        /// </summary>
        public required string SearchTerm;

        /// <summary>
        /// Cached Output Of Get-AppxPackage, Filtered To Only Include "Name: " Lines
        /// </summary>
        public static string[] AppxPackageLines
        {
            get
            {
                if (_AppxPackageLines == null)
                {
                    _AppxPackageLines = Util.SimpleCMD("powershell -c \"Get-AppxPackage | Select Name\"").Split("\n");
                }

                return _AppxPackageLines;
            }
        }

        private static string[]? _AppxPackageLines = null;

        /// <summary>
        /// Checks If An App Is Installed Based On Multiple Discovery Parameters
        /// </summary>
        public static bool IsAppInstalled(AppDiscovery[] discoveries)
        {
            return discoveries.Any(IsAppInstalled);
        }

        /// <summary>
        /// Checks If An App Is Installed Based On Its Discovery Parameters
        /// </summary>
        public static bool IsAppInstalled(AppDiscovery discovery)
        {
            if (discovery == null) { return false; }

            if (discovery.Method == AppDiscoveryMethod.MatchAlways) { return true; }
            else if (discovery.Method == AppDiscoveryMethod.MatchMSIX) 
            {
                return AppxPackageLines.Any(line => line.Contains(discovery.SearchTerm));
            }
            else if (discovery.Method == AppDiscoveryMethod.MatchCustomPath)
            {
                // The Search Term May Contain Environment Variables
                var pathWithEnvVariables = Environment.ExpandEnvironmentVariables(discovery.SearchTerm);
                return Directory.Exists(pathWithEnvVariables) || File.Exists(pathWithEnvVariables);
            }

            return false;
        }
    }
}
