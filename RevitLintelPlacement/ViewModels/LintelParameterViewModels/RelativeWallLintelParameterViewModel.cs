
using System;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RelativeWallLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private double _relationValue;
        private string _name;
        private string _wallParameterName;

        public double RelationValue {
            get => _relationValue;
            set => this.RaiseAndSetIfChanged(ref _relationValue, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string WallParameterName {
            get => _wallParameterName;
            set => this.RaiseAndSetIfChanged(ref _wallParameterName, value);
        }

        public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            if (elementInWall.Host==null || !(elementInWall.Host is Wall wall))
                throw new ArgumentNullException(nameof(elementInWall), "Элемент не находится в стене");

            //TODO: разные версии Revit
            lintel.SetParamValue(Name, wall.Width*RelationValue);

            //тут всегда половина толщины стены (поиск не по названию параметра)


        }
    }


}
