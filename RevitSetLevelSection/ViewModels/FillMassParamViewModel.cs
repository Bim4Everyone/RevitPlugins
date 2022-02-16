
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillMassParamViewModel : FillParamViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOptions;

        private string _paramValue;
        private RevitParam _revitParam;
        private string _errorText;

        public FillMassParamViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
        }

        public RevitParam RevitParam {
            get => _revitParam;
            set {
                this.RaiseAndSetIfChanged(ref _revitParam, value);
                ParamValue = (string) _revitRepository.ProjectInfo.GetParamValueOrDefault(RevitParam);
            }
        }

        public string Name => $"Заполнить \"{RevitParam.Name}\"";

        public string ParamValue {
            get => _paramValue;
            set => this.RaiseAndSetIfChanged(ref _paramValue, value);
        }

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorText);

        public DesignOptionsViewModel DesignOptions {
            get => _designOptions;
            set {
                this.RaiseAndSetIfChanged(ref _designOptions, value);
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

            if(DesignOptions == null) {
                return "Выберите вариант с формообразующими.";
            }

            return DesignOptions.CountMassElements == 0
                ? $"В выбранном варианте \"{DesignOptions.Name}\" нет формообразующих."
                : null;
        }

        public override void UpdateElements(bool fromRevitParam) {
            if(fromRevitParam) {
                _revitRepository.UpdateElements(RevitParam, ParamValue);
            } else {
                _revitRepository.UpdateElements(RevitParam, DesignOptions.GetMassObjects());
            }
        }

        private bool HasNotRussianLetters() {
            if(DesignOptions == null) {
                return false;
            }
            
            return DesignOptions.GetMassObjects()
                .Any(item => HasNotRussianLetters((string) item.GetParamValueOrDefault(RevitParam)));
        }

        private bool HasNotRussianLetters(string text) {
            if(string.IsNullOrEmpty(text)) {
                return false;
            }

            return !Regex.IsMatch(text, @"^[А-Яа-я0-9,\s]+$");
        }
    }
}