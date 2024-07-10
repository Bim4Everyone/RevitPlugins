using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models.ElementModels;

namespace RevitReinforcementCoefficient.ViewModels {
    internal class DesignTypeVM : BaseViewModel {
        private double _formworkVolume = 0;
        private double _rebarMass = 0;
        private string _rebarCoef = "0";
        private bool _isCheck = false;

        public DesignTypeVM(string typeName, string docPackage, bool aboveZero, ICommonElement firstElement) {
            TypeName = typeName;
            DocPackage = docPackage;
            AboveZero = aboveZero;
            AddItem(firstElement);
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
        public List<FormworkElement> Formworks { get; set; } = new List<FormworkElement>();

        /// <summary>
        /// Арматурные элементы данного типа конструкции
        /// </summary>
        public List<RebarElement> Rebars { get; set; } = new List<RebarElement>();

        /// <summary>
        /// Суммарны объем опалубочных элементов данного типа конструкции
        /// </summary>
        public double FormworkVolume {
            get => _formworkVolume;
            set => this.RaiseAndSetIfChanged(ref _formworkVolume, value);
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
        public string RebarCoef {
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
        /// Указывает, что данный тип конструкции уже рассчитывался ранее в этом сеансе
        /// </summary>
        public bool AlreadyCalculated { get; set; } = false;

        /// <summary>
        /// Добавляет элемент в коллекцию опалубочных элементов или арматуры в зависимости от его класса
        /// </summary>
        public void AddItem(ICommonElement elem) {
            if(elem is RebarElement) {
                Rebars.Add(elem as RebarElement);
            } else {
                Formworks.Add(elem as FormworkElement);
            }
        }

        /// <summary>
        /// Рассчитать объем бетона
        /// </summary>
        public void CalculateFormworks() => FormworkVolume = Calculate(Formworks);

        /// <summary>
        /// Рассчитать массу арматуры
        /// </summary>
        public void CalculateRebars() => RebarMass = Calculate(Rebars);

        /// <summary>
        /// Рассчитывает коэффициент армирования у одного типа конструкции по массе арматуры и объему бетона
        /// </summary>
        public void CalculateRebarCoef() {
            RebarCoef = Math.Round(RebarMass / FormworkVolume).ToString(CultureInfo.GetCultureInfo("ru-Ru"));
            AlreadyCalculated = true;
        }

        /// <summary>
        /// Выполняет вызов метода калькуляции на каждом объекте коллекции
        /// </summary>
        private double Calculate(IEnumerable<ICommonElement> elements) {
            double sum = elements
                .Select(e => e.Calculate())
                .Sum();
            return Math.Round(sum, 2);
        }
    }
}
