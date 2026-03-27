using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superint.ProjectBootstrapper.Application.Services;
using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.DTO.Configuration;
using superint.ProjectBootstrapper.UI.Models;
using superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

namespace superint.ProjectBootstrapper.UI.ViewModels;

public partial class BootstrapWizardViewModel : ViewModelBase
{
    private static readonly WizardStep[] StepOrder = Enum.GetValues<WizardStep>();
    private Dictionary<WizardStep, ViewModelBase> _stepViewModels;
    private readonly ProjectBootstrapService? _bootstrapService;
    private readonly ApplicationSettings? _appSettings;

    [ObservableProperty]
    private WizardStep _currentStepType = WizardStep.BasicInfo;

    [ObservableProperty]
    private int _displayStepNumber = 1;

    [ObservableProperty]
    private string _currentStepTitle = "Basic Info";

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _canGoNext = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowExecutionSummary))]
    private bool _isLastStep;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private double _executionProgress;

    [ObservableProperty]
    private string _executionStatus = string.Empty;

    [ObservableProperty]
    private ProjectConfiguration _configuration = new();

    [ObservableProperty]
    private ViewModelBase? _currentStep;

    [ObservableProperty]
    private ObservableCollection<BootstrapTaskResult> _executionResults = [];

    // Phase 6: Confirmation overlay
    [ObservableProperty]
    private bool _showConfirmation;

    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = [];

    [ObservableProperty]
    private ObservableCollection<ConnectionTestResult> _connectionTestResults = [];

    [ObservableProperty]
    private bool _isTestingConnections;

    [ObservableProperty]
    private bool _hasConnectionFailures;

    [ObservableProperty]
    private bool _hasValidationErrors;

    // Phase 7: Execution summary
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowExecutionSummary))]
    private bool _isExecutionComplete;

    [ObservableProperty]
    private int _successCount;

    [ObservableProperty]
    private int _failureCount;

    [ObservableProperty]
    private int _skippedCount;

    [ObservableProperty]
    private bool _hasFailures;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    public bool ShowExecutionSummary => IsExecutionComplete && IsLastStep;

    public ObservableCollection<WizardStepInfo> Steps { get; } = [];

    public int TotalSteps => StepOrder.Length;
    public int CurrentStepIndex => (int)CurrentStepType;

    public BootstrapWizardViewModel(
        ProjectBootstrapService bootstrapService,
        ApplicationSettings appSettings)
    {
        _bootstrapService = bootstrapService;
        _appSettings = appSettings;

        // Initialize Configuration with defaults from AppSettings
        Configuration = CreateConfigurationWithDefaults(appSettings);

        _stepViewModels = CreateStepViewModels();
        InitializeSteps();

        CurrentStepType = WizardStep.BasicInfo;
        UpdateCurrentStep();
        UpdateNavigationState();
    }

    // Design-time constructor
    public BootstrapWizardViewModel() : this(new ProjectConfiguration())
    {
    }

    // Constructor for testing/design with configuration
    public BootstrapWizardViewModel(ProjectConfiguration configuration)
    {
        Configuration = configuration;
        _stepViewModels = CreateStepViewModels();
        InitializeSteps();

        CurrentStepType = WizardStep.BasicInfo;
        UpdateCurrentStep();
        UpdateNavigationState();
    }

    private static ProjectConfiguration CreateConfigurationWithDefaults(ApplicationSettings appSettings)
    {
        return new ProjectConfiguration
        {
            // Git defaults
            GitNamespace = appSettings.Git.DefaultNamespace,

            // Jenkins defaults from AppSettings
            JenkinsGitCredentialsId = appSettings.Jenkins.CredentialsId,
            JenkinsFolder = appSettings.Jenkins.DefaultFolder,

            // Deployment/ReverseProxy defaults
            ReverseProxyHostStg = appSettings.Deployment.ReverseProxyHostStaging,
            ReverseProxyHostPrd = appSettings.Deployment.ReverseProxyHostProduction
        };
    }

    private Dictionary<WizardStep, ViewModelBase> CreateStepViewModels()
    {
        return new Dictionary<WizardStep, ViewModelBase>
        {
            [WizardStep.BasicInfo] = new WizardBasicInfoStepViewModel(Configuration),
            [WizardStep.Git] = new WizardGitStepViewModel(Configuration),
            [WizardStep.ContainerRegistry] = new WizardContainerRegistryStepViewModel(Configuration),
            [WizardStep.Jenkins] = new WizardJenkinsStepViewModel(Configuration),
            [WizardStep.Database] = new WizardDatabaseStepViewModel(Configuration),
            [WizardStep.Docker] = new WizardDockerStepViewModel(Configuration),
            [WizardStep.Review] = new WizardReviewStepViewModel(Configuration)
        };
    }

    private void InitializeSteps()
    {
        Steps.Add(new WizardStepInfo { StepType = WizardStep.BasicInfo, DisplayNumber = 1, Title = "Basic Info", IsRequired = true });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.Git, DisplayNumber = 2, Title = "Git", IsRequired = false });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.ContainerRegistry, DisplayNumber = 3, Title = "Container Registry", IsRequired = false });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.Jenkins, DisplayNumber = 4, Title = "Jenkins", IsRequired = false });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.Database, DisplayNumber = 5, Title = "Database", IsRequired = false });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.Docker, DisplayNumber = 6, Title = "Docker", IsRequired = false });
        Steps.Add(new WizardStepInfo { StepType = WizardStep.Review, DisplayNumber = 7, Title = "Review", IsRequired = true, IsLast = true });
    }

    [RelayCommand]
    private void GoBack()
    {
        var currentIndex = (int)CurrentStepType;
        if (currentIndex > 0)
        {
            CurrentStepType = StepOrder[currentIndex - 1];
            UpdateCurrentStep();
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private void GoNext()
    {
        var currentIndex = (int)CurrentStepType;

        // Apply defaults when leaving BasicInfo
        if (CurrentStepType == WizardStep.BasicInfo)
        {
            ApplyDefaultsFromBasicInfo();
            RecreateStepViewModels();
        }

        if (currentIndex < TotalSteps - 1)
        {
            CurrentStepType = StepOrder[currentIndex + 1];
            UpdateCurrentStep();
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private void GoToStep(int stepIndex)
    {
        if (stepIndex >= 0 && stepIndex < TotalSteps)
        {
            CurrentStepType = StepOrder[stepIndex];
            UpdateCurrentStep();
            UpdateNavigationState();
        }
    }

    [RelayCommand]
    private async Task RequestExecuteAsync()
    {
        IsExecutionComplete = false;
        ValidationErrors.Clear();
        ConnectionTestResults.Clear();
        HasValidationErrors = false;
        HasConnectionFailures = false;

        // Step 1: Validate AppSettings
        ValidateAppSettings();
        HasValidationErrors = ValidationErrors.Count > 0;

        // Step 2: Test connections
        if (_bootstrapService != null)
        {
            IsTestingConnections = true;
            try
            {
                var results = await _bootstrapService.TestConnectionsAsync();
                foreach (var (service, result) in results)
                {
                    ConnectionTestResults.Add(new ConnectionTestResult
                    {
                        ServiceName = service,
                        ServiceKey = service,
                        Status = result.Success ? ConnectionStatus.Success : ConnectionStatus.Failed,
                        Message = result.Message,
                        Details = result.Details
                    });
                }

                HasConnectionFailures = ConnectionTestResults.Any(r => r.Status == ConnectionStatus.Failed);
            }
            catch (Exception ex)
            {
                ConnectionTestResults.Add(new ConnectionTestResult
                {
                    ServiceName = "Connection Test",
                    ServiceKey = "error",
                    Status = ConnectionStatus.Failed,
                    Message = $"Error: {ex.Message}"
                });
                HasConnectionFailures = true;
            }
            finally
            {
                IsTestingConnections = false;
            }
        }

        // Step 3: Show confirmation overlay
        ShowConfirmation = true;
    }

    [RelayCommand]
    private async Task ConfirmExecuteAsync()
    {
        ShowConfirmation = false;
        await ExecuteAsync();
    }

    [RelayCommand]
    private void CancelConfirmation()
    {
        ShowConfirmation = false;
    }

    private void ValidateAppSettings()
    {
        if (_appSettings == null) return;

        if (Configuration.CreateGitRepository && string.IsNullOrWhiteSpace(_appSettings.Git.ApiToken))
            ValidationErrors.Add("Git: API Token is not configured in appsettings");

        if (Configuration.CreateContainerRegistryProject && string.IsNullOrWhiteSpace(_appSettings.ContainerRegistry.Url))
            ValidationErrors.Add("Container Registry: URL is not configured in appsettings");

        if (Configuration.CreateJenkinsPipeline && string.IsNullOrWhiteSpace(_appSettings.Jenkins.BaseUrl))
            ValidationErrors.Add("Jenkins: Base URL is not configured in appsettings");

        if (Configuration.DeployDockerComposeStg && string.IsNullOrWhiteSpace(_appSettings.Servers.Stg.Host))
            ValidationErrors.Add("Servers: STG host is not configured in appsettings");

        if (Configuration.DeployDockerComposePrd && string.IsNullOrWhiteSpace(_appSettings.Servers.Prd.Host))
            ValidationErrors.Add("Servers: PRD host is not configured in appsettings");

        // Jenkins <- Git: creating credentials requires Git URLs
        var cfg = Configuration;
        var hasBackend = cfg.ProjectType == Shared.Enums.ProjectType.Backend || cfg.ProjectType == Shared.Enums.ProjectType.Fullstack;
        var hasFrontend = cfg.ProjectType == Shared.Enums.ProjectType.Frontend || cfg.ProjectType == Shared.Enums.ProjectType.Fullstack;

        if (cfg.CreateJenkinsPipeline && hasBackend)
        {
            if (cfg.CreateJenkinsGitCredentialBackend && !cfg.CreateGitRepository
                && string.IsNullOrWhiteSpace(cfg.ProjectType == Shared.Enums.ProjectType.Fullstack ? cfg.BackendGitUrl : cfg.GitRepositoryUrl))
                ValidationErrors.Add("Jenkins: Backend Git URL required when creating credential without Git repo");

            if (!cfg.CreateJenkinsGitCredentialBackend && string.IsNullOrWhiteSpace(cfg.JenkinsExistingGitUrlIdBackend))
                ValidationErrors.Add("Jenkins: Backend Git URL credential ID required when skipping creation");
        }

        if (cfg.CreateJenkinsPipeline && hasFrontend)
        {
            if (cfg.CreateJenkinsGitCredentialFrontend && !cfg.CreateGitRepository
                && string.IsNullOrWhiteSpace(cfg.ProjectType == Shared.Enums.ProjectType.Fullstack ? cfg.FrontendGitUrl : cfg.GitRepositoryUrl))
                ValidationErrors.Add("Jenkins: Frontend Git URL required when creating credential without Git repo");

            if (!cfg.CreateJenkinsGitCredentialFrontend && string.IsNullOrWhiteSpace(cfg.JenkinsExistingGitUrlIdFrontend))
                ValidationErrors.Add("Jenkins: Frontend Git URL credential ID required when skipping creation");
        }

        // Docker <- CR: CR paths required when generating Docker Compose
        if (cfg.GenerateDockerCompose)
        {
            if (hasBackend && string.IsNullOrWhiteSpace(cfg.ContainerRegistryPathBackendStg))
                ValidationErrors.Add("Container Registry: Backend STG path required for Docker Compose");
            if (hasBackend && string.IsNullOrWhiteSpace(cfg.ContainerRegistryPathBackendPrd))
                ValidationErrors.Add("Container Registry: Backend PRD path required for Docker Compose");
            if (hasFrontend && string.IsNullOrWhiteSpace(cfg.ContainerRegistryPathFrontendStg))
                ValidationErrors.Add("Container Registry: Frontend STG path required for Docker Compose");
            if (hasFrontend && string.IsNullOrWhiteSpace(cfg.ContainerRegistryPathFrontendPrd))
                ValidationErrors.Add("Container Registry: Frontend PRD path required for Docker Compose");
        }
    }

    private async Task ExecuteAsync()
    {
        if (_bootstrapService == null)
        {
            await SimulateExecutionAsync();
            return;
        }

        IsExecuting = true;
        IsExecutionComplete = false;
        ExecutionProgress = 0;
        ExecutionResults.Clear();

        try
        {
            var tasks = _bootstrapService.BuildTaskList(Configuration);
            var totalTasks = tasks.Count;
            var completedTasks = 0;

            // Criar lista de resultados pendentes
            foreach (var (name, _, enabled) in tasks)
            {
                ExecutionResults.Add(new BootstrapTaskResult
                {
                    TaskName = name,
                    Status = enabled ? BootstrapTaskStatus.Pending : BootstrapTaskStatus.Skipped,
                    IsEnabled = enabled
                });
            }

            // Executar cada tarefa
            for (int i = 0; i < tasks.Count; i++)
            {
                var (name, action, enabled) = tasks[i];
                var resultItem = ExecutionResults[i];

                if (!enabled)
                {
                    resultItem.Status = BootstrapTaskStatus.Skipped;
                    resultItem.Message = "Desabilitado pelo usuario";
                    completedTasks++;
                    ExecutionProgress = (double)completedTasks / totalTasks * 100;
                    continue;
                }

                ExecutionStatus = name;
                resultItem.Status = BootstrapTaskStatus.Running;

                try
                {
                    var result = await action();

                    resultItem.Status = result.Success ? BootstrapTaskStatus.Success : BootstrapTaskStatus.Failed;
                    resultItem.Message = result.Message;
                    resultItem.Details = result.Details;
                }
                catch (Exception ex)
                {
                    resultItem.Status = BootstrapTaskStatus.Failed;
                    resultItem.Message = $"Erro: {ex.Message}";
                }

                completedTasks++;
                ExecutionProgress = (double)completedTasks / totalTasks * 100;
            }

            ExecutionStatus = "Concluido";
        }
        catch (Exception ex)
        {
            ExecutionStatus = $"Erro: {ex.Message}";
        }
        finally
        {
            IsExecuting = false;
            UpdateExecutionSummary();
        }
    }

    private async Task SimulateExecutionAsync()
    {
        IsExecuting = true;
        ExecutionProgress = 0;
        ExecutionResults.Clear();

        var simulatedTasks = new[]
        {
            "Criar Repositorio Git",
            "Criar Projeto Harbor",
            "Criar Pipeline Jenkins",
            "Criar Usuario PostgreSQL",
            "Gerar Docker Compose"
        };

        foreach (var task in simulatedTasks)
        {
            ExecutionResults.Add(new BootstrapTaskResult
            {
                TaskName = task,
                Status = BootstrapTaskStatus.Pending
            });
        }

        for (int i = 0; i < simulatedTasks.Length; i++)
        {
            ExecutionStatus = simulatedTasks[i];
            ExecutionResults[i].Status = BootstrapTaskStatus.Running;

            await Task.Delay(500);

            ExecutionResults[i].Status = BootstrapTaskStatus.Success;
            ExecutionResults[i].Message = "Simulado com sucesso";
            ExecutionProgress = (double)(i + 1) / simulatedTasks.Length * 100;
        }

        ExecutionStatus = "Simulacao concluida";
        IsExecuting = false;
        UpdateExecutionSummary();
    }

    private void UpdateExecutionSummary()
    {
        SuccessCount = ExecutionResults.Count(r => r.Status == BootstrapTaskStatus.Success);
        FailureCount = ExecutionResults.Count(r => r.Status == BootstrapTaskStatus.Failed);
        SkippedCount = ExecutionResults.Count(r => r.Status == BootstrapTaskStatus.Skipped);
        HasFailures = FailureCount > 0;
        SummaryText = $"Completed: {SuccessCount} success, {FailureCount} failures, {SkippedCount} skipped";
        IsExecutionComplete = true;
    }

    private void ApplyDefaultsFromBasicInfo()
    {
        var config = Configuration;

        // Git defaults
        config.CreateGitRepository = true;
        if (_appSettings != null)
            config.GitNamespace = _appSettings.Git.DefaultNamespace;
        if (string.IsNullOrEmpty(config.GitRepositoryName))
            config.GitRepositoryName = config.GetDefaultGitRepoName();

        // Container Registry defaults
        config.CreateContainerRegistryProject = true;
        config.ContainerRegistryProjectName = config.ProjectSlug;
        config.AutoFillContainerRegistryPaths();

        // Jenkins defaults
        config.CreateJenkinsPipeline = true;
        config.CreateJenkinsGitCredentialBackend = true;
        config.CreateJenkinsGitCredentialFrontend = true;
        if (_appSettings != null)
            config.JenkinsFolder = _appSettings.Jenkins.DefaultFolder;
        config.JenkinsProjectFolderName = config.ProjectSlug;

        // Database defaults
        config.CreateDatabaseUser = false;
        config.DatabaseUserStg = config.GetDefaultDatabaseUser();
        config.DatabaseUserPrd = config.GetDefaultDatabaseUser();

        // Docker defaults
        config.GenerateDockerCompose = true;
        config.DeployDockerCompose = true;
        config.DeployDockerComposeStg = true;
        config.DeployDockerComposePrd = true;
        config.CopyDockerComposeLocalStg = true;
        config.CopyDockerComposeLocalPrd = true;
        config.DeployFolderName = config.ProjectSlug;
        config.ContainerNameFrontend = config.GetDefaultContainerNameFrontend();
        config.ContainerNameBackend = config.GetDefaultContainerNameBackend();
        config.ReverseProxyHostStg = _appSettings?.Deployment.ReverseProxyHostStaging ?? "systems-stg.superint.ai";
        config.ReverseProxyHostPrd = _appSettings?.Deployment.ReverseProxyHostProduction ?? "systems.superint.ai";
        config.ReverseProxyPathPrefixStg = config.GetDefaultReverseProxyPathPrefix();
        config.ReverseProxyPathPrefixPrd = config.GetDefaultReverseProxyPathPrefix();
        config.ReverseProxyPathPrefixApiStg = $"{config.ReverseProxyPathPrefixStg}/api";
        config.ReverseProxyPathPrefixApiPrd = $"{config.ReverseProxyPathPrefixPrd}/api";
        config.LocalDeployPathStg = $@"E:\Projetos\superint.ai\Server\stg\compose\project-data\{config.DeployFolderName}";
        config.LocalDeployPathPrd = $@"E:\Projetos\superint.ai\Server\prd\compose\project-data\{config.DeployFolderName}";
    }

    private void RecreateStepViewModels()
    {
        _stepViewModels = CreateStepViewModels();
    }

    [RelayCommand]
    private void Cancel()
    {
        // Voltar para o primeiro step
        CurrentStepType = WizardStep.BasicInfo;
        UpdateCurrentStep();
        UpdateNavigationState();
    }

    private void UpdateCurrentStep()
    {
        CurrentStep = _stepViewModels[CurrentStepType];

        var stepInfo = Steps[(int)CurrentStepType];
        CurrentStepTitle = stepInfo.Title;
        DisplayStepNumber = stepInfo.DisplayNumber;

        // Atualizar estado ativo dos steps
        foreach (var step in Steps)
        {
            step.IsActive = step.StepType == CurrentStepType;
            step.IsCompleted = (int)step.StepType < (int)CurrentStepType;
        }

        // Refresh Jenkins step when navigating to it (to pick up Git URLs)
        if (CurrentStepType == WizardStep.Jenkins && _stepViewModels[WizardStep.Jenkins] is WizardJenkinsStepViewModel jenkinsVm)
        {
            jenkinsVm.Refresh();
        }

        // Refresh Review step when navigating to it
        if (CurrentStepType == WizardStep.Review && _stepViewModels[WizardStep.Review] is WizardReviewStepViewModel reviewVm)
        {
            reviewVm.Refresh();
        }
    }

    private void UpdateNavigationState()
    {
        var currentIndex = (int)CurrentStepType;
        CanGoBack = currentIndex > 0;
        CanGoNext = currentIndex < TotalSteps - 1;
        IsLastStep = currentIndex == TotalSteps - 1;
    }
}
