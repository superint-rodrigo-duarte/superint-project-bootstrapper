using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace superint.ProjectBootstrapper.UI.Collections;

/// <summary>
/// A wrapper class that provides INotifyPropertyChanged support for any DTO object.
/// This allows DTOs to be used with two-way binding in MVVM scenarios without
/// modifying the original DTO classes.
/// </summary>
/// <typeparam name="T">The type of the wrapped object.</typeparam>
public class ObservableWrapper<T> : INotifyPropertyChanged where T : class
{
    private T _model;

    /// <summary>
    /// Creates a new ObservableWrapper for the specified model.
    /// </summary>
    /// <param name="model">The model to wrap.</param>
    public ObservableWrapper(T model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    /// Gets or sets the wrapped model.
    /// </summary>
    public T Model
    {
        get => _model;
        set
        {
            if (ReferenceEquals(_model, value)) return;
            _model = value;
            OnPropertyChanged();
            OnAllPropertiesChanged();
        }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises PropertyChanged for all properties (empty string notification).
    /// </summary>
    protected void OnAllPropertiesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    /// <summary>
    /// Sets a property value on the model and raises PropertyChanged if the value changed.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="getter">Function to get the current value.</param>
    /// <param name="setter">Action to set the new value.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property (auto-filled by compiler).</param>
    /// <returns>True if the value changed, false otherwise.</returns>
    protected bool SetModelProperty<TProperty>(
        Func<TProperty> getter,
        Action<TProperty> setter,
        TProperty value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TProperty>.Default.Equals(getter(), value))
            return false;

        setter(value);
        OnPropertyChanged(propertyName);
        return true;
    }
}
