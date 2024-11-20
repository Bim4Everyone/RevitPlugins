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

namespace RevitOpeningPlacement.ViewModels.Navigator {
    /// <summary>
    /// Модель представления окна для просмотра исходящих заданий на отверстия в файле инженера
    /// </summary>
    internal class MepNavigatorForOutcomingTasksViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IConstantsProvider _constantsProvider;
        private readonly IResolutionRoot _resolutionRoot;
        private OpeningMepTaskOutcomingViewModel _selectedOpeningMepTaskOutcoming;


        public MepNavigatorForOutcomingTasksViewModel(
            Models.Configs.OpeningConfig openingConfig,
            RevitRepository revitRepository,
            IResolutionRoot resolutionRoot,
            IConstantsProvider constantsProvider) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
            _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));

            ConfigName = openingConfig.Name;
            OpeningsMepTaskOutcoming = new ObservableCollection<OpeningMepTaskOutcomingViewModel>();
            OpeningsMepTasksOutcomingViewSource = new CollectionViewSource() { Source = OpeningsMepTaskOutcoming };

            SelectCommand = RelayCommand.Create<ISelectorAndHighlighter>(SelectElement, CanSelect);
            RenewCommand = RelayCommand.Create(Renew);
            LoadViewCommand = RelayCommand.Create(LoadView);
        }


        public ObservableCollection<OpeningMepTaskOutcomingViewModel> OpeningsMepTaskOutcoming { get; }

        public CollectionViewSource OpeningsMepTasksOutcomingViewSource { get; private set; }

        public OpeningMepTaskOutcomingViewModel SelectedOpeningMepTaskOutcoming {
            get => _selectedOpeningMepTaskOutcoming;
            set => RaiseAndSetIfChanged(ref _selectedOpeningMepTaskOutcoming, value);
        }

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
            Action action = () => {
                var command = new GetOpeningTasksCmd();
                command.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }


        private void LoadView() {
            var outcomingTasks = _revitRepository.GetOpeningsMepTasksOutcoming();
            IList<ElementId> outcomingTasksIds = outcomingTasks.Select(task => task.Id).ToList();
            var mepElementsIds = _revitRepository.GetMepElementsIds();
            var openingTaskOutcomingViewModels = GetMepTaskOutcomingViewModels(outcomingTasks);

            OpeningsMepTaskOutcoming.Clear();
            foreach(var item in openingTaskOutcomingViewModels) {
                OpeningsMepTaskOutcoming.Add(item);
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
}
