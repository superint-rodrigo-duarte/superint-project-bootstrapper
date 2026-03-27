using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.UI.Configuration;
using superint.ProjectBootstrapper.UI.ViewModels;
using superint.ProjectBootstrapper.UI.Views;

namespace superint.ProjectBootstrapper.UI;

public partial class App : Avalonia.Application
{
    public static IServiceProvider? Services { get; private set; }
    public static ApplicationSettings? AppSettings { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Disable Avalonia data annotation validation (we use CommunityToolkit)
            DisableAvaloniaDataAnnotationValidation();

            // Load configuration
            AppSettings = LoadAppSettings();

            // Configure services
            var services = new ServiceCollection();
            services.ConfigureServices(AppSettings);
            Services = services.BuildServiceProvider();

            // Create main window
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ApplicationSettings LoadAppSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("PROJINIT_")
            .Build();

        var appSettings = new ApplicationSettings();
        configuration.Bind(appSettings);

        // Garantir valores padrao se nao configurados
        appSettings.Git ??= new GitSettings();
        appSettings.Jenkins ??= new JenkinsSettings();
        appSettings.ContainerRegistry ??= new ContainerRegistrySettings();
        appSettings.ContainerRegistry.RetentionPolicy ??= new ContainerRegistryRetentionPolicySettings();
        appSettings.Servers ??= new ServersSettings();
        appSettings.Servers.Stg ??= new ServerSettings();
        appSettings.Servers.Prd ??= new ServerSettings();
        appSettings.Deployment ??= new DeploymentSettings();
        appSettings.DatabaseContainer ??= new DatabaseContainerSettings();
        appSettings.DatabaseContainer.Stg ??= new DatabaseContainerEnvironmentSettings();
        appSettings.DatabaseContainer.Prd ??= new DatabaseContainerEnvironmentSettings();

        return appSettings;
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
