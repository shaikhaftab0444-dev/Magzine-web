namespace Digital_Magzine.Services
{
    public interface ISettingsService
    {
        string GetValue(string key, string defaultValue = "");
        Task<string> GetValueAsync(string key, string defaultValue = "");
        string GetCurrencySymbol();
        Task SetValueAsync(string key, string value);
        void RefreshCache();
    }
}
