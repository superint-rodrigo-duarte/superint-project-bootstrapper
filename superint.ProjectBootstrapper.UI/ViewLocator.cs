using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using superint.ProjectBootstrapper.UI.ViewModels;

namespace superint.ProjectBootstrapper.UI;

/// <summary>
/// Localiza Views correspondentes a ViewModels usando convenção de nomes.
/// Inclui cache de tipos para melhor performance.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    private static readonly ConcurrentDictionary<Type, Type?> ViewTypeCache = new();

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var viewModelType = param.GetType();

        try
        {
            var viewType = ViewTypeCache.GetOrAdd(viewModelType, ResolveViewType);

            if (viewType != null)
            {
                var view = Activator.CreateInstance(viewType);
                if (view is Control control)
                    return control;
            }

            return CreateNotFoundControl(viewModelType);
        }
        catch (Exception ex)
        {
            return CreateErrorControl(viewModelType, ex);
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private static Type? ResolveViewType(Type viewModelType)
    {
        var fullName = viewModelType.FullName;
        if (string.IsNullOrEmpty(fullName))
            return null;

        var viewName = fullName.Replace("ViewModel", "View", StringComparison.Ordinal);
        return Type.GetType(viewName);
    }

    private static Control CreateNotFoundControl(Type viewModelType)
    {
        return new Border
        {
            Background = Brushes.DarkOrange,
            Padding = new Avalonia.Thickness(16),
            CornerRadius = new Avalonia.CornerRadius(8),
            Child = new TextBlock
            {
                Text = $"View não encontrada para: {viewModelType.Name}",
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold
            }
        };
    }

    private static Control CreateErrorControl(Type viewModelType, Exception ex)
    {
        return new Border
        {
            Background = Brushes.DarkRed,
            Padding = new Avalonia.Thickness(16),
            CornerRadius = new Avalonia.CornerRadius(8),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Erro ao criar View para: {viewModelType.Name}",
                        Foreground = Brushes.White,
                        FontWeight = FontWeight.Bold
                    },
                    new TextBlock
                    {
                        Text = ex.Message,
                        Foreground = Brushes.LightCoral,
                        FontSize = 11,
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }
}
