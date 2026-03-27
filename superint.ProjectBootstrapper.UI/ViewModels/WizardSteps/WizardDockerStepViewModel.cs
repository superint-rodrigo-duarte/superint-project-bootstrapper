using superint.ProjectBootstrapper.DTO;
using superint.ProjectBootstrapper.Shared.Enums;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardDockerStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardDockerStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public bool IsEnabled
    {
        get => _config.GenerateDockerCompose;
        set
        {
            if (SetProperty(_config.GenerateDockerCompose, value, _config, (c, v) => c.GenerateDockerCompose = v))
            {
                if (!value)
                {
                    _config.DeployDockerCompose = false;
                    _config.DeployDockerComposeStg = false;
                    _config.DeployDockerComposePrd = false;
                    _config.CopyDockerComposeLocalStg = false;
                    _config.CopyDockerComposeLocalPrd = false;
                    OnPropertyChanged(nameof(DeployToStg));
                    OnPropertyChanged(nameof(DeployToPrd));
                    OnPropertyChanged(nameof(CopyToLocalStg));
                    OnPropertyChanged(nameof(CopyToLocalPrd));
                }
            }
        }
    }

    public string DeployFolderName
    {
        get => _config.DeployFolderName ?? _config.GetDefaultDeployFolderName();
        set => SetProperty(_config.DeployFolderName, value, _config, (c, v) => c.DeployFolderName = v);
    }

    // ===== Container Names =====
    public string ContainerNameFrontend
    {
        get => _config.ContainerNameFrontend ?? _config.GetDefaultContainerNameFrontend();
        set => SetProperty(_config.ContainerNameFrontend, value, _config, (c, v) => c.ContainerNameFrontend = v);
    }

    public string ContainerNameBackend
    {
        get => _config.ContainerNameBackend ?? _config.GetDefaultContainerNameBackend();
        set => SetProperty(_config.ContainerNameBackend, value, _config, (c, v) => c.ContainerNameBackend = v);
    }

    // ===== Strip Prefix Options (STG) =====
    public bool UseStripPrefixFrontendStg
    {
        get => _config.UseStripPrefixFrontendStg;
        set => SetProperty(_config.UseStripPrefixFrontendStg, value, _config, (c, v) => c.UseStripPrefixFrontendStg = v);
    }

    public bool UseStripPrefixBackendStg
    {
        get => _config.UseStripPrefixBackendStg;
        set => SetProperty(_config.UseStripPrefixBackendStg, value, _config, (c, v) => c.UseStripPrefixBackendStg = v);
    }

    // ===== Strip Prefix Options (PRD) =====
    public bool UseStripPrefixFrontendPrd
    {
        get => _config.UseStripPrefixFrontendPrd;
        set => SetProperty(_config.UseStripPrefixFrontendPrd, value, _config, (c, v) => c.UseStripPrefixFrontendPrd = v);
    }

    public bool UseStripPrefixBackendPrd
    {
        get => _config.UseStripPrefixBackendPrd;
        set => SetProperty(_config.UseStripPrefixBackendPrd, value, _config, (c, v) => c.UseStripPrefixBackendPrd = v);
    }

    // ===== ReverseProxy Hosts =====
    public string ReverseProxyHostStg
    {
        get => _config.ReverseProxyHostStg ?? "systems-stg.superint.ai";
        set => SetProperty(_config.ReverseProxyHostStg, value, _config, (c, v) => c.ReverseProxyHostStg = v);
    }

    public string ReverseProxyHostPrd
    {
        get => _config.ReverseProxyHostPrd ?? "systems.superint.ai";
        set => SetProperty(_config.ReverseProxyHostPrd, value, _config, (c, v) => c.ReverseProxyHostPrd = v);
    }

    // ===== ReverseProxy Path Prefixes (Frontend) =====
    public string ReverseProxyPathPrefixStg
    {
        get => _config.ReverseProxyPathPrefixStg ?? _config.GetDefaultReverseProxyPathPrefix();
        set => SetProperty(_config.ReverseProxyPathPrefixStg, value, _config, (c, v) => c.ReverseProxyPathPrefixStg = v);
    }

    public string ReverseProxyPathPrefixPrd
    {
        get => _config.ReverseProxyPathPrefixPrd ?? _config.GetDefaultReverseProxyPathPrefix();
        set => SetProperty(_config.ReverseProxyPathPrefixPrd, value, _config, (c, v) => c.ReverseProxyPathPrefixPrd = v);
    }

    // ===== ReverseProxy Path Prefixes (Backend/API) =====
    public string ReverseProxyPathPrefixApiStg
    {
        get => _config.ReverseProxyPathPrefixApiStg ?? $"{_config.GetDefaultReverseProxyPathPrefix()}/api";
        set => SetProperty(_config.ReverseProxyPathPrefixApiStg, value, _config, (c, v) => c.ReverseProxyPathPrefixApiStg = v);
    }

    public string ReverseProxyPathPrefixApiPrd
    {
        get => _config.ReverseProxyPathPrefixApiPrd ?? $"{_config.GetDefaultReverseProxyPathPrefix()}/api";
        set => SetProperty(_config.ReverseProxyPathPrefixApiPrd, value, _config, (c, v) => c.ReverseProxyPathPrefixApiPrd = v);
    }

    // ===== Deploy Options =====
    public bool DeployToStg
    {
        get => _config.DeployDockerComposeStg;
        set
        {
            if (SetProperty(_config.DeployDockerComposeStg, value, _config, (c, v) => c.DeployDockerComposeStg = v))
            {
                OnPropertyChanged(nameof(DeployToStg));
            }
        }
    }

    public bool DeployToPrd
    {
        get => _config.DeployDockerComposePrd;
        set
        {
            if (SetProperty(_config.DeployDockerComposePrd, value, _config, (c, v) => c.DeployDockerComposePrd = v))
            {
                OnPropertyChanged(nameof(DeployToPrd));
            }
        }
    }

    // ===== Copy to Local (STG) =====
    public bool CopyToLocalStg
    {
        get => _config.CopyDockerComposeLocalStg;
        set
        {
            if (SetProperty(_config.CopyDockerComposeLocalStg, value, _config, (c, v) => c.CopyDockerComposeLocalStg = v))
            {
                OnPropertyChanged(nameof(CopyToLocalStg));
            }
        }
    }

    public string? LocalPathStg
    {
        get => _config.LocalDeployPathStg;
        set => SetProperty(_config.LocalDeployPathStg, value, _config, (c, v) => c.LocalDeployPathStg = v);
    }

    // ===== Copy to Local (PRD) =====
    public bool CopyToLocalPrd
    {
        get => _config.CopyDockerComposeLocalPrd;
        set
        {
            if (SetProperty(_config.CopyDockerComposeLocalPrd, value, _config, (c, v) => c.CopyDockerComposeLocalPrd = v))
            {
                OnPropertyChanged(nameof(CopyToLocalPrd));
            }
        }
    }

    public string? LocalPathPrd
    {
        get => _config.LocalDeployPathPrd;
        set => SetProperty(_config.LocalDeployPathPrd, value, _config, (c, v) => c.LocalDeployPathPrd = v);
    }

    // ===== Visibility Helpers =====
    public bool ShowFrontendOptions => _config.ProjectType == ProjectType.Frontend || _config.ProjectType == ProjectType.Fullstack;
    public bool ShowBackendOptions => _config.ProjectType == ProjectType.Backend || _config.ProjectType == ProjectType.Fullstack;

    // Project Type Helpers for layout
    public bool IsFullstack => _config.ProjectType == ProjectType.Fullstack;
    public bool IsFrontendOnly => _config.ProjectType == ProjectType.Frontend;
    public bool IsBackendOnly => _config.ProjectType == ProjectType.Backend;
}
