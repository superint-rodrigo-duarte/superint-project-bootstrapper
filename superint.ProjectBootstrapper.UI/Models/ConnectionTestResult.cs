using CommunityToolkit.Mvvm.ComponentModel;

namespace superint.ProjectBootstrapper.UI.Models;

/// <summary>
/// Resultado de um teste de conexao com um servico externo
/// </summary>
public partial class ConnectionTestResult : ObservableObject
{
    [ObservableProperty]
    private string _serviceName = string.Empty;

    [ObservableProperty]
    private string _serviceKey = string.Empty;

    [ObservableProperty]
    private ConnectionStatus _status = ConnectionStatus.Pending;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _details;
}
