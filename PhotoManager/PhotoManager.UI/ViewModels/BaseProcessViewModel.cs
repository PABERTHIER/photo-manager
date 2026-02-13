using PhotoManager.Domain;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public abstract class BaseProcessViewModel<TC, TR> : BaseViewModel
{
    public abstract string Description { get; }

    public abstract TC GetProcessConfiguration();

    public abstract void SetProcessConfiguration(TC configuration);

    public abstract Task RunProcessAsync(ProcessStatusChangedCallback callback);

    public ObservableCollection<string> ProcessStatusMessages { get; } = [];

    public ObservableCollection<TR> Results
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(nameof(Results));
            NotifyPropertyChanged(nameof(CanViewResults));
        }
    } = [];

    public ProcessStep Step
    {
        get;

        private set
        {
            field = value;
            NotifyPropertyChanged(
                nameof(Step),
                nameof(DescriptionVisible),
                nameof(ConfigurationVisible),
                nameof(RunVisible),
                nameof(ResultsVisible),
                nameof(CanConfigure));
        }
    } = ProcessStep.ViewDescription;

    public Visibility DescriptionVisible =>
        Step == ProcessStep.ViewDescription ? Visibility.Visible : Visibility.Hidden;

    public Visibility ConfigurationVisible => Step == ProcessStep.Configure ? Visibility.Visible : Visibility.Hidden;

    public Visibility RunVisible => Step == ProcessStep.Run ? Visibility.Visible : Visibility.Hidden;

    public Visibility ResultsVisible => Step == ProcessStep.ViewResults ? Visibility.Visible : Visibility.Hidden;

    public bool CanViewDescription => Step == ProcessStep.ViewDescription;

    public bool CanConfigure => Step == ProcessStep.Configure;

    public bool CanViewResults => Step == ProcessStep.ViewResults && Results.Count > 0;

    public void AdvanceStep()
    {
        Step = Step switch
        {
            ProcessStep.ViewDescription => ProcessStep.Configure,
            ProcessStep.Configure => ProcessStep.Run,
            ProcessStep.Run => ProcessStep.ViewResults,
            _ => Step
        };
    }

    public void NotifyProcessStatusChanged(ProcessStatusChangedCallbackEventArgs e)
    {
        ProcessStatusMessages.Add(e.NewStatus);
    }
}
