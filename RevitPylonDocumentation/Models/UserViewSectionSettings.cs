using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models
{
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


        public string GENERAL_VIEW_PREFIX { get; set; } = "";
        public string GENERAL_VIEW_PREFIX_TEMP {
            get => _generalViewPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_SUFFIX { get; set; } = "";
        public string GENERAL_VIEW_SUFFIX_TEMP {
            get => _generalViewSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; } = "Пилон ";
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP {
            get => _generalViewPerpendicularPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPerpendicularPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; } = "_Перпендикулярный";
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP {
            get => _generalViewPerpendicularSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPerpendicularSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_TEMPLATE_NAME { get; set; } = "КЖ0.2_пилоны_орг.ур.-2";
        public string GENERAL_VIEW_TEMPLATE_NAME_TEMP {
            get => _generalViewTemplateNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewTemplateNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_X_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_X_OFFSET_TEMP {
            get => _generalViewXOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_Y_TOP_OFFSET { get; set; } = "2300";
        public string GENERAL_VIEW_Y_TOP_OFFSET_TEMP {
            get => _generalViewYTopOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string GENERAL_VIEW_Y_BOTTOM_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP {
            get => _generalViewYBottomOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_FIRST_PREFIX_TEMP {
            get => _transverseViewFirstPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewFirstPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; } = "_Сеч.1-1";
        public string TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP {
            get => _transverseViewFirstSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewFirstSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_SECOND_PREFIX_TEMP {
            get => _transverseViewSecondPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewSecondPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; } = "_Сеч.2-2";
        public string TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP {
            get => _transverseViewSecondSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewSecondSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_THIRD_PREFIX_TEMP {
            get => _transverseViewThirdPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewThirdPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; } = "_Сеч.3-3";
        public string TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP {
            get => _transverseViewThirdSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewThirdSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_TEMPLATE_NAME { get; set; } = "";
        public string TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP {
            get => _transverseViewTemplateNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewTemplateNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_X_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_X_OFFSET_TEMP {
            get => _transverseViewXOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TRANSVERSE_VIEW_Y_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_Y_OFFSET_TEMP {
            get => _transverseViewYOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

    }
}
