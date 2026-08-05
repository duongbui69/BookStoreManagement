using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStoreManagement.Repositories
{
    public class SettingsRepository : RepositoryBase
    {
        public Dictionary<string, string> GetAllSettings()
        {
            var settings = new Dictionary<string, string>();
            ExecuteQuery(cmd =>
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    settings[reader.GetString(0)] = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                }
                return true;
            }, "SELECT SettingKey, SettingValue FROM SystemSettings");
            return settings;
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            var settings = new Dictionary<string, string>();
            var results = await QueryAsync<dynamic>("SELECT SettingKey, SettingValue FROM SystemSettings");
            foreach(var row in results)
            {
                settings[(string)row.SettingKey] = (row.SettingValue == null) ? string.Empty : (string)row.SettingValue;
            }
            return settings;
        }

        public void SaveSetting(string key, string value)
        {
            ExecuteNonQuery(
                "UPDATE SystemSettings SET SettingValue = @Value WHERE SettingKey = @Key",
                p => {
                    AddParameter(p, "@Value", value);
                    AddParameter(p, "@Key", key);
                });
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            await ExecuteAsync(
                "UPDATE SystemSettings SET SettingValue = @Value WHERE SettingKey = @Key",
                new { Value = value, Key = key });
        }
    }
}
