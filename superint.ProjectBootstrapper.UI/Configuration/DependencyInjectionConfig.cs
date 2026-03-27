using Microsoft.Extensions.DependencyInjection;
using superint.ProjectBootstrapper.Application.Services;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.Infrastructure.Interfaces;
using superint.ProjectBootstrapper.Infrastructure.Services;
using superint.ProjectBootstrapper.UI.ViewModels;

namespace superint.ProjectBootstrapper.UI.Configuration;

/// <summary>
/// Configuracao de injecao de dependencia para a aplicacao UI.
/// </summary>
public static class DependencyInjectionConfig
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, ApplicationSettings appSettings)
    {
        // Settings
        services.AddSingleton(appSettings);
        services.AddSingleton(appSettings.Git);
        services.AddSingleton(appSettings.ContainerRegistry);
        services.AddSingleton(appSettings.Jenkins);
        services.AddSingleton(appSettings.Servers);
        services.AddSingleton(appSettings.DatabaseContainer);
        services.AddSingleton(appSettings.Deployment);

        // Git Service - selecao dinamica baseada em Provider
        services.AddSingleton<IGitService>(serviceProvider =>
        {
            var gitSettings = serviceProvider.GetRequiredService<GitSettings>();
            return gitSettings.Provider?.ToLowerInvariant() switch
            {
                "github" => new GitHubService(gitSettings),
                _ => new GitHubService(gitSettings) // Default para GitHub
            };
        });

        // Container Registry Service - selecao dinamica baseada em Provider
        services.AddSingleton<IContainerRegistryService>(serviceProvider =>
        {
            var registrySettings = serviceProvider.GetRequiredService<ContainerRegistrySettings>();
            return registrySettings.Provider?.ToLowerInvariant() switch
            {
                "harbor" => new HarborService(registrySettings),
                _ => new HarborService(registrySettings) // Default para Harbor
            };
        });

        // Jenkins Service
        services.AddSingleton<IJenkinsService>(serviceProvider =>
            new JenkinsService(serviceProvider.GetRequiredService<JenkinsSettings>()));

        // SSH Services - keyed por ambiente
        services.AddKeyedSingleton<ISshService>("STG", (serviceProvider, key) =>
        {
            var servers = serviceProvider.GetRequiredService<ServersSettings>();
            return new SshService(servers.Stg, "STG");
        });
        services.AddKeyedSingleton<ISshService>("PRD", (serviceProvider, key) =>
        {
            var servers = serviceProvider.GetRequiredService<ServersSettings>();
            return new SshService(servers.Prd, "PRD");
        });

        // Database Services - usando SSH + docker exec
        services.AddKeyedSingleton<IDatabaseService>("STG", (serviceProvider, key) =>
        {
            var sshService = serviceProvider.GetRequiredKeyedService<ISshService>("STG");
            var containerSettings = serviceProvider.GetRequiredService<DatabaseContainerSettings>();
            return new PostgresDatabaseService(sshService, containerSettings.Stg, "STG");
        });
        services.AddKeyedSingleton<IDatabaseService>("PRD", (serviceProvider, key) =>
        {
            var sshService = serviceProvider.GetRequiredKeyedService<ISshService>("PRD");
            var containerSettings = serviceProvider.GetRequiredService<DatabaseContainerSettings>();
            return new PostgresDatabaseService(sshService, containerSettings.Prd, "PRD");
        });

        // Docker Compose Service
        services.AddSingleton<IDockerComposeService>(serviceProvider =>
        {
            var deployment = serviceProvider.GetRequiredService<DeploymentSettings>();
            var registry = serviceProvider.GetRequiredService<ContainerRegistrySettings>();
            return new DockerComposeService(deployment, registry);
        });

        // Jenkinsfile Service
        services.AddSingleton<IJenkinsfileService, JenkinsfileService>();

        // Deployment Services - keyed por ambiente
        services.AddKeyedSingleton<IDeploymentService>("STG", (serviceProvider, key) =>
        {
            var sshService = serviceProvider.GetRequiredKeyedService<ISshService>("STG");
            var dockerComposeService = serviceProvider.GetRequiredService<IDockerComposeService>();
            var deployment = serviceProvider.GetRequiredService<DeploymentSettings>();
            return new DeploymentService(sshService, dockerComposeService, deployment, "STG");
        });
        services.AddKeyedSingleton<IDeploymentService>("PRD", (serviceProvider, key) =>
        {
            var sshService = serviceProvider.GetRequiredKeyedService<ISshService>("PRD");
            var dockerComposeService = serviceProvider.GetRequiredService<IDockerComposeService>();
            var deployment = serviceProvider.GetRequiredService<DeploymentSettings>();
            return new DeploymentService(sshService, dockerComposeService, deployment, "PRD");
        });

        // Application Services
        services.AddSingleton<ProjectBootstrapService>(serviceProvider => new ProjectBootstrapService(
            appSettings,
            serviceProvider.GetRequiredService<IGitService>(),
            serviceProvider.GetRequiredService<IContainerRegistryService>(),
            serviceProvider.GetRequiredService<IJenkinsService>(),
            serviceProvider.GetRequiredKeyedService<IDatabaseService>("STG"),
            serviceProvider.GetRequiredKeyedService<IDatabaseService>("PRD"),
            serviceProvider.GetRequiredService<IDockerComposeService>(),
            serviceProvider.GetRequiredKeyedService<ISshService>("STG"),
            serviceProvider.GetRequiredKeyedService<ISshService>("PRD"),
            serviceProvider.GetRequiredKeyedService<IDeploymentService>("STG"),
            serviceProvider.GetRequiredKeyedService<IDeploymentService>("PRD")
        ));

        // ViewModels
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SecretsViewModel>(sp =>
            new SecretsViewModel(sp.GetRequiredService<ApplicationSettings>()));

        services.AddSingleton<TestConnectionsViewModel>(sp =>
            new TestConnectionsViewModel(sp.GetRequiredService<ProjectBootstrapService>()));

        services.AddSingleton<BootstrapWizardViewModel>(sp =>
            new BootstrapWizardViewModel(
                sp.GetRequiredService<ProjectBootstrapService>(),
                sp.GetRequiredService<ApplicationSettings>()));

        services.AddTransient<MainViewModel>();

        return services;
    }
}
