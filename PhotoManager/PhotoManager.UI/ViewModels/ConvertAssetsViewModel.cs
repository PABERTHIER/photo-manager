using Avalonia.Threading;
using PhotoManager.Application;
using PhotoManager.Domain;
using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public class ConvertAssetsViewModel(IApplication application) : BaseViewModel
{
    public ObservableCollection<string> SelectedFilePaths { get; } = [];
    public string[] TargetFormats { get; } = ["JPG", "PNG"];

    public string SelectedTargetFormat
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(SelectedTargetFormat));
        }
    }
        = "JPG";

    public bool IsConverting
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(IsConverting), nameof(CanConvert));
        }
    }

    public double ProgressPercentage
    {
        get;
        private set
        {
            if (field.Equals(value))
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(ProgressPercentage));
        }
    }

    public string ProgressMessage
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(ProgressMessage));
        }
    }
        = string.Empty;

    public string SummaryMessage
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            NotifyPropertyChanged(nameof(SummaryMessage));
        }
    }
        = string.Empty;

    public bool CanConvert => SelectedFilePaths.Count > 0 && !IsConverting;

    public void SetSelectedFilePaths(IEnumerable<string> filePaths)
    {
        SelectedFilePaths.Clear();

        foreach (string filePath in filePaths)
        {
            SelectedFilePaths.Add(filePath);
        }

        SummaryMessage = $"{SelectedFilePaths.Count} image(s) selected.";
        NotifyPropertyChanged(nameof(CanConvert), nameof(SelectedFilePaths));
    }

    public async Task<AssetConversionResult?> ConvertAsync(CancellationToken token = default)
    {
        if (SelectedFilePaths.Count == 0)
        {
            SummaryMessage = "Select at least one image before converting.";
            return null;
        }

        IsConverting = true;
        ProgressPercentage = 0;
        ProgressMessage = "Starting conversion...";
        SummaryMessage = string.Empty;

        try
        {
            AssetConversionRequest request = new(SelectedFilePaths.ToArray(), GetTargetFormat());
            AssetConversionResult result = await application.ConvertAssetsAsync(request, NotifyProgress, token);
            SummaryMessage = $"Converted: {result.ConvertedCount}. Failed: {result.FailedCount}.";

            return result;
        }
        finally
        {
            IsConverting = false;
        }
    }

    private void NotifyProgress(AssetConversionProgressEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => NotifyProgress(e)).GetAwaiter().GetResult();
            return;
        }

        ProgressPercentage = e.ProgressPercentage;
        ProgressMessage = e.Message;
    }

    private AssetConversionTargetFormat GetTargetFormat() =>
        SelectedTargetFormat.Equals("PNG", StringComparison.OrdinalIgnoreCase)
            ? AssetConversionTargetFormat.Png
            : AssetConversionTargetFormat.Jpeg;

}
