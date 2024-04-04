using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class DesignTypeInfoVM : BaseViewModel {
        private double _concreteVolume = 0;
        private double _rebarMass = 0;
        private double _rebarCoef = 0;

        public DesignTypeInfoVM(string typeName, string docPackage, bool aboveZero) {
            TypeName = typeName;
            DocPackage = docPackage;
            AboveZero = aboveZero;
        }

        /// <summary>
        /// "обр_ФОП_Марка ведомости расхода"
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// "обр_ФОП_Раздел проекта"
        /// </summary>
        public string DocPackage { get; set; }

        /// <summary>
        /// "обр_ФОП_Орг. уровень"
        /// </summary>
        public bool AboveZero { get; set; }

        public List<Element> Elements { get; set; } = new List<Element>();
        public List<Element> Rebars { get; set; } = new List<Element>();


        public double ConcreteVolume {
            get => _concreteVolume;
            set => this.RaiseAndSetIfChanged(ref _concreteVolume, value);
        }
        public double RebarMass {
            get => _rebarMass;
            set => this.RaiseAndSetIfChanged(ref _rebarMass, value);
        }

        public double RebarCoef {
            get => _rebarCoef;
            set => this.RaiseAndSetIfChanged(ref _rebarCoef, value);
        }

        public bool FormParamsChecked { get; set; } = false;
        public bool RebarParamsChecked { get; set; } = false;


        public bool HasErrors { get; set; } = false;

        public void AddItem(Element elem) {

            if(elem.Category.GetBuiltInCategory() == BuiltInCategory.OST_Rebar) {
                Rebars.Add(elem);
            } else {
                Elements.Add(elem);
            }
        }
    }
}
