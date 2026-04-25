using Microsoft.Extensions.DependencyInjection;
using System;
using UABEAvalonia.ViewModels;

namespace UABEAvalonia
{
    public static class AppServices
    {
        public static IServiceProvider Provider { get; private set; }

        public static void Initialize()
        {
            var services = new ServiceCollection();

            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InfoWindowViewModel>();

            Provider = services.BuildServiceProvider();
        }
    }
}
