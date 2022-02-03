using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelParameterCollectionViewModel : BaseViewModel {
        private ObservableCollection<ILintelParameterViewModel> _lintelParameters;

        public LintelParameterCollectionViewModel() {

        }

        public LintelParameterCollectionViewModel(IEnumerable<LintelParameterSetting> conditionSettingss) {
            LintelParameters = new ObservableCollection<ILintelParameterViewModel>(/*GetLintelParameterViewModels(conditionSettingss)*/);
        }

        public ObservableCollection<ILintelParameterViewModel> LintelParameters {
            get => _lintelParameters;
            set => this.RaiseAndSetIfChanged(ref _lintelParameters, value);
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            foreach(var lintelParameter in LintelParameters) {
                lintelParameter.SetTo(lintel, elementInWall);
            }
        }

        //private IEnumerable<ILintelParameterViewModel> GetLintelParameterViewModels(IEnumerable<LintelParameterSetting> lintelParameterSettings) {
        //    foreach(var lp in lintelParameterSettings) {
        //        switch(lp.LintelParameterType) {
        //            case LintelParameterType.NumberParameter: {
        //                yield return new NumberLintelParameterViewModel() {
        //                    Name = lp.Name,
        //                    Value = lp.NumberValue
        //                };
        //                break;
        //            }
        //            case LintelParameterType.YesNoLintelParameter: {
        //                yield return new YesNoLintelParameterViewModel() {
        //                    Name = lp.Name,
        //                    IsChecked = lp.IsChecked
        //                };
        //                break;
        //            }
        //            case LintelParameterType.RelativeOpeningParameter: {
        //                yield return new RelativeOpeningLintelParameterViewModel() {
        //                    Name = lp.Name,
        //                    RelationValue = lp.RelationValue,
        //                    OpeningParameterName = lp.OpeninigParameterName
        //                };
        //                break;
        //            }
        //            case LintelParameterType.RelativeWallParameter: {
        //                yield return new RelativeWallLintelParameterViewModel() {
        //                    Name = lp.Name,
        //                    RelationValue = lp.RelationValue,
        //                    WallParameterName = lp.WallParameterName
        //                };
        //                break;
        //            }
        //            default:
        //            throw new ArgumentException($"Следующий тип условия: \"{lp.LintelParameterType}\", не найден.");
        //        }

        //    }
        //}
    }
}
