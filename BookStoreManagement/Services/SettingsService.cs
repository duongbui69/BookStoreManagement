using System.Collections.Generic;
using System.Threading.Tasks;
using BookStoreManagement.Repositories;

namespace BookStoreManagement.Services
{
    public class SettingsService : ServiceBase
    {
        private readonly SettingsRepository _repo;

        public SettingsService()
        {
            _repo = new SettingsRepository();
        }

        public Dictionary<string, string> GetAllSettings()
        {
            return _repo.GetAllSettings();
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            return await _repo.GetAllSettingsAsync();
        }

        public void SaveSetting(string key, string value)
        {
            _repo.SaveSetting(key, value);
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            await _repo.SaveSettingAsync(key, value);
        }
        
        public async Task SaveMultipleSettingsAsync(Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                await _repo.SaveSettingAsync(kvp.Key, kvp.Value);
            }
        }
    }
}
