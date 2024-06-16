using PhotoManager.Application;
using PhotoManager.Domain;
using PhotoManager.UI.ViewModels.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoManager.UI.ViewModels;

public abstract class BaseProcessViewModel<C, R> : BaseViewModel
{
    private ProcessStep step = ProcessStep.ViewDescription;
    private ObservableCollection<string> processStatusMessages;
    private ObservableCollection<R> results;

    protected BaseProcessViewModel(IApplication application) : base(application)
    {
        processStatusMessages = new ObservableCollection<string>();
    }

    public ObservableCollection<string> ProcessStatusMessages
    {
        get { return processStatusMessages; }
    }

    public ObservableCollection<R> Results
    {
        get { return results; }
        set
        {
            results = value;
            NotifyPropertyChanged(nameof(Results));
            NotifyPropertyChanged(nameof(CanViewResults));
        }
    }

    public abstract C GetProcessConfiguration();

    public abstract void SetProcessConfiguration(C configuration);

    public abstract string Description { get; }

    public ProcessStep Step
    {
        get { return step; }

        private set
        {
            step = value;
            NotifyPropertyChanged(
                nameof(Step),
                nameof(DescriptionVisible),
                nameof(ConfigurationVisible),
                nameof(RunVisible),
                nameof(ResultsVisible),
                nameof(CanConfigure));
        }
    }

    public Visibility DescriptionVisible
    {
        get { return Step == ProcessStep.ViewDescription ? Visibility.Visible : Visibility.Hidden; }
    }

    public Visibility ConfigurationVisible
    {
        get { return Step == ProcessStep.Configure ? Visibility.Visible : Visibility.Hidden; }
    }

    public Visibility RunVisible
    {
        get { return Step == ProcessStep.Run ? Visibility.Visible : Visibility.Hidden; }
    }

    public Visibility ResultsVisible
    {
        get { return Step == ProcessStep.ViewResults ? Visibility.Visible : Visibility.Hidden; }
    }

    public bool CanViewDescription
    {
        get { return Step == ProcessStep.ViewDescription; }
    }

    public bool CanConfigure
    {
        get { return Step == ProcessStep.Configure; }
    }

    public bool CanViewResults
    {
        get { return Step == ProcessStep.Run && Results != null && Results.Count > 0; }
    }

    public void AdvanceStep()
    {
        switch (Step)
        {
            case ProcessStep.ViewDescription:
                Step = ProcessStep.Configure;
                break;

            case ProcessStep.Configure:
                Step = ProcessStep.Run;
                break;

            case ProcessStep.Run:
                Step = ProcessStep.ViewResults;
                break;
        }
    }

    public void NotifyProcessStatusChanged(ProcessStatusChangedCallbackEventArgs e)
    {
        ProcessStatusMessages.Add(e.NewStatus);
    }

    public abstract Task RunProcessAsync(ProcessStatusChangedCallback callback);
}
