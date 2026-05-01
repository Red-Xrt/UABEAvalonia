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

            // Register Services
            services.AddSingleton<UABEAvalonia.Services.IDialogService, UABEAvalonia.Services.AvaloniaDialogService>();
            services.AddSingleton<UABEAvalonia.Services.ICompressionService, UABEAvalonia.Infrastructure.Compression.CompressionService>();
            services.AddSingleton<UABEAvalonia.Services.IBundleService, UABEAvalonia.Services.BundleService>();
            services.AddSingleton<UABEAvalonia.Services.IWindowService, UABEAvalonia.Services.AvaloniaWindowService>();

            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InfoWindowViewModel>();

            Provider = services.BuildServiceProvider();
        }
    }
}
