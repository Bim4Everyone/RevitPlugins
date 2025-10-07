using System;
using System.Globalization;
using System.Windows.Data;

using RevitServerFolders.ViewModels.Rs;

namespace RevitServerFolders.Converters;
[ValueConversion(typeof(RsModelObjectViewModel), typeof(object))]
internal sealed class RsNodeImageConverter : IValueConverter {
    public object Empty { get; set; }
    public object Server { get; set; }
    public object Folder { get; set; }
    public object OpenedFolder { get; set; }
    public object File { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is RsServerDataViewModel) {
            return Server;
        }
        if(value is RsModelDataViewModel) {
            return File;
        }

        return value is RsFolderDataViewModel folderDataViewModel
            ? folderDataViewModel.IsLoadedChildren
                ? OpenedFolder
                : Folder
            : Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
