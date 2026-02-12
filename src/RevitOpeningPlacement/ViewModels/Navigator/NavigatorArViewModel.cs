using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления окна для просмотра входящих заданий на отверстия от инженера в файле архитектора или конструктора
/// </summary>
internal class NavigatorArViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IConstantsProvider _constantsProvider;
    private readonly ILocalizationService _localization;

    public NavigatorArViewModel(
        RevitRepository revitRepository,
        IConstantsProvider constantsProvider,
        ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        OpeningsMepTaskIncoming = [];
        OpeningsReal = [];

        LoadViewCommand
            = RelayCommand.Create(LoadView);
        SelectCommand
            = RelayCommand.Create<ISelectorAndHighlighter>(SelectElement, CanSelect);
        RenewCommand
            = RelayCommand.Create(Renew);
        PlaceRealOpeningBySingleTaskCommand
            = RelayCommand.Create(PlaceRealOpeningBySingleTask);
        PlaceOneRealOpeningByManyTasksCommand
            = RelayCommand.Create(PlaceOneRealOpeningByManyTasks);
        PlaceManyRealOpeningsByManyTasksCommand
            = RelayCommand.Create(PlaceManyRealOpeningsByManyTasks);
        PlaceManyRealOpeningsByManyTasksInManyHostsCommand
            = RelayCommand.Create(PlaceManyRealOpeningsByManyTasksInManyHosts);
    }

    // Входящие задания на отверстия
    public ObservableCollection<IOpeningMepTaskIncomingToArViewModel> OpeningsMepTaskIncoming { get; }

    // Чистовые отверстия из активного документа
    public bool ShowOpeningsReal => OpeningsReal.Count > 0;
    public ObservableCollection<IOpeningRealArViewModel> OpeningsReal { get; }

    public ICommand LoadViewCommand { get; }

    public ICommand SelectCommand { get; }

    public ICommand RenewCommand { get; }

    public ICommand PlaceRealOpeningBySingleTaskCommand { get; }

    public ICommand PlaceOneRealOpeningByManyTasksCommand { get; }

    public ICommand PlaceManyRealOpeningsByManyTasksCommand { get; }

    public ICommand PlaceManyRealOpeningsByManyTasksInManyHostsCommand { get; }

    private void SelectElement(ISelectorAndHighlighter famInstanceProvider) {
        _revitRepository.SelectAndShowElement(famInstanceProvider);
    }

    private bool CanSelect(ISelectorAndHighlighter p) {
        return p != null;
    }

    private void Renew() {
        void action() {
            var command = new GetOpeningTasksCmd();
            command.ExecuteCommand(_revitRepository.UIApplication);
        }

        _revitRepository.DoAction(action);
    }

    private void PlaceRealOpeningBySingleTask() {
        void action() {
            var cmd = new PlaceOneOpeningRealByOneTaskCmd();
            cmd.ExecuteCommand(_revitRepository.UIApplication);
        }

        _revitRepository.DoAction(action);
    }

    private void PlaceOneRealOpeningByManyTasks() {
        void action() {
            var cmd = new PlaceOneOpeningRealByManyTasksCmd();
            cmd.ExecuteCommand(_revitRepository.UIApplication);
        }

        _revitRepository.DoAction(action);
    }

    private void PlaceManyRealOpeningsByManyTasks() {
        void action() {
            var cmd = new PlaceManyOpeningRealsByManyTasksInOneHostCmd();
            cmd.ExecuteCommand(_revitRepository.UIApplication);
        }

        _revitRepository.DoAction(action);
    }

    private void PlaceManyRealOpeningsByManyTasksInManyHosts() {
        void action() {
            var cmd = new PlaceManyOpeningRealsByManyTasksInManyHostsCmd();
            cmd.ExecuteCommand(_revitRepository.UIApplication);
        }

        _revitRepository.DoAction(action);
    }

    private void LoadView() {
        var realOpenings = _revitRepository.GetRealOpeningsAr();

        LoadIncomingTasks(realOpenings);
        LoadOpeningsReal(realOpenings);
    }

    private void LoadIncomingTasks(ICollection<OpeningRealAr> realOpenings) {
        var incomingTasks = _revitRepository.GetOpeningsMepTasksIncoming();
        var constructureElementsIds = _revitRepository.GetConstructureElementsIds();
        var incomingTasksViewModels = GetOpeningsMepIncomingTasksViewModels(
            incomingTasks,
            realOpenings.ToArray<IOpeningReal>(),
            constructureElementsIds);
        OpeningsMepTaskIncoming.Clear();
        foreach(var incomingTask in incomingTasksViewModels) {
            OpeningsMepTaskIncoming.Add(incomingTask);
        }

        ICollection<(FamilyInstance Opening, Transform Transform)> uniqueTasks =
            _revitRepository.GetOpeningsIncomingUnique(
                RevitRepository.MepUniqueFamilyName,
                BuiltInCategory.OST_GenericModel);
        foreach(var item in uniqueTasks) {
            OpeningsMepTaskIncoming.Add(
                new OpeningMepTaskIncomingUniqueViewModel(
                    item.Opening,
                    item.Transform,
                    _localization.GetLocalizedString("Unique")));
        }
    }

    private void LoadOpeningsReal(ICollection<OpeningRealAr> realOpenings) {
        ICollection<IMepLinkElementsProvider> mepLinks = _revitRepository
            .GetSelectedRevitLinks()
            .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
            .ToArray();
        var openingsRealViewModels = GetOpeningsRealArViewModels(mepLinks, realOpenings);
        OpeningsReal.Clear();
        foreach(var openingReal in openingsRealViewModels) {
            OpeningsReal.Add(openingReal);
        }

        var uniqueOpenings = _revitRepository.GetOpeningsOutcomingUnique(
            RevitRepository.ArUniqueFamilyName,
            BuiltInCategory.OST_Windows);
        foreach(var item in uniqueOpenings) {
            OpeningsReal.Add(new OpeningRealArUniqueViewModel(item, _localization.GetLocalizedString("Unique")));
        }

        OnPropertyChanged(nameof(ShowOpeningsReal));
    }

    /// <summary>
    /// Возвращает коллекцию моделей представления для входящих заданий на отверстия из ВИС
    /// </summary>
    /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
    /// <param name="realOpenings">Чистовые отверстия из текущего документа</param>
    /// <param name="constructureElementsIds">Элементы конструкций из текущего документа</param>
    private ICollection<OpeningMepTaskIncomingViewModel> GetOpeningsMepIncomingTasksViewModels(
        ICollection<OpeningMepTaskIncoming> incomingTasks,
        ICollection<IOpeningReal> realOpenings,
        ICollection<ElementId> constructureElementsIds) {
        var incomingTasksViewModels = new HashSet<OpeningMepTaskIncomingViewModel>();

        using(var pb = GetPlatformService<IProgressDialogService>()) {
            pb.StepValue = _constantsProvider.ProgressBarStepLarge;
            pb.DisplayTitleFormat = "Анализ заданий... [{0}\\{1}]";
            var progress = pb.CreateProgress();
            pb.MaxValue = incomingTasks.Count;
            var ct = pb.CreateCancellationToken();
            pb.Show();

            int i = 0;
            foreach(var incomingTask in incomingTasks) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i);
                try {
                    incomingTask.UpdateStatusAndHostName(realOpenings, constructureElementsIds);
                } catch(ArgumentException) {
                    // не удалось получить солид у задания на отверстие. Например, если его толщина равна 0
                    continue;
                }

                incomingTasksViewModels.Add(new OpeningMepTaskIncomingViewModel(incomingTask));
                i++;
            }
        }

        return incomingTasksViewModels;
    }

    /// <summary>
    /// Возвращает коллекцию моделей представления чистовых отверстий, размещенных в активном документа АР
    /// </summary>
    /// <param name="mepLinks">Связи ВИС</param>
    /// <param name="openingsReal">Чистовые отверстия, размещенные в активном документе АР</param>
    private ICollection<OpeningRealArViewModel> GetOpeningsRealArViewModels(
        ICollection<IMepLinkElementsProvider> mepLinks,
        ICollection<OpeningRealAr> openingsReal) {
        var openingsRealViewModels = new HashSet<OpeningRealArViewModel>();

        using(var pb = GetPlatformService<IProgressDialogService>()) {
            pb.StepValue = _constantsProvider.ProgressBarStepSmall;
            pb.DisplayTitleFormat = "Анализ отверстий... [{0}\\{1}]";
            var progress = pb.CreateProgress();
            pb.MaxValue = openingsReal.Count;
            var ct = pb.CreateCancellationToken();
            pb.Show();

            int i = 0;
            foreach(var openingReal in openingsReal) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i);
                openingReal.UpdateStatus(mepLinks);
                openingsRealViewModels.Add(new OpeningRealArViewModel(openingReal));
                i++;
            }
        }

        return openingsRealViewModels;
    }
}
