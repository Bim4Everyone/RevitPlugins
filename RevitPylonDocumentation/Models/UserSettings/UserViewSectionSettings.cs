﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings {
    class UserViewSectionSettings : BaseViewModel {

        private string _generalViewPrefixTemp = "";
        private string _generalViewSuffixTemp = "";
        private string _generalViewPerpendicularPrefixTemp = "Пилон ";
        private string _generalViewPerpendicularSuffixTemp = "_Боковой";
        private string _generalViewTemplateNameTemp = "!Новый РАЗРЕЗ";
        private string _generalViewXOffsetTemp = "200";
        private string _generalViewYTopOffsetTemp = "2300";
        private string _generalViewYBottomOffsetTemp = "200";

        private string _transverseViewFirstPrefixTemp = "";
        private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
        private string _transverseViewSecondPrefixTemp = "";
        private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
        private string _transverseViewThirdPrefixTemp = "";
        private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";
        private string _transverseViewTemplateNameTemp = "!Новый РАЗРЕЗ";
        private string _transverseViewXOffsetTemp = "200";
        private string _transverseViewYOffsetTemp = "200";

        private string _viewFamilyTypeNameTemp = "РАЗРЕЗ_Без номера листа";

        public UserViewSectionSettings(MainViewModel mainViewModel) {

            ViewModel = mainViewModel;
        }

        public MainViewModel ViewModel { get; set; }

        public string GeneralViewPrefix { get; set; }
        public string GeneralViewPrefixTemp {
            get => _generalViewPrefixTemp;
            set => RaiseAndSetIfChanged(ref _generalViewPrefixTemp, value);
        }

        public string GeneralViewSuffix { get; set; }
        public string GeneralViewSuffixTemp {
            get => _generalViewSuffixTemp;
            set => RaiseAndSetIfChanged(ref _generalViewSuffixTemp, value);
        }

        public string GeneralViewPerpendicularPrefix { get; set; }
        public string GeneralViewPerpendicularPrefixTemp {
            get => _generalViewPerpendicularPrefixTemp;
            set => RaiseAndSetIfChanged(ref _generalViewPerpendicularPrefixTemp, value);
        }

        public string GeneralViewPerpendicularSuffix { get; set; }
        public string GeneralViewPerpendicularSuffixTemp {
            get => _generalViewPerpendicularSuffixTemp;
            set => RaiseAndSetIfChanged(ref _generalViewPerpendicularSuffixTemp, value);
        }

        public string GeneralViewTemplateName { get; set; }
        public string GeneralViewTemplateNameTemp {
            get => _generalViewTemplateNameTemp;
            set => RaiseAndSetIfChanged(ref _generalViewTemplateNameTemp, value);
        }

        public string GeneralViewXOffset { get; set; }
        public string GeneralViewXOffsetTemp {
            get => _generalViewXOffsetTemp;
            set => RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
        }

        public string GeneralViewYTopOffset { get; set; }
        public string GeneralViewYTopOffsetTemp {
            get => _generalViewYTopOffsetTemp;
            set => RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
        }

        public string GeneralViewYBottomOffset { get; set; }
        public string GeneralViewYBottomOffsetTemp {
            get => _generalViewYBottomOffsetTemp;
            set => RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
        }


        public string TransverseViewFirstPrefix { get; set; }
        public string TransverseViewFirstPrefixTemp {
            get => _transverseViewFirstPrefixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewFirstPrefixTemp, value);
        }

        public string TransverseViewFirstSuffix { get; set; }
        public string TransverseViewFirstSuffixTemp {
            get => _transverseViewFirstSuffixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewFirstSuffixTemp, value);
        }


        public string TransverseViewSecondPrefix { get; set; }
        public string TransverseViewSecondPrefixTemp {
            get => _transverseViewSecondPrefixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewSecondPrefixTemp, value);
        }

        public string TransverseViewSecondSuffix { get; set; }
        public string TransverseViewSecondSuffixTemp {
            get => _transverseViewSecondSuffixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewSecondSuffixTemp, value);
        }

        public string TransverseViewThirdPrefix { get; set; }
        public string TransverseViewThirdPrefixTemp {
            get => _transverseViewThirdPrefixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewThirdPrefixTemp, value);
        }

        public string TransverseViewThirdSuffix { get; set; }
        public string TransverseViewThirdSuffixTemp {
            get => _transverseViewThirdSuffixTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewThirdSuffixTemp, value);
        }

        public string TransverseViewTemplateName { get; set; }
        public string TransverseViewTemplateNameTemp {
            get => _transverseViewTemplateNameTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewTemplateNameTemp, value);
        }

        public string TransverseViewXOffset { get; set; }
        public string TransverseViewXOffsetTemp {
            get => _transverseViewXOffsetTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
        }

        public string TransverseViewYOffset { get; set; }
        public string TransverseViewYOffsetTemp {
            get => _transverseViewYOffsetTemp;
            set => RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
        }

        public string ViewFamilyTypeName { get; set; }
        public string ViewFamilyTypeNameTemp {
            get => _viewFamilyTypeNameTemp;
            set => RaiseAndSetIfChanged(ref _viewFamilyTypeNameTemp, value);
        }

        public void ApplyViewSectionsSettings() {

            GeneralViewPrefix = GeneralViewPrefixTemp;
            GeneralViewSuffix = GeneralViewSuffixTemp;
            GeneralViewPerpendicularPrefix = GeneralViewPerpendicularPrefixTemp;
            GeneralViewPerpendicularSuffix = GeneralViewPerpendicularSuffixTemp;

            GeneralViewTemplateName = GeneralViewTemplateNameTemp;
            GeneralViewXOffset = GeneralViewXOffsetTemp;
            GeneralViewYTopOffset = GeneralViewYTopOffsetTemp;
            GeneralViewYBottomOffset = GeneralViewYBottomOffsetTemp;

            TransverseViewFirstPrefix = TransverseViewFirstPrefixTemp;
            TransverseViewFirstSuffix = TransverseViewFirstSuffixTemp;
            TransverseViewSecondPrefix = TransverseViewSecondPrefixTemp;
            TransverseViewSecondSuffix = TransverseViewSecondSuffixTemp;
            TransverseViewThirdPrefix = TransverseViewThirdPrefixTemp;
            TransverseViewThirdSuffix = TransverseViewThirdSuffixTemp;

            TransverseViewTemplateName = TransverseViewTemplateNameTemp;
            TransverseViewXOffset = TransverseViewXOffsetTemp;
            TransverseViewYOffset = TransverseViewYOffsetTemp;

            ViewFamilyTypeName = ViewFamilyTypeNameTemp;
        }

        public void CheckViewSectionsSettings() {

            int temp;
            if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewXOffset, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по X некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewYTopOffset, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.GeneralViewYBottomOffset, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
            }

            if(!int.TryParse(ViewModel.ViewSectionSettings.TransverseViewXOffset, out temp)) {
                ViewModel.ErrorText = "Значение отступа поперечного вида по X некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.TransverseViewYOffsetTemp, out temp)) {
                ViewModel.ErrorText = "Значение отступа поперечного вида по Y некорректно";
            }
        }
    }
}
