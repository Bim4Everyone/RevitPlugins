using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

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
    private readonly IResolutionRoot _resolutionRoot;

    public NavigatorArViewModel(
        RevitRepository revitRepository,
        IConstantsProvider constantsProvider,
        IProgressDialogFactory progressDialogFactory,
        IMessageBoxService messageBoxService,
        IResolutionRoot resolutionRoot,
        ILocalizationService localization) {
        ProgressDialogFactory = progressDialogFactory ?? throw new ArgumentNullException(nameof(progressDialogFactory));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
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

    public IProgressDialogFactory ProgressDialogFactory { get; }
    public IMessageBoxService MessageBoxService { get; }

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
        _revitRepository.SelectAndShowElement(famInstanceProvider, MessageBoxService);
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
        LoadIncomingTasks();
        LoadOpeningsReal();
    }

    private void LoadIncomingTasks() {
        var incomingTasksViewModels = GetOpeningsMepIncomingTasksViewModels(
            _revitRepository.GetOpeningsMepTasksIncoming());
        OpeningsMepTaskIncoming.Clear();
        foreach(var incomingTask in incomingTasksViewModels) {
            OpeningsMepTaskIncoming.Add(incomingTask);
        }

        var uniqueTasks = _revitRepository.GetFamilyInstancesFromLinks(
            RevitRepository.MepUniqueFamilyName,
            BuiltInCategory.OST_GenericModel);
        foreach(var item in uniqueTasks) {
            OpeningsMepTaskIncoming.Add(
                new OpeningMepTaskIncomingUniqueViewModel(
                    item.Instance,
                    item.Transform,
                    _localization.GetLocalizedString("AllOpeningStatus.Unique")));
        }
    }

    private void LoadOpeningsReal() {
        var openingsRealViewModels = GetOpeningsRealArViewModels(_revitRepository.GetRealOpeningsAr());
        OpeningsReal.Clear();
        foreach(var openingReal in openingsRealViewModels) {
            OpeningsReal.Add(openingReal);
        }

        var uniqueOpenings = _revitRepository.GetFamilyInstances(
            RevitRepository.ArUniqueFamilyName,
            BuiltInCategory.OST_Windows);
        foreach(var item in uniqueOpenings) {
            OpeningsReal.Add(new OpeningRealArUniqueViewModel(item, _localization.GetLocalizedString("AllOpeningStatus.Unique")));
        }

        var ventBlocks = _revitRepository.GetFamilyInstances(
            RevitRepository.VentBlockArFamilyName,
            RevitRepository.VentBlockCategory);
        foreach(var item in ventBlocks) {
            OpeningsReal.Add(
                new VentBlockArViewModel(
                    item,
                    _localization.GetLocalizedString("AllOpeningStatus.VentBlock")));
        }

        OnPropertyChanged(nameof(ShowOpeningsReal));
    }

    /// <summary>
    /// Возвращает коллекцию моделей представления для входящих заданий на отверстия из ВИС
    /// </summary>
    /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
    private ICollection<OpeningMepTaskIncomingViewModel> GetOpeningsMepIncomingTasksViewModels(
        ICollection<OpeningMepTaskIncoming> incomingTasks) {
        using var pb = ProgressDialogFactory.CreateDialog();
        pb.StepValue = _constantsProvider.ProgressBarStepLarge;
        pb.DisplayTitleFormat = _localization.GetLocalizedString("Progress.TaskAnalysis");
        var progress = pb.CreateProgress();
        pb.MaxValue = incomingTasks.Count;
        var ct = pb.CreateCancellationToken();
        pb.Show();

        int i = 0;
        var incomingTasksViewModels = new HashSet<OpeningMepTaskIncomingViewModel>();
        var infoUpdater = _resolutionRoot.Get<IOpeningInfoUpdater<IOpeningTaskIncoming>>();
        foreach(var incomingTask in incomingTasks) {
            ct.ThrowIfCancellationRequested();
            progress.Report(i);
            infoUpdater.UpdateInfo(incomingTask);
            incomingTasksViewModels.Add(new OpeningMepTaskIncomingViewModel(incomingTask, _localization));
            i++;
        }

        return incomingTasksViewModels;
    }

    /// <summary>
    /// Возвращает коллекцию моделей представления чистовых отверстий, размещенных в активном документа АР
    /// </summary>
    /// <param name="openingsReal">Чистовые отверстия, размещенные в активном документе АР</param>
    private ICollection<OpeningRealArViewModel> GetOpeningsRealArViewModels(ICollection<OpeningRealAr> openingsReal) {
        using var pb = ProgressDialogFactory.CreateDialog();
        pb.StepValue = _constantsProvider.ProgressBarStepSmall;
        pb.DisplayTitleFormat = _localization.GetLocalizedString("Progress.OpeningAnalysis");
        var progress = pb.CreateProgress();
        pb.MaxValue = openingsReal.Count;
        var ct = pb.CreateCancellationToken();
        pb.Show();

        int i = 0;
        var infoUpdater = _resolutionRoot.Get<IOpeningInfoUpdater<OpeningRealAr>>();
        var openingsRealViewModels = new HashSet<OpeningRealArViewModel>();
        foreach(var openingReal in openingsReal) {
            ct.ThrowIfCancellationRequested();
            progress.Report(i);
            infoUpdater.UpdateInfo(openingReal);
            openingsRealViewModels.Add(new OpeningRealArViewModel(openingReal, _localization));
            i++;
        }

        return openingsRealViewModels;
    }
}
