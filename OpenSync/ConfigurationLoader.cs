using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSync
{
    internal class ConfigurationLoader
    {
        public static string GetConfigFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        }

        public static string GetTrackingAppsFilePath()
        {
            string configFilePath = GetConfigFilePath();

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"The configuration file was not found: {configFilePath}");
            }

            string jsonContent = File.ReadAllText(configFilePath);
            dynamic config = JsonConvert.DeserializeObject(jsonContent);

            if (config == null || config.TrackingAppsFilePath == null)
            {
                throw new InvalidOperationException("The configuration file is empty or not formatted correctly.");
            }

            string trackingAppsFilePath = config.TrackingAppsFilePath.ToString();
            string resolvedFilePath = Environment.ExpandEnvironmentVariables(trackingAppsFilePath);
            return resolvedFilePath;

        }

        public static void UpdateTrackingAppsFilePath(string newFilePath)
        {
            string configFilePath = GetConfigFilePath();

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"The configuration file was not found: {configFilePath}");
            }

            string jsonContent = File.ReadAllText(configFilePath);
            dynamic config = JsonConvert.DeserializeObject(jsonContent);

            if (config == null)
            {
                config = new JObject();
            }

            config.TrackingAppsFilePath = newFilePath;
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}
