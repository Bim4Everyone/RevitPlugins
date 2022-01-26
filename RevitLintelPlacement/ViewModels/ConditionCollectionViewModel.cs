using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class ConditionCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<IConditionViewModel> _conditions;

        public ConditionCollectionViewModel() {

        }

        public ConditionCollectionViewModel(RevitRepository revitRepository, IEnumerable<ConditionSetting> conditionSettings) {

            this._revitRepository = revitRepository;
            Conditions = new ObservableCollection<IConditionViewModel>(GetConditionViewModels(conditionSettings));
        }

        public ObservableCollection<IConditionViewModel> Conditions {
            get => _conditions;
            set => this.RaiseAndSetIfChanged(ref _conditions, value);
        }
        
        public bool CheckConditions(FamilyInstance elementInWall) {
            return Conditions.All(c => c.Check(elementInWall));
        }

        private IEnumerable<IConditionViewModel> GetConditionViewModels(IEnumerable<ConditionSetting> conditionSettings) {
            foreach(var cs in conditionSettings) {
                switch(cs.ConditionType) {
                    case ConditionType.OpeningWidth: {
                        yield return new OpeningWidthConditionViewModel() {
                            MinWidth = cs.OpeningWidthMin,
                            MaxWidth = cs.OpeningWidthMax
                        };
                        break;
                    }
                    case ConditionType.WallMaterialClasses: {
                        yield return new MaterialClassesConditionViewModel(_revitRepository) {
                            MaterialClassConditions = new ObservableCollection<MaterialClassConditionViewModel>(
                            cs.WallMaterialClasses.Select(mc => new MaterialClassConditionViewModel() {
                                Name = mc,
                                IsChecked = true
                            }))
                        }; //TODO: надо еще добавлять кроме выбранных классов материалов еще те, которые есть в проекте (аналогично, с типами стен и материалами)
                        break;
                    }
                    case ConditionType.ExclusionWallTypes: {
                        yield return new ExclusionWallTypesConditionViewModel() {
                            WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                            cs.ExclusionWallTypes.Select(ewt => new WallTypeConditionViewModel() {
                                Name = ewt,
                                IsChecked = true
                            }))
                        };
                        break;
                    }
                    case ConditionType.WallTypes: {
                        yield return new WallTypesConditionViewModel() {
                            WallTypes = new ObservableCollection<WallTypeConditionViewModel>(
                            cs.WallTypes.Select(wt => new WallTypeConditionViewModel() {
                                Name = wt,
                                IsChecked = true
                            }))
                        };
                        break;
                    }
                    case ConditionType.WallMaterials: {
                        yield return new MaterialConditionsViewModel(_revitRepository) {
                            MaterialConditions = new ObservableCollection<MaterialConditionViewModel>(
                            cs.WallMaterials.Select(m => new MaterialConditionViewModel() {
                                Name = m,
                                IsChecked = true
                            }))
                        };
                        break;
                    }

                    default:
                    throw new ArgumentException($"Следующий тип условия: {cs.ConditionType}, не найден.");
                }

            }
        }
    }
}
