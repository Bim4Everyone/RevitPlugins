
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillMassParamViewModel : FillParamViewModel {
        private readonly MainViewModel _mainViewModel;
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOption;

        private string _paramValue;
        private RevitParam _revitParam;
        private string _errorText;

        public FillMassParamViewModel(MainViewModel mainViewModel, RevitRepository revitRepository) {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }

        public override RevitParam RevitParam {
            get => _revitParam;
            set {
                this.RaiseAndSetIfChanged(ref _revitParam, value);
                ParamValue = (string) _revitRepository.ProjectInfo.GetParamValueOrDefault(RevitParam);
            }
        }

        public string Name => $"Перенести \"{RevitParam.Name}\"";

        public string ParamValue {
            get => _paramValue;
            set => this.RaiseAndSetIfChanged(ref _paramValue, value);
        }

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public DesignOptionsViewModel DesignOption {
            get => _designOption;
            set {
                this.RaiseAndSetIfChanged(ref _designOption, value);
                if(HasNotRussianLetters()) {
                    ErrorText =
                        "В данном варианте содержится формообразующий элемент со значением параметра содержащий запрещенные символы.";
                } else {
                    ErrorText = null;
                }
            }
        }

        public string ErrorText {
            get => _errorText;
            set {
                this.RaiseAndSetIfChanged(ref _errorText, value);
                this.RaisePropertyChanged(nameof(HasError));
            }
        }

        public override string GetErrorText(bool fromRevitParam) {
            if(!IsEnabled) {
                return null;
            }

            if(fromRevitParam) {
                return string.IsNullOrEmpty(ParamValue) ? "Заполните значение параметра документа." : null;
            }

            if(DesignOption == null) {
                return "Выберите вариант с формообразующими.";
            }

            return DesignOption.CountMassElements == 0
                ? $"В выбранном варианте \"{DesignOption.Name}\" нет формообразующих."
                : null;
        }

        public override void UpdateElements(bool fromRevitParam) {
            if(fromRevitParam) {
                _revitRepository.UpdateElements(RevitParam, ParamValue);
            } else {
                _revitRepository.UpdateElements(RevitParam, DesignOption.GetMassObjects());
            }
        }

        private bool HasNotRussianLetters() {
            if(DesignOption == null) {
                return false;
            }

            return DesignOption.GetMassObjects()
                .Any(item => HasNotRussianLetters((string) item.GetParamValueOrDefault(RevitParam)));
        }

        private bool HasNotRussianLetters(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }

            return !Regex.IsMatch(text, @"^[А-Яа-я0-9,\s]+$");
        }

        public override ParamSettings GetParamSettings() {
            return new ParamSettings() {
                IsEnabled = IsEnabled, ElementId = DesignOption?.Id, PropertyName = RevitParam.PropertyName
            };
        }

        public override void SetParamSettings(ParamSettings paramSettings) {
            IsEnabled = paramSettings.IsEnabled;
            DesignOption = _mainViewModel.LinkType?.DesignOptions
                               .FirstOrDefault(item => item.Id == paramSettings.ElementId)
                           ?? _mainViewModel.LinkType?.DesignOptions.FirstOrDefault();
        }
    }
}