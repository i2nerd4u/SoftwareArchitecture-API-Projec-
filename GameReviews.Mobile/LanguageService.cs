using System.Globalization;

namespace GameReviews.Mobile
{
    public class LanguageService
    {
        public string CurrentLanguage => CultureInfo.CurrentUICulture.Name;

        public void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}