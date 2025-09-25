using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels;

internal class ElementInfosViewModel : BaseViewModel {
    private readonly ViewOrientation3D _orientation;
    private readonly RevitRepository _revitRepository;
    private CollectionViewSource _descriptionViewSoure;
    private ObservableCollection<ElementInfoViewModel> _elementIfos;
    private CollectionViewSource _elementInfosViewSource;
    private ObservableCollection<GroupedDescriptionViewModel> _groupedDescriptions;
    private GroupedDescriptionViewModel _selectedGroupedDescription;

    public ElementInfosViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        ElementInfos = new ObservableCollection<ElementInfoViewModel>();
        _orientation = _revitRepository.GetOrientation3D();
        SelectElementCommand = new RelayCommand(async p => await SelectElement(p));

        InitializeTypeInfos();

        ElementInfosViewSource = new CollectionViewSource { Source = ElementInfos };
        DescriptionCheckedCommand = new RelayCommand(DescriptionChecked);
        TypeInfoCheckedCommand = new RelayCommand(TypeInfoChecked);
        ElementInfosViewSource.Filter += MessageFilter;
    }

    public ICommand SelectElementCommand { get; set; }
    public ICommand TypeInfoCheckedCommand { get; set; }
    public ICommand DescriptionCheckedCommand { get; set; }

    public GroupedDescriptionViewModel SelectedGroupedDescription {
        get => _selectedGroupedDescription;
        set => RaiseAndSetIfChanged(ref _selectedGroupedDescription, value);
    }

    public ObservableCollection<ElementInfoViewModel> ElementInfos {
        get => _elementIfos;
        set => RaiseAndSetIfChanged(ref _elementIfos, value);
    }

    public ObservableCollection<GroupedDescriptionViewModel> GroupedDescriptions {
        get => _groupedDescriptions;
        set => RaiseAndSetIfChanged(ref _groupedDescriptions, value);
    }

    public CollectionViewSource DescriptionViewSoure {
        get => _descriptionViewSoure;
        set => RaiseAndSetIfChanged(ref _descriptionViewSoure, value);
    }

    public List<object> SelectedObjects { get; set; } = new();
    public List<TypeInfo> TypeInfos => SelectedObjects?.OfType<TypeInfo>()?.ToList() ?? new List<TypeInfo>();

    public CollectionViewSource ElementInfosViewSource {
        get => _elementInfosViewSource;
        set => RaiseAndSetIfChanged(ref _elementInfosViewSource, value);
    }

    public void UpdateCollection() {
        ElementInfos = new ObservableCollection<ElementInfoViewModel>(
            ElementInfos
                .Distinct()
                .OrderBy(e => e.Name));
        ElementInfosViewSource.Source = ElementInfos;
    }

    public void UpdateGroupedMessage() {
        if(ElementInfos.Count > 0) {
            GroupedDescriptions = new ObservableCollection<GroupedDescriptionViewModel>(
                ElementInfos.GroupBy(item =>
                        new {
                            Type = item.TypeInfo,
                            item.Message
                        })
                    .Select(item => new GroupedDescriptionViewModel {
                        TypeInfo = item.Key.Type,
                        Message = item.Key.Message
                    }));
            SelectedGroupedDescription = GroupedDescriptions.FirstOrDefault();
        } else {
            GroupedDescriptions = new ObservableCollection<GroupedDescriptionViewModel>();
        }

        DescriptionViewSoure = new CollectionViewSource { Source = GroupedDescriptions };
        DescriptionViewSoure.Filter += TypeFilter;
    }

    private async Task SelectElement(object p) {
        if(p is ElementInfoViewModel elementInfo) {
            if(_revitRepository.GetElementById(elementInfo.ElementId) is Autodesk.Revit.DB.ElementType
               elementType) {
                TaskDialog.Show("Revit", "Невозможно подобрать вид для отображения элемента.");
            } else {
                await _revitRepository.SelectAndShowElement(elementInfo.ElementId, _orientation);
            }
        }
    }

    private void InitializeTypeInfos() {
        foreach(object value in Enum.GetValues(typeof(TypeInfo))) {
            SelectedObjects.Add(value);
        }
    }

    private void TypeFilter(object sender, FilterEventArgs e) {
        if(e.Item is GroupedDescriptionViewModel description) {
            if(!TypeInfos
                   .Any(t => t == description.TypeInfo)) {
                e.Accepted = false;
            }
        }
    }

    private void MessageFilter(object sender, FilterEventArgs e) {
        if(SelectedGroupedDescription == null) {
            e.Accepted = false;
            return;
        }

        if(e.Item is ElementInfoViewModel elementInfo) {
            if(!elementInfo.Message.Equals(
                   SelectedGroupedDescription.Message,
                   StringComparison.CurrentCultureIgnoreCase)
               || elementInfo.TypeInfo != SelectedGroupedDescription.TypeInfo) {
                e.Accepted = false;
            }
        }
    }

    private void TypeInfoChecked(object p) {
        DescriptionViewSoure.View.Refresh();
    }

    private void DescriptionChecked(object p) {
        ElementInfosViewSource.View.Refresh();
    }
}

internal class GroupedDescriptionViewModel : BaseViewModel {
    private string _message;
    private TypeInfo _typeInfo;

    public TypeInfo TypeInfo {
        get => _typeInfo;
        set => RaiseAndSetIfChanged(ref _typeInfo, value);
    }

    public string Message {
        get => _message;
        set => RaiseAndSetIfChanged(ref _message, value);
    }
}
