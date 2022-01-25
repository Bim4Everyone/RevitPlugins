using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class ConditionCollectionViewModel : BaseViewModel {
        private ObservableCollection<IConditionViewModel> _conditions;

        public ConditionCollectionViewModel() {

        }

        public ConditionCollectionViewModel(List<ConditionSettingConfig> conditionSettings) {

            Conditions = new ObservableCollection<IConditionViewModel>(MapConditionSettingsToConditionViewModels(conditionSettings));
        }

        public ObservableCollection<IConditionViewModel> Conditions {
            get => _conditions;
            set => this.RaiseAndSetIfChanged(ref _conditions, value);
        }

        
        
        private List<IConditionViewModel> MapConditionSettingsToConditionViewModels(List<ConditionSettingConfig> conditionSettingss) {
            var conditionViewModels = new List<IConditionViewModel>();
            foreach(var cs in conditionSettingss) {
                switch(cs.ConditionType) {
                    case ConditionType.OpeningWidth: {
                        var conditionViewModel = new OpeningWidthConditionViewModel();
                        conditionViewModel.MinWidth = cs.OpeningWidthMin;
                        conditionViewModel.MaxWidth = cs.OpeningWidthMax;
                        conditionViewModels.Add(conditionViewModel);
                        break;
                    }
                    case ConditionType.WallMaterialClasses: {
                        var conditionViewModel = new MaterialClassesConditionViewModel(); //TODO: надо еще добавлять кроме выбранных классов материалов еще те, которые есть в проекте (аналогично, с типами стен и материалами)
                        conditionViewModel.MaterialClassConditions = new ObservableCollection<MaterialClassConditionViewModel>(
                            cs.WallMaterialClasses.Select(mc => new MaterialClassConditionViewModel() {
                                Name = mc,
                                IsChecked = true
                            })); 
                        conditionViewModels.Add(conditionViewModel);
                        break;
                    }
                    case ConditionType.ExclusionWallTypes: {
                        var conditionViewModel = new ExclusionWallTypesConditionViewModel();
                        conditionViewModel.WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                            cs.ExclusionWallTypes.Select(ewt => new WallTypeConditionViewModel() {
                                Name = ewt,
                                IsChecked = true
                            }));
                        conditionViewModels.Add(conditionViewModel);
                        break;
                    }
                    case ConditionType.WallTypes: {
                        var conditionViewModel = new WallTypesConditionViewModel();
                        conditionViewModel.WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                            cs.WallTypes.Select(wt => new WallTypeConditionViewModel() {
                                Name = wt,
                                IsChecked = true
                            }));
                        conditionViewModels.Add(conditionViewModel);
                        break;
                    }

                    default:
                    break;
                }

            }
            return conditionViewModels;
        }
    }
}
