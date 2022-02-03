using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelInfoViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string elementInWallName;
        private ElementId _lintelId;
        private ElementId _elementInWallId;
        private string wallTypeName;
        private string _level;
        private ElementInWallKind _elementInWallKind;

        public LintelInfoViewModel() {

        }

        public LintelInfoViewModel(RevitRepository revitRepository, FamilyInstance lintel, FamilyInstance elementInWall) {
            this._revitRepository = revitRepository;
            LintelId = lintel.Id;
            ElementInWallId = elementInWall.Id;
            WallTypeName = elementInWall.Host.Name;
            ElementInWallName = elementInWall.Name;
            Level = _revitRepository.GetElementById(elementInWall.LevelId)?.Name;
            if(elementInWall.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id) {
                ElementInWallKind = ElementInWallKind.Door;
            } else if (elementInWall.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id) {
                if(elementInWall.Symbol.Name.Contains("Отверстие")){
                    ElementInWallKind = ElementInWallKind.Opening;
                } else {
                    ElementInWallKind = ElementInWallKind.Window;
                }
            }

        }

        public string ElementInWallName {
            get => elementInWallName;
            set => this.RaiseAndSetIfChanged(ref elementInWallName, value);
        }
        public string WallTypeName {
            get => wallTypeName;
            set => this.RaiseAndSetIfChanged(ref wallTypeName, value);
        }

        public string Level {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        public ElementInWallKind ElementInWallKind { 
            get => _elementInWallKind; 
            set => this.RaiseAndSetIfChanged(ref _elementInWallKind, value); 
        }

        public ElementId LintelId {
            get => _lintelId;
            set => this.RaiseAndSetIfChanged(ref _lintelId, value);
        }

        public ElementId ElementInWallId {
            get => _elementInWallId;
            set => this.RaiseAndSetIfChanged(ref _elementInWallId, value);
        }
    }

    public enum ElementInWallKind {
        [Description("Двери")]
        Door,
        [Description("Окна")]
        Window,
        [Description("Отверстия")]
        Opening
    }
}
