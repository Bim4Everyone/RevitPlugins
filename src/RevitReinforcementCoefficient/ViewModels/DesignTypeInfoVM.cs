using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class DesignTypeInfoVM : BaseViewModel {
        private double _concreteVolume = 0;
        private double _rebarMass = 0;
        private double _rebarCoef = 0;
        private bool _isCheck = false;

        public DesignTypeInfoVM(string typeName, string docPackage, bool aboveZero) {
            TypeName = typeName;
            DocPackage = docPackage;
            AboveZero = aboveZero;
        }

        /// <summary>
        /// Указывает выбран ли пользователем данный тип конструкции в интерфейсе для работы
        /// </summary>
        public bool IsCheck {
            get => _isCheck;
            set => this.RaiseAndSetIfChanged(ref _isCheck, value);
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

        /// <summary>
        /// Опалубочные элементы данного типа конструкции
        /// </summary>
        public List<Element> Elements { get; set; } = new List<Element>();

        /// <summary>
        /// Арматурные элементы данного типа конструкции
        /// </summary>
        public List<Element> Rebars { get; set; } = new List<Element>();

        /// <summary>
        /// Суммарны объем опалубочных элементов данного типа конструкции
        /// </summary>
        public double ConcreteVolume {
            get => _concreteVolume;
            set => this.RaiseAndSetIfChanged(ref _concreteVolume, value);
        }

        /// <summary>
        /// Суммарная масса арматурных элементов данного типа конструкции
        /// </summary>
        public double RebarMass {
            get => _rebarMass;
            set => this.RaiseAndSetIfChanged(ref _rebarMass, value);
        }

        /// <summary>
        /// Коэффициент армирования опалубочных элементов данного типа конструкции
        /// </summary>
        public double RebarCoef {
            get => _rebarCoef;
            set => this.RaiseAndSetIfChanged(ref _rebarCoef, value);
        }

        /// <summary>
        /// Указывает, что параметры опалубки уже были проверены плагином
        /// </summary>
        public bool FormParamsChecked { get; set; } = false;

        /// <summary>
        /// Указывает, что параметры арматуры уже были проверены плагином
        /// </summary>
        public bool RebarParamsChecked { get; set; } = false;

        /// <summary>
        /// Указывает, что у элементов данного типа конструкции (опалубки или арматуры) есть ошибки
        /// </summary>
        public bool HasErrors { get; set; } = false;


        /// <summary>
        /// Добавляет элемент в коллекцию опалубочных элементов или арматуры в зависимости от его категории
        /// </summary>
        public void AddItem(Element elem) {

            if(elem.Category.GetBuiltInCategory() == BuiltInCategory.OST_Rebar) {
                Rebars.Add(elem);
            } else {
                Elements.Add(elem);
            }
        }
    }
}
