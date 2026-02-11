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
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления окна для просмотра входящих заданий на отверстия от архитектора в файле конструктора
/// </summary>
internal class NavigatorKrViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly OpeningRealsKrConfig _config;
    private readonly IConstantsProvider _constantsProvider;

    public NavigatorKrViewModel(
        RevitRepository revitRepository,
        OpeningRealsKrConfig config,
        IConstantsProvider constantsProvider) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        OpeningsTasksIncoming = [];
        OpeningsTasksIncomingViewSource = new CollectionViewSource() { Source = OpeningsTasksIncoming };

        OpeningsReal = [];
        OpeningsRealViewSource = new CollectionViewSource() { Source = OpeningsReal };

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

    // Входящие задания на отверстия из АР/ВИС
    public ObservableCollection<IOpeningTaskIncomingToKrViewModel> OpeningsTasksIncoming { get; }

    public CollectionViewSource OpeningsTasksIncomingViewSource { get; }

    // Чистовые отверстия из активного документа КР
    public ObservableCollection<IOpeningRealKrViewModel> OpeningsReal { get; }

    public bool ShowOpeningsReal => OpeningsReal.Count > 0;

    public CollectionViewSource OpeningsRealViewSource { get; }

    public ICommand LoadViewCommand { get; }

    public ICommand SelectCommand { get; }

    public ICommand RenewCommand { get; }

    public ICommand PlaceRealOpeningBySingleTaskCommand { get; }

    public ICommand PlaceOneRealOpeningByManyTasksCommand { get; }

    public ICommand PlaceManyRealOpeningsByManyTasksCommand { get; }

    public ICommand PlaceManyRealOpeningsByManyTasksInManyHostsCommand { get; }

    private void SelectElement(ISelectorAndHighlighter p) {
        _revitRepository.SelectAndShowElement(p);
    }

    private bool CanSelect(ISelectorAndHighlighter p) {
        return p != null;
    }

    private void Renew() {
        void action() {
            var cmd = new GetOpeningTasksCmd();
            cmd.ExecuteCommand(_revitRepository.UIApplication);
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
        var mode = _config.PlacementType;
        switch(mode) {
            case OpeningRealKrPlacementType.PlaceByMep:
                LoadIncomingMepTasks();
                break;
            case OpeningRealKrPlacementType.PlaceByAr:
                LoadIncomingArTasks();
                break;
            default:
                throw new InvalidOperationException($"Режим обработки заданий для КР: '{mode}' не поддерживается.");
        }
    }

    private void LoadIncomingArTasks() {
        var incomingTasks = _revitRepository.GetOpeningsArTasksIncoming();
        var realOpenings = _revitRepository.GetRealOpeningsKr();
        var constructureElementsIds = _revitRepository.GetConstructureElementsIds();
        ICollection<IConstructureLinkElementsProvider> arLinks = _revitRepository
            .GetSelectedRevitLinks()
            .Select(link => new ConstructureLinkElementsProvider(_revitRepository, link)
                as IConstructureLinkElementsProvider)
            .ToArray();

        var incomingTasksViewModels = GetOpeningsArIncomingTasksViewModels(
            incomingTasks,
            realOpenings,
            constructureElementsIds);
        UpdateOpeningsTasksIncoming(incomingTasksViewModels);

        var openingsRealViewModels = GetOpeningsRealKrViewModels(
            realOpenings,
            (OpeningRealKr opening) => { opening.UpdateStatus(arLinks); });
        UpdateOpeningsReal(openingsRealViewModels);
    }

    private void LoadIncomingMepTasks() {
        var incomingTasks = _revitRepository.GetOpeningsMepTasksIncoming();
        var realOpenings = _revitRepository.GetRealOpeningsKr();
        var constructureElementsIds = _revitRepository.GetConstructureElementsIds();
        ICollection<IMepLinkElementsProvider> mepLinks = _revitRepository
            .GetSelectedRevitLinks()
            .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
            .ToArray();

        var incomingTasksViewModels = GetOpeningsMepIncomingTasksViewModels(
                incomingTasks,
                realOpenings.ToArray<IOpeningReal>(),
                constructureElementsIds)
            .ToArray<IOpeningTaskIncomingToKrViewModel>();
        UpdateOpeningsTasksIncoming(incomingTasksViewModels);

        var openingsRealViewModels = GetOpeningsRealKrViewModels(
            realOpenings,
            (OpeningRealKr opening) => { opening.UpdateStatus(mepLinks); });
        UpdateOpeningsReal(openingsRealViewModels);
    }

    private void UpdateOpeningsTasksIncoming(ICollection<IOpeningTaskIncomingToKrViewModel> incomingTasks) {
        OpeningsTasksIncoming.Clear();
        foreach(var incomingTask in incomingTasks) {
            OpeningsTasksIncoming.Add(incomingTask);
        }
    }

    private void UpdateOpeningsReal(ICollection<OpeningRealKrViewModel> openingsReal) {
        OpeningsReal.Clear();
        foreach(var openingReal in openingsReal) {
            OpeningsReal.Add(openingReal);
        }

        OnPropertyChanged(nameof(ShowOpeningsReal));
    }

    /// <summary>
    /// Возвращает коллекцию моделей представления для входящих заданий на отверстия из АР
    /// </summary>
    /// <param name="incomingTasks">Входящие задания на отверстия из связей</param>
    /// <param name="realOpenings">Чистовые отверстия из текущего документа</param>
    /// <param name="constructureElementsIds">Элементы конструкций из текущего документа</param>
    private ICollection<IOpeningTaskIncomingToKrViewModel> GetOpeningsArIncomingTasksViewModels(
        ICollection<OpeningArTaskIncoming> incomingTasks,
        ICollection<OpeningRealKr> realOpenings,
        ICollection<ElementId> constructureElementsIds) {
        var incomintTasksViewModels = new HashSet<IOpeningTaskIncomingToKrViewModel>();

        using(var pb = GetPlatformService<IProgressDialogService>()) {
            pb.StepValue = _constantsProvider.ProgressBarStepSmall;
            pb.DisplayTitleFormat = "Анализ заданий... [{0}]\\[{1}]";
            var progress = pb.CreateProgress();
            pb.MaxValue = incomingTasks.Count;
            var ct = pb.CreateCancellationToken();
            pb.Show();

            int i = 0;
            foreach(var incomingTask in incomingTasks) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i);
                incomingTask.UpdateStatusAndHost(realOpenings, constructureElementsIds);
                incomintTasksViewModels.Add(new OpeningArTaskIncomingViewModel(incomingTask));
                i++;
            }
        }

        return incomintTasksViewModels;
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
    /// Возвращает коллекцию моделей представления чистовых отверстий, размещенных в активном документе КР
    /// </summary>
    /// <param name="openingsReal">Чистовые отверстия, размещенные в активном документе КР</param>
    /// <param name="updateStatus">Делегат для обновления статусов размещенных чистовых отверстий КР</param>
    private ICollection<OpeningRealKrViewModel> GetOpeningsRealKrViewModels(
        ICollection<OpeningRealKr> openingsReal,
        Action<OpeningRealKr> updateStatus) {
        var openingsRealViewModels = new HashSet<OpeningRealKrViewModel>();

        using(var pb = GetPlatformService<IProgressDialogService>()) {
            pb.StepValue = _constantsProvider.ProgressBarStepSmall;
            pb.DisplayTitleFormat = "Анализ отверстий... [{0}]\\[{1}]";
            var progress = pb.CreateProgress();
            pb.MaxValue = openingsReal.Count;
            var ct = pb.CreateCancellationToken();
            pb.Show();

            int i = 0;
            foreach(var openingReal in openingsReal) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i);
                updateStatus.Invoke(openingReal);
                openingsRealViewModels.Add(new OpeningRealKrViewModel(openingReal));
                i++;
            }
        }

        return openingsRealViewModels;
    }
}
