using System.Configuration;

namespace Configuration
{
    public static class Naming
    {
        public static string CurrentEnvironment
        {
            get { return ConfigurationManager.AppSettings["StatsdEnvironment"]; }
        }

        public static string CurrentApplication
        {
            get { return ConfigurationManager.AppSettings["StatsdApplication"]; }
        }

        public static string CurrentHostname
        {
            get { return System.Environment.MachineName; }
        }

        public static string withEnvironmentAndApplication(string statName)
        {
            return string.Format("{0}.{1}.{2}", CurrentEnvironment, CurrentApplication, statName); 
        }

        public static string withEnvironmentApplicationAndHostname(string statName)
        {
            return string.Format("{0}.{1}.{2}.{3}", CurrentEnvironment, CurrentApplication, statName, CurrentHostname);
        }
    }
}
