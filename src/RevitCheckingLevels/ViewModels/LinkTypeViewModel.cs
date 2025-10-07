using System;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCheckingLevels.ViewModels;
internal class LinkTypeViewModel : BaseViewModel {
    private readonly Workset _workset;
    private string _linkLoadToolTip;

    public LinkTypeViewModel(RevitLinkType linkType) {
        Element = linkType;
        _workset = Element.Document.GetWorksetTable().GetWorkset(Element.WorksetId);

        LinkLoadCommand = new RelayCommand(LinkLoad, CanLinkLoad);
    }

    public RevitLinkType Element { get; }

    public ElementId Id => Element.Id;
    public string Name => Element.Name;
    public bool IsLinkLoaded => Element.GetLinkedFileStatus() == LinkedFileStatus.Loaded;

    public ICommand LinkLoadCommand { get; }

    public string LinkLoadToolTip {
        get => _linkLoadToolTip;
        set => RaiseAndSetIfChanged(ref _linkLoadToolTip, value);
    }

    private void LinkLoad(object p) {
        Element.Load();
        OnPropertyChanged(nameof(IsLinkLoaded));
    }

    private bool CanLinkLoad(object p) {
        if(IsLinkLoaded) {
            LinkLoadToolTip = "Данная связь уже загружена.";
            return false;
        }

        if(!_workset.IsOpen) {
            LinkLoadToolTip = $"Откройте рабочий набор \"{_workset.Name}\"."
                              + Environment.NewLine
                              + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!";

            return false;
        }

        LinkLoadToolTip = "Загрузить координационный файл";
        return true;

    }
}
