using CommunityToolkit.Mvvm.ComponentModel;
using MediaDownloader.Services.Models;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Represents a selectable video quality in the user interface.
/// </summary>
public partial class VideoQualityOptionViewModel : ObservableObject
{
    private readonly Action<string>? selectionChanged;

    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoQualityOptionViewModel"/> class.
    /// </summary>
    /// <param name="model">Source service model.</param>
    /// <param name="selectionChanged">Callback invoked when this quality is selected.</param>
    public VideoQualityOptionViewModel(
        VideoQualityOption model,
        Action<string>? selectionChanged = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        Name = model.Name;
        this.selectionChanged = selectionChanged;
        isSelected = model.IsSelected;
    }

    public string Name { get; }

    /// <summary>
    /// Converts this view model to its persistence model.
    /// </summary>
    /// <returns>Persistable video quality data.</returns>
    public VideoQualityOption ToModel() =>
        new()
        {
            Name = Name,
            IsSelected = IsSelected
        };

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            selectionChanged?.Invoke(Name);
        }
    }
}
