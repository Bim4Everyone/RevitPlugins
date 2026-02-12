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

using Ninject;
using Ninject.Syntax;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.ViewModels.Navigator;
/// <summary>
/// Модель представления окна для просмотра исходящих заданий на отверстия в файле инженера
/// </summary>
internal class NavigatorMepViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IConstantsProvider _constantsProvider;
    private readonly ILocalizationService _localization;
    private readonly IResolutionRoot _resolutionRoot;

    public NavigatorMepViewModel(
        Models.Configs.OpeningConfig openingConfig,
        RevitRepository revitRepository,
        IResolutionRoot resolutionRoot,
        IConstantsProvider constantsProvider,
        ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));

        ConfigName = openingConfig.Name;
        OpeningsMepTaskOutcoming = [];

        SelectCommand = RelayCommand.Create<ISelectorAndHighlighter>(SelectElement, CanSelect);
        RenewCommand = RelayCommand.Create(Renew);
        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ObservableCollection<IOpeningMepTaskOutcomingViewModel> OpeningsMepTaskOutcoming { get; }

    public string ConfigName { get; }

    public ICommand SelectCommand { get; }

    public ICommand RenewCommand { get; }

    public ICommand LoadViewCommand { get; }


    private void SelectElement(ISelectorAndHighlighter p) {
        _revitRepository.SelectAndShowElement(p);
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


    private void LoadView() {
        var outcomingTasks = _revitRepository.GetOpeningsMepTasksOutcoming();
        var openingTaskOutcomingViewModels = GetMepTaskOutcomingViewModels(outcomingTasks);

        OpeningsMepTaskOutcoming.Clear();
        foreach(var item in openingTaskOutcomingViewModels) {
            OpeningsMepTaskOutcoming.Add(item);
        }

        var uniqueTasks = _revitRepository.GetOpeningsOutcomingUnique(
            RevitRepository.MepUniqueFamilyName,
            BuiltInCategory.OST_GenericModel);
        foreach(var item in uniqueTasks) {
            OpeningsMepTaskOutcoming.Add(
                new OpeningMepTaskOutcomingUniqueViewModel(item, _localization.GetLocalizedString("Unique")));
        }
    }

    private ICollection<OpeningMepTaskOutcomingViewModel> GetMepTaskOutcomingViewModels(
        ICollection<OpeningMepTaskOutcoming> outcomingTasks) {

        var service = _resolutionRoot.Get<IOpeningInfoUpdater<OpeningMepTaskOutcoming>>();

        var openingTaskOutcomingViewModels = new List<OpeningMepTaskOutcomingViewModel>();

        using(var pb = GetPlatformService<IProgressDialogService>()) {
            pb.StepValue = _constantsProvider.ProgressBarStepLarge;
            pb.DisplayTitleFormat = "Анализ заданий... [{0}\\{1}]";
            var progress = pb.CreateProgress();
            pb.MaxValue = outcomingTasks.Count;
            var ct = pb.CreateCancellationToken();
            pb.Show();

            int i = 0;
            foreach(var outcomingTask in outcomingTasks) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i);
                service.UpdateInfo(outcomingTask);
                openingTaskOutcomingViewModels.Add(new OpeningMepTaskOutcomingViewModel(outcomingTask));
                i++;
            }
        }
        return openingTaskOutcomingViewModels;
    }
}
