using PhotoManager.Application;
using System.ComponentModel;

namespace PhotoManager.UI.ViewModels;

public abstract class BaseViewModel(IApplication application) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the application object.
    /// This property is declared as protected so the views
    /// always use the view model as a facade to the application object.
    /// </summary>
    protected IApplication Application { get; private set; } = application;
    // TODO: Remove IApplication application

    protected void NotifyPropertyChanged(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
