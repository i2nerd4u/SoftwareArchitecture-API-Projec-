using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace GameReviews.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped(sp =>
            {
                var handler = new HttpClientHandler();

                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;

                return new HttpClient(handler);
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var culture = new CultureInfo("ga-IE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            return builder.Build();
        }
    }
}