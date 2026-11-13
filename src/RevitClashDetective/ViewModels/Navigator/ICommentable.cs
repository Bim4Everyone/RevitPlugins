using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RevitClashDetective.ViewModels.Navigator;

/// <summary>
/// Интерфейс для объектов, которые могут иметь комментарии
/// </summary>
internal interface ICommentable {
    string CommentsTitle { get; }
    ICommand AddCommentCommand { get; }
    ICommand RemoveCommentCommand { get; }
    ClashCommentViewModel SelectedComment { get; set; }
    bool CanEditComments { get; set; }
    ObservableCollection<ClashCommentViewModel> Comments { get; }
}
