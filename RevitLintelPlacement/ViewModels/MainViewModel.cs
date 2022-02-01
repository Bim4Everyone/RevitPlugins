using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private RuleCollectionViewModel _rules;
        private LintelCollectionViewModel _lintels;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository, IEnumerable<RulesSettigs> rulesSettings) {
            this._revitRepository = revitRepository;
            Lintels = new LintelCollectionViewModel(_revitRepository);
            Rules = new RuleCollectionViewModel(_revitRepository, rulesSettings);
            InitializeLintels();
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
        }
        public RuleCollectionViewModel Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public LintelCollectionViewModel Lintels {
            get => _lintels;
            set => this.RaiseAndSetIfChanged(ref _lintels, value);
        }

        public ICommand PlaceLintelCommand { get; set; }

        public void PlaceLintels(object p) {
            var elementInWallIds = _revitRepository.GetAllElementsInWall()
                .Select(e => e.Id)
                .ToList();

            foreach(var lintel in Lintels.LintelInfos) {
                if(elementInWallIds.Contains(lintel.ElementInWallId))
                    elementInWallIds.Remove(lintel.ElementInWallId);
            }

            var lintelType = _revitRepository.GetLintelType();

            using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {
                var view3D = _revitRepository.GetView3D();
                foreach(var elementId in elementInWallIds) {
                    var elementInWall = (FamilyInstance) _revitRepository.GetElementById(elementId);
                    var rule = Rules.GetRule(elementInWall);
                    if(rule == null)
                        continue;
                    if(!_revitRepository.CheckUp(view3D, elementInWall))
                        continue;
                    var lintel = _revitRepository.PlaceLintel(lintelType, elementId);
                    rule.LintelParameters.SetTo(lintel, elementInWall);
                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, true)) {
                        lintel.SetParamValue("ОпираниеСправа", 0); //ToDo: параметр
                    }

                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, false)) {
                        lintel.SetParamValue("ОпираниеСлева", 0);
                    }
                    _revitRepository.LockLintel(lintel, elementInWall);
                    var lintelViewModel = new LintelInfoViewModel() {
                        LintelId = lintel.Id,
                        ElementInWallId = elementInWall.Id,
                        WallTypeName = elementInWall.Host.Name,
                        ElementInWallName = elementInWall.Name
                    };
                    Lintels.LintelInfos.Add(lintelViewModel);
                }
                t.Commit();
            }
        }


        //сопоставляются перемычки в группе + перемычки, закрепленные с элементом
        private void InitializeLintels() {
            var lintels = _revitRepository.GetLintels();
            
            var elementLocationDict = _revitRepository
                   .GetAllElementsInWall()
                   .Where(e => e.Location != null)
                   .ToDictionary(e => _revitRepository.GetLocationPoint(e));

            foreach(var lintel in lintels) {
                //перемычки в группе c элементом
                if(lintel.SuperComponent != null) {
                    var elementInWall = (FamilyInstance) _revitRepository.GetElementById(lintel.SuperComponent.Id);
                    AddLintelToCollection(lintel, elementInWall);
                    continue;
                }

                //перемычки, закрепленные с элементом
                var allignElement = _revitRepository.GetDimensionFamilyInstance(lintel);
                if(allignElement != null) {
                    if(allignElement.Category.Name == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Name ||
                        allignElement.Category.Name == _revitRepository.GetCategory(BuiltInCategory.OST_Windows).Name) {
                        var elementInWall = (FamilyInstance) _revitRepository.GetElementById(allignElement.Id);
                        AddLintelToCollection(lintel, elementInWall);
                        continue;
                    }

                }

                //перемычки, геометрически находящиеся над проемом
                var lintelLocation = ((LocationPoint) lintel.Location).Point;
                XYZ nearestXYZ = elementLocationDict.First().Key;
                var minDist = lintelLocation.DistanceTo(nearestXYZ);
                foreach(var elementLocation in elementLocationDict.Keys) {
                    var dist = lintelLocation.DistanceTo(elementLocation);
                    if(dist < minDist) {
                        minDist = dist;
                        nearestXYZ = elementLocation;
                    }
                }

                if(minDist < 1.6) { //TODO: другое число (может, половина ширины проема)
                    var elementInWall = elementLocationDict[nearestXYZ];
                    AddLintelToCollection(lintel, elementInWall);
                    continue;
                }
            }
        }

        private void AddLintelToCollection(FamilyInstance lintel, FamilyInstance elementInWall) {
            var lintelViewModel = new LintelInfoViewModel() {
                LintelId = lintel.Id,
                ElementInWallId = elementInWall.Id,
                WallTypeName = elementInWall.Host.Name,
                ElementInWallName = elementInWall.Name,
                Level = _revitRepository.GetElementById(elementInWall.LevelId)?.Name
            };
            Lintels.LintelInfos.Add(lintelViewModel);
        }

    }
}
