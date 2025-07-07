using System.Configuration;

namespace Hermes.Parking.Server.DataService
{
    public static class Configuration
    {
        private static ConfigSection cfg = (ConfigSection)ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections["DataService"];

        public static string ConnectionString
        {
            get
            {
                return cfg.ConnectionString.Value;
            }
        }
    }

    public class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("ConnectionString")]
        public ConnectionStringConfigElement ConnectionString
        {
            get { return (ConnectionStringConfigElement)base["ConnectionString"]; }
        }

        public class ConnectionStringConfigElement : ConfigurationElement
        {
            [ConfigurationProperty("value", DefaultValue = "", IsKey = false, IsRequired = true)]
            public string Value
            {
                get { return ((string)(base["value"])); }
            }
        }
    }
}
