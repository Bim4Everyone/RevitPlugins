
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillMassParamViewModel : FillParamViewModel {
        private readonly ParamOption _paramOption;
        private readonly MainViewModel _mainViewModel;
        private readonly RevitRepository _revitRepository;
        private readonly IFillParamFactory _fillParamFactory;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOption;
        
        private string _errorText;
        private string _selectedParamName;

        public FillMassParamViewModel(ParamOption paramOption, MainViewModel mainViewModel, RevitRepository revitRepository, IFillParamFactory fillParamFactory) {
            _paramOption = paramOption;
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _fillParamFactory = fillParamFactory;

            CheckRussianTextCommand = new RelayCommand(CheckRussianText);
            UpdatePartParamNameCommand = new RelayCommand(UpdatePartParamName);
        }

        public ICommand CheckRussianTextCommand { get; }
        public ICommand UpdatePartParamNameCommand { get; }

        public override RevitParam RevitParam => _paramOption.RevitParam;

        public string SelectedParamName {
            get => _selectedParamName;
            set => this.RaiseAndSetIfChanged(ref _selectedParamName, value);
        }

        public string Name => $"Обновить \"{RevitParam.Name}\"";

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public DesignOptionsViewModel DesignOption {
            get => _designOption;
            set => this.RaiseAndSetIfChanged(ref _designOption, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public override string GetErrorText() {
            if(!IsEnabled) {
                return null;
            }

            if(DesignOption == null) {
                return "Выберите вариант с формообразующими.";
            }

            if(DesignOption.CountMassElements == 0) {
                return $"В варианте \"{DesignOption.Name}\" нет формообразующих.";
            }

            if(DesignOption.HasMassIntersect) {
                return $"В варианте \"{DesignOption.Name}\" формообразующие имеют пересечения.";
            }
            
            if(string.IsNullOrEmpty(_mainViewModel.LinkType.BuildPart)) {
                return "Выберите раздел.";
            }

            if(_paramOption.IsRequired) {
                string partParamName = _paramOption.RevitParamName + _mainViewModel.LinkType.BuildPart;
                var paramOption = new ParamOption() {
                    RevitParam = RevitParam, RevitParamName = partParamName
                };
                return DesignOption.GetMassObjects()
                    .All(item => item.IsExistsParamValue(paramOption))
                    ? null
                    : $"У формообразующих не заполнен параметр \"{RevitParam.Name}\".";
            }

            return null;
        }

        public override IFillParam CreateFillParam() {
            string partParamName = _paramOption.RevitParamName + _mainViewModel.LinkType.BuildPart;
            var paramOption = new ParamOption() {
                RevitParam = RevitParam, RevitParamName = partParamName
            };
            return _fillParamFactory.Create(paramOption, 
                DesignOption.DesignOption,
                _mainViewModel.LinkType.GetMassRepository());
        }

        private bool HasNotRussianLetters() {
            if(DesignOption == null) {
                return false;
            }

            string partParamName = _paramOption.RevitParamName + _mainViewModel.LinkType.BuildPart;
            var paramOption = new ParamOption() {
                RevitParam = RevitParam, RevitParamName = partParamName
            };
            return DesignOption.GetMassObjects()
                .Any(item => HasNotRussianLetters(item.GetParamValue<string>(paramOption)));
        }

        private bool HasNotRussianLetters(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }

            return !Regex.IsMatch(text, @"^[А-Яа-я0-9,\s]+$");
        }

        public override ParamSettings GetParamSettings() {
            return new ParamSettings() {
                IsEnabled = IsEnabled, DesignOptionId = DesignOption?.Id, ParamId = RevitParam.Id
            };
        }

        public override void SetParamSettings(ParamSettings paramSettings) {
            IsEnabled = paramSettings.IsEnabled;
            DesignOption = _mainViewModel.LinkType?.DesignOptions
                               .FirstOrDefault(item => item.Id == paramSettings.DesignOptionId)
                           ?? _mainViewModel.LinkType?.DesignOptions.FirstOrDefault();
        }

        private void CheckRussianText(object args) {
            ErrorText = HasNotRussianLetters()
                ? "Найдены формообразующие содержащие запрещенные символы."
                : null;
        }

        private void UpdatePartParamName(object args) {
            var partNames = _mainViewModel.LinkType.GetPartNames(new[] {_paramOption.RevitParamName}).ToArray();
            SelectedParamName = partNames.FirstOrDefault(item => item.Equals(_mainViewModel.LinkType.BuildPart)) ??
                                partNames.FirstOrDefault();
        }
    }
}