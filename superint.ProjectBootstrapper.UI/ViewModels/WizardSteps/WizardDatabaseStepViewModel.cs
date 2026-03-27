using superint.ProjectBootstrapper.DTO;

namespace superint.ProjectBootstrapper.UI.ViewModels.WizardSteps;

public partial class WizardDatabaseStepViewModel : ViewModelBase
{
    private readonly ProjectConfiguration _config;

    public WizardDatabaseStepViewModel(ProjectConfiguration config)
    {
        _config = config;
    }

    public bool IsEnabled
    {
        get => _config.CreateDatabaseUser;
        set => SetProperty(_config.CreateDatabaseUser, value, _config, (c, v) => c.CreateDatabaseUser = v);
    }

    public bool CreateInStg
    {
        get => _config.CreateDatabaseUserStg;
        set
        {
            if (SetProperty(_config.CreateDatabaseUserStg, value, _config, (c, v) => c.CreateDatabaseUserStg = v))
            {
                if (!value && !_config.CreateDatabaseUserPrd)
                {
                    _config.CreateDatabaseUser = false;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }
    }

    public bool CreateInPrd
    {
        get => _config.CreateDatabaseUserPrd;
        set
        {
            if (SetProperty(_config.CreateDatabaseUserPrd, value, _config, (c, v) => c.CreateDatabaseUserPrd = v))
            {
                if (!value && !_config.CreateDatabaseUserStg)
                {
                    _config.CreateDatabaseUser = false;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }
    }

    public string UserStg
    {
        get => _config.DatabaseUserStg ?? _config.GetDefaultDatabaseUser();
        set => SetProperty(_config.DatabaseUserStg, value, _config, (c, v) => c.DatabaseUserStg = v);
    }

    public string? PasswordStg
    {
        get => _config.DatabasePasswordStg;
        set => SetProperty(_config.DatabasePasswordStg, value, _config, (c, v) => c.DatabasePasswordStg = v);
    }

    public string UserPrd
    {
        get => _config.DatabaseUserPrd ?? _config.GetDefaultDatabaseUser();
        set => SetProperty(_config.DatabaseUserPrd, value, _config, (c, v) => c.DatabaseUserPrd = v);
    }

    public string? PasswordPrd
    {
        get => _config.DatabasePasswordPrd;
        set => SetProperty(_config.DatabasePasswordPrd, value, _config, (c, v) => c.DatabasePasswordPrd = v);
    }
}
