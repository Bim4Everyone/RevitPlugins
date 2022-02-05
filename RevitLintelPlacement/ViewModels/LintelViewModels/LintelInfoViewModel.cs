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
        private string _elementInWallName;
        private ElementId _lintelId;
        private ElementId _elementInWallId;
        private string _wallTypeName;
        private string _level;
        private ElementInWallKind _elementInWallKind;

        public LintelInfoViewModel() {

        }

        public LintelInfoViewModel(RevitRepository revitRepository, FamilyInstance lintel, FamilyInstance elementInWall) {
            this._revitRepository = revitRepository;
            if(elementInWall != null) {
                ElementInWall = elementInWall;
                ElementInWallId = elementInWall.Id;
                ElementInWallName = elementInWall.Name;
                WallTypeName = elementInWall.Host.Name;
                if(elementInWall.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id) {
                    ElementInWallKind = ElementInWallKind.Door;
                } else if(elementInWall.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id) {
                    ElementInWallKind = elementInWall.Symbol.Name.Contains("Отверстие") ? 
                        ElementInWallKind.Opening : 
                        ElementInWallKind.Window;
                }
            } else {
                ElementInWallKind = ElementInWallKind.None;
            }
            Lintel = lintel;
            LintelId = lintel.Id;
            Level = _revitRepository.GetElementById(lintel.LevelId)?.Name;
        }

        public string ElementInWallName {
            get => _elementInWallName;
            set => this.RaiseAndSetIfChanged(ref _elementInWallName, value);
        }

        public string WallTypeName {
            get => _wallTypeName;
            set => this.RaiseAndSetIfChanged(ref _wallTypeName, value);
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

        public FamilyInstance Lintel { get; set; }
        public FamilyInstance ElementInWall { get; set; }
    }

    public enum ElementInWallKind {
        [Description("Двери")]
        Door,
        [Description("Окна")]
        Window,
        [Description("Отверстия")]
        Opening, 
        [Description("Без элемента")]
        None
    }
}
