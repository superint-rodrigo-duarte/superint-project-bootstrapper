using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using superint.ProjectBootstrapper.Application.Services;
using superint.ProjectBootstrapper.UI.Constants;
using superint.ProjectBootstrapper.UI.Models;

namespace superint.ProjectBootstrapper.UI.ViewModels;

public partial class TestConnectionsViewModel : ViewModelBase
{
    private readonly ProjectBootstrapService? _bootstrapService;

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private ObservableCollection<ConnectionTestResult> _results = [];

    public TestConnectionsViewModel(ProjectBootstrapService bootstrapService)
    {
        _bootstrapService = bootstrapService;
        InitializeResults();
    }

    // Design-time constructor
    public TestConnectionsViewModel()
    {
        InitializeResults();
    }

    private void InitializeResults()
    {
        Results =
        [
            new() { ServiceName = "Git", ServiceKey = "Git", Status = ConnectionStatus.Pending },
            new() { ServiceName = "Harbor (Container Registry)", ServiceKey = "Container Registry", Status = ConnectionStatus.Pending },
            new() { ServiceName = "Jenkins", ServiceKey = "Jenkins", Status = ConnectionStatus.Pending },
            new() { ServiceName = "SSH - STG", ServiceKey = "SSH (STG)", Status = ConnectionStatus.Pending },
            new() { ServiceName = "SSH - PRD", ServiceKey = "SSH (PRD)", Status = ConnectionStatus.Pending },
            new() { ServiceName = "PostgreSQL - STG", ServiceKey = "Database (STG)", Status = ConnectionStatus.Pending },
            new() { ServiceName = "PostgreSQL - PRD", ServiceKey = "Database (PRD)", Status = ConnectionStatus.Pending }
        ];
    }

    [RelayCommand]
    private async Task TestAllConnectionsAsync()
    {
        if (_bootstrapService == null)
        {
            await SimulateTestsAsync();
            return;
        }

        IsTesting = true;

        foreach (var result in Results)
        {
            result.Status = ConnectionStatus.Testing;
            result.Message = null;
            result.Details = null;
        }

        try
        {
            var connectionResults = await _bootstrapService.TestConnectionsAsync();

            foreach (var result in Results)
            {
                if (connectionResults.TryGetValue(result.ServiceKey, out var opResult))
                {
                    result.Status = opResult.Success ? ConnectionStatus.Success : ConnectionStatus.Failed;
                    result.Message = opResult.Message;
                    result.Details = opResult.Details;
                }
                else
                {
                    result.Status = ConnectionStatus.Failed;
                    result.Message = UIStrings.Messages.ServiceNotFound;
                }
            }
        }
        catch (Exception ex)
        {
            foreach (var result in Results)
            {
                if (result.Status == ConnectionStatus.Testing)
                {
                    result.Status = ConnectionStatus.Failed;
                    result.Message = $"{UIStrings.Messages.ErrorPrefix}{ex.Message}";
                }
            }
        }
        finally
        {
            IsTesting = false;
        }
    }

    [RelayCommand]
    private async Task TestSingleConnectionAsync(ConnectionTestResult? result)
    {
        if (result == null) return;

        if (_bootstrapService == null)
        {
            result.Status = ConnectionStatus.Testing;
            await Task.Delay(500);
            result.Status = ConnectionStatus.Success;
            result.Message = UIStrings.Messages.SimulatedConnection;
            return;
        }

        result.Status = ConnectionStatus.Testing;
        result.Message = null;
        result.Details = null;

        try
        {
            var connectionResults = await _bootstrapService.TestConnectionsAsync();

            if (connectionResults.TryGetValue(result.ServiceKey, out var opResult))
            {
                result.Status = opResult.Success ? ConnectionStatus.Success : ConnectionStatus.Failed;
                result.Message = opResult.Message;
                result.Details = opResult.Details;
            }
            else
            {
                result.Status = ConnectionStatus.Failed;
                result.Message = UIStrings.Messages.ServiceNotFound;
            }
        }
        catch (Exception ex)
        {
            result.Status = ConnectionStatus.Failed;
            result.Message = $"{UIStrings.Messages.ErrorPrefix}{ex.Message}";
        }
    }

    private async Task SimulateTestsAsync()
    {
        IsTesting = true;

        foreach (var result in Results)
        {
            result.Status = ConnectionStatus.Testing;
        }

        await Task.Delay(500);

        foreach (var result in Results)
        {
            result.Status = ConnectionStatus.Pending;
            result.Message = UIStrings.Messages.DesignTimeMode;
        }

        IsTesting = false;
    }
}
