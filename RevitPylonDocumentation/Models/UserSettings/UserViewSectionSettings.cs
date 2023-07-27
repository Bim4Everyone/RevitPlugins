using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings {
    class UserViewSectionSettings : BaseViewModel {
        public UserViewSectionSettings(MainViewModel mainViewModel) {

            ViewModel = mainViewModel;
        }

        public MainViewModel ViewModel { get; set; }


        private string _generalViewPrefixTemp = "";
        private string _generalViewSuffixTemp = "";
        private string _generalViewPerpendicularPrefixTemp = "Пилон ";
        private string _generalViewPerpendicularSuffixTemp = "_Перпендикулярный";
        private string _generalViewTemplateNameTemp = "КЖ0.2_пилоны_орг.ур.-2";
        private string _generalViewXOffsetTemp = "200";
        private string _generalViewYTopOffsetTemp = "2300";
        private string _generalViewYBottomOffsetTemp = "200";

        private string _transverseViewFirstPrefixTemp = "";
        private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
        private string _transverseViewSecondPrefixTemp = "";
        private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
        private string _transverseViewThirdPrefixTemp = "";
        private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";
        private string _transverseViewTemplateNameTemp = "";
        private string _transverseViewXOffsetTemp = "200";
        private string _transverseViewYOffsetTemp = "200";

        private string _viewFamilyTypeNameTemp = "РАЗРЕЗ_Без номера листа";


        public string GENERAL_VIEW_PREFIX { get; set; } = "";
        public string GENERAL_VIEW_PREFIX_TEMP {
            get => _generalViewPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_SUFFIX { get; set; } = "";
        public string GENERAL_VIEW_SUFFIX_TEMP {
            get => _generalViewSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; } = "Пилон ";
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP {
            get => _generalViewPerpendicularPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewPerpendicularPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; } = "_Перпендикулярный";
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP {
            get => _generalViewPerpendicularSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewPerpendicularSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_TEMPLATE_NAME { get; set; } = "КЖ0.2_пилоны_орг.ур.-2";
        public string GENERAL_VIEW_TEMPLATE_NAME_TEMP {
            get => _generalViewTemplateNameTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewTemplateNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_X_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_X_OFFSET_TEMP {
            get => _generalViewXOffsetTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_Y_TOP_OFFSET { get; set; } = "2300";
        public string GENERAL_VIEW_Y_TOP_OFFSET_TEMP {
            get => _generalViewYTopOffsetTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_Y_BOTTOM_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP {
            get => _generalViewYBottomOffsetTemp;
            set {
                RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_FIRST_PREFIX_TEMP {
            get => _transverseViewFirstPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewFirstPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; } = "_Сеч.1-1";
        public string TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP {
            get => _transverseViewFirstSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewFirstSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_SECOND_PREFIX_TEMP {
            get => _transverseViewSecondPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewSecondPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; } = "_Сеч.2-2";
        public string TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP {
            get => _transverseViewSecondSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewSecondSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_THIRD_PREFIX_TEMP {
            get => _transverseViewThirdPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewThirdPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; } = "_Сеч.3-3";
        public string TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP {
            get => _transverseViewThirdSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewThirdSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_TEMPLATE_NAME { get; set; } = "";
        public string TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP {
            get => _transverseViewTemplateNameTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewTemplateNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_X_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_X_OFFSET_TEMP {
            get => _transverseViewXOffsetTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_Y_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_Y_OFFSET_TEMP {
            get => _transverseViewYOffsetTemp;
            set {
                RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string VIEW_FAMILY_TYPE_NAME { get; set; } = "РАЗРЕЗ_Без номера листа";
        public string VIEW_FAMILY_TYPE_NAME_TEMP {
            get => _viewFamilyTypeNameTemp;
            set {
                RaiseAndSetIfChanged(ref _viewFamilyTypeNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public void ApplyViewSectionsSettings() {

            GENERAL_VIEW_PREFIX = GENERAL_VIEW_PREFIX_TEMP;
            GENERAL_VIEW_SUFFIX = GENERAL_VIEW_SUFFIX_TEMP;
            GENERAL_VIEW_PERPENDICULAR_PREFIX = GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP;
            GENERAL_VIEW_PERPENDICULAR_SUFFIX = GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP;

            GENERAL_VIEW_TEMPLATE_NAME = GENERAL_VIEW_TEMPLATE_NAME_TEMP;
            GENERAL_VIEW_X_OFFSET = GENERAL_VIEW_X_OFFSET_TEMP;
            GENERAL_VIEW_Y_TOP_OFFSET = GENERAL_VIEW_Y_TOP_OFFSET_TEMP;
            GENERAL_VIEW_Y_BOTTOM_OFFSET = GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP;

            TRANSVERSE_VIEW_FIRST_PREFIX = TRANSVERSE_VIEW_FIRST_PREFIX_TEMP;
            TRANSVERSE_VIEW_FIRST_SUFFIX = TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP;
            TRANSVERSE_VIEW_SECOND_PREFIX = TRANSVERSE_VIEW_SECOND_PREFIX_TEMP;
            TRANSVERSE_VIEW_SECOND_SUFFIX = TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP;
            TRANSVERSE_VIEW_THIRD_PREFIX = TRANSVERSE_VIEW_THIRD_PREFIX_TEMP;
            TRANSVERSE_VIEW_THIRD_SUFFIX = TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP;

            TRANSVERSE_VIEW_TEMPLATE_NAME = TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP;
            TRANSVERSE_VIEW_X_OFFSET = TRANSVERSE_VIEW_X_OFFSET_TEMP;
            TRANSVERSE_VIEW_Y_OFFSET = TRANSVERSE_VIEW_Y_OFFSET_TEMP;

            VIEW_FAMILY_TYPE_NAME = VIEW_FAMILY_TYPE_NAME_TEMP;
        }

        public void CheckViewSectionsSettings() {

            int temp;
            if(!int.TryParse(ViewModel.ViewSectionSettings.GENERAL_VIEW_X_OFFSET, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по X некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.GENERAL_VIEW_Y_TOP_OFFSET, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.GENERAL_VIEW_Y_BOTTOM_OFFSET, out temp)) {
                ViewModel.ErrorText = "Значение отступа основного вида по Y сверху некорректно";
            }

            if(!int.TryParse(ViewModel.ViewSectionSettings.TRANSVERSE_VIEW_X_OFFSET, out temp)) {
                ViewModel.ErrorText = "Значение отступа поперечного вида по X некорректно";
            }
            if(!int.TryParse(ViewModel.ViewSectionSettings.TRANSVERSE_VIEW_Y_OFFSET_TEMP, out temp)) {
                ViewModel.ErrorText = "Значение отступа поперечного вида по Y некорректно";
            }
        }
    }
}
