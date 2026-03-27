using System.Collections;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace superint.ProjectBootstrapper.UI.ViewModels;

/// <summary>
/// Base class for all ViewModels with common functionality.
/// </summary>
public abstract class ViewModelBase : ObservableObject, INotifyDataErrorInfo, IDisposable
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private bool _isBusy;
    private string? _errorMessage;
    private bool _disposed;

    /// <summary>
    /// Indicates if the ViewModel is currently busy performing an operation.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        protected set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Current error message, if any.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set => SetProperty(ref _errorMessage, value);
    }

    #region INotifyDataErrorInfo

    /// <summary>
    /// Indicates if there are any validation errors.
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Raised when validation errors change for a property.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets the validation errors for a specified property.
    /// </summary>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(e => e.Value);

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Adds a validation error for a property.
    /// </summary>
    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears all validation errors for a property.
    /// </summary>
    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Clears all validation errors.
    /// </summary>
    protected void ClearAllErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
            OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Validates a required field.
    /// </summary>
    protected bool ValidateRequired(string? value, string propertyName, string? errorMessage = null)
    {
        ClearErrors(propertyName);

        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(propertyName, errorMessage ?? $"{propertyName} is required.");
            return false;
        }

        return true;
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes resources used by the ViewModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override to dispose managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _errors.Clear();
        }

        _disposed = true;
    }

    #endregion
}
