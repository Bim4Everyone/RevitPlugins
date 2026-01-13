using System.ComponentModel.DataAnnotations;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserSchedulesSettingsPageVM : ValidatableViewModel {
    // Префиксы и суффиксы для поиска и новых спек
    private string _materialSchedulePrefixTemp = "КЖ..._СМ_";
    private string _materialScheduleSuffixTemp = "";
    private string _systemPartsSchedulePrefixTemp = "КЖ..._ВД_";
    private string _systemPartsScheduleSuffixTemp = "_Системная";
    private string _ifcPartsSchedulePrefixTemp = "КЖ..._ВД_";
    private string _ifcPartsScheduleSuffixTemp = "_IFC";
    private string _skeletonSchedulePrefixTemp = "КЖ..._СА_";
    private string _skeletonScheduleSuffixTemp = "_Изделия";
    private string _skeletonByElemsSchedulePrefixTemp = "КЖ..._СА_";
    private string _skeletonByElemsScheduleSuffixTemp = "_Изделия_Поэлементная";

    // Названия эталонных спек
    private string _materialScheduleNameTemp = "01_(КЖ...)_СМ_Базовая_(Марка пилона)_Одноэтажный";
    private string _systemPartsScheduleNameTemp = "01_(КЖ...)_ВД_(Марка пилона)_Системная";
    private string _ifcPartsScheduleNameTemp = "01_(КЖ...)_ВД_(Марка пилона)_IFC";
    private string _skeletonScheduleNameTemp = "01_(КЖ...)_Изделия_(Марка)";
    private string _skeletonByElemsScheduleNameTemp = "01_(КЖ...)_Изделия_(Марка)_Поэлементная";

    // Заполнение параметров диспетчера
    private string _materialScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
    private string _materialScheduleDisp2Temp = "СМ_Пилоны";
    private string _systemPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
    private string _systemPartsScheduleDisp2Temp = "ВД_СИС_Пилоны";
    private string _ifcPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
    private string _ifcPartsScheduleDisp2Temp = "ВД_IFC_Пилоны";
    private string _skeletonScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
    private string _skeletonScheduleDisp2Temp = "СА_Пилоны";
    private string _skeletonByElemsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
    private string _skeletonByElemsScheduleDisp2Temp = "СА_Пилоны";


    public UserSchedulesSettingsPageVM(MainViewModel mainViewModel) {
        ViewModel = mainViewModel;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }

    public string MaterialSchedulePrefix { get; set; }
    public string MaterialSchedulePrefixTemp {
        get => _materialSchedulePrefixTemp;
        set => RaiseAndSetIfChanged(ref _materialSchedulePrefixTemp, value);
    }

    public string MaterialScheduleSuffix { get; set; }
    public string MaterialScheduleSuffixTemp {
        get => _materialScheduleSuffixTemp;
        set => RaiseAndSetIfChanged(ref _materialScheduleSuffixTemp, value);
    }

    public string SystemPartsSchedulePrefix { get; set; }
    public string SystemPartsSchedulePrefixTemp {
        get => _systemPartsSchedulePrefixTemp;
        set => RaiseAndSetIfChanged(ref _systemPartsSchedulePrefixTemp, value);
    }

    public string SystemPartsScheduleSuffix { get; set; }
    public string SystemPartsScheduleSuffixTemp {
        get => _systemPartsScheduleSuffixTemp;
        set => RaiseAndSetIfChanged(ref _systemPartsScheduleSuffixTemp, value);
    }

    public string IfcPartsSchedulePrefix { get; set; }
    public string IfcPartsSchedulePrefixTemp {
        get => _ifcPartsSchedulePrefixTemp;
        set => RaiseAndSetIfChanged(ref _ifcPartsSchedulePrefixTemp, value);
    }

    public string IfcPartsScheduleSuffix { get; set; }
    public string IfcPartsScheduleSuffixTemp {
        get => _ifcPartsScheduleSuffixTemp;
        set => RaiseAndSetIfChanged(ref _ifcPartsScheduleSuffixTemp, value);
    }

    public string SkeletonSchedulePrefix { get; set; }
    public string SkeletonSchedulePrefixTemp {
        get => _skeletonSchedulePrefixTemp;
        set => RaiseAndSetIfChanged(ref _skeletonSchedulePrefixTemp, value);
    }

    public string SkeletonScheduleSuffix { get; set; }
    public string SkeletonScheduleSuffixTemp {
        get => _skeletonScheduleSuffixTemp;
        set => RaiseAndSetIfChanged(ref _skeletonScheduleSuffixTemp, value);
    }
    public string SkeletonByElemsSchedulePrefix { get; set; }
    public string SkeletonByElemsSchedulePrefixTemp {
        get => _skeletonByElemsSchedulePrefixTemp;
        set => RaiseAndSetIfChanged(ref _skeletonByElemsSchedulePrefixTemp, value);
    }

    public string SkeletonByElemsScheduleSuffix { get; set; }
    public string SkeletonByElemsScheduleSuffixTemp {
        get => _skeletonByElemsScheduleSuffixTemp;
        set => RaiseAndSetIfChanged(ref _skeletonByElemsScheduleSuffixTemp, value);
    }

    public string SkeletonScheduleName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonScheduleNameTemp {
        get => _skeletonScheduleNameTemp;
        set {
            RaiseAndSetIfChanged(ref _skeletonScheduleNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string SkeletonByElemsScheduleName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonByElemsScheduleNameTemp {
        get => _skeletonByElemsScheduleNameTemp;
        set {
            RaiseAndSetIfChanged(ref _skeletonByElemsScheduleNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string MaterialScheduleName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string MaterialScheduleNameTemp {
        get => _materialScheduleNameTemp;
        set {
            RaiseAndSetIfChanged(ref _materialScheduleNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string SystemPartsScheduleName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SystemPartsScheduleNameTemp {
        get => _systemPartsScheduleNameTemp;
        set {
            RaiseAndSetIfChanged(ref _systemPartsScheduleNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string IfcPartsScheduleName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string IfcPartsScheduleNameTemp {
        get => _ifcPartsScheduleNameTemp;
        set {
            RaiseAndSetIfChanged(ref _ifcPartsScheduleNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string MaterialScheduleDisp1 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string MaterialScheduleDisp1Temp {
        get => _materialScheduleDisp1Temp;
        set {
            RaiseAndSetIfChanged(ref _materialScheduleDisp1Temp, value);
            ValidateProperty(value);
        }
    }
    public string SystemPartsScheduleDisp1 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SystemPartsScheduleDisp1Temp {
        get => _systemPartsScheduleDisp1Temp;
        set {
            RaiseAndSetIfChanged(ref _systemPartsScheduleDisp1Temp, value);
            ValidateProperty(value);
        }
    }
    public string IfcPartsScheduleDisp1 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string IfcPartsScheduleDisp1Temp {
        get => _ifcPartsScheduleDisp1Temp;
        set {
            RaiseAndSetIfChanged(ref _ifcPartsScheduleDisp1Temp, value);
            ValidateProperty(value);
        }
    }

    public string SkeletonScheduleDisp1 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonScheduleDisp1Temp {
        get => _skeletonScheduleDisp1Temp;
        set {
            RaiseAndSetIfChanged(ref _skeletonScheduleDisp1Temp, value);
            ValidateProperty(value);
        }
    }
    public string SkeletonByElemsScheduleDisp1 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonByElemsScheduleDisp1Temp {
        get => _skeletonByElemsScheduleDisp1Temp;
        set {
            RaiseAndSetIfChanged(ref _skeletonByElemsScheduleDisp1Temp, value);
            ValidateProperty(value);
        }
    }

    public string MaterialScheduleDisp2 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string MaterialScheduleDisp2Temp {
        get => _materialScheduleDisp2Temp;
        set {
            RaiseAndSetIfChanged(ref _materialScheduleDisp2Temp, value);
            ValidateProperty(value);
        }
    }
    public string SystemPartsScheduleDisp2 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SystemPartsScheduleDisp2Temp {
        get => _systemPartsScheduleDisp2Temp;
        set {
            RaiseAndSetIfChanged(ref _systemPartsScheduleDisp2Temp, value);
            ValidateProperty(value);
        }
    }

    public string IfcPartsScheduleDisp2 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string IfcPartsScheduleDisp2Temp {
        get => _ifcPartsScheduleDisp2Temp;
        set {
            RaiseAndSetIfChanged(ref _ifcPartsScheduleDisp2Temp, value);
            ValidateProperty(value);
        }
    }

    public string SkeletonScheduleDisp2 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonScheduleDisp2Temp {
        get => _skeletonScheduleDisp2Temp;
        set {
            RaiseAndSetIfChanged(ref _skeletonScheduleDisp2Temp, value);
            ValidateProperty(value);
        }
    }
    public string SkeletonByElemsScheduleDisp2 { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SkeletonByElemsScheduleDisp2Temp {
        get => _skeletonByElemsScheduleDisp2Temp;
        set {
            RaiseAndSetIfChanged(ref _skeletonByElemsScheduleDisp2Temp, value);
            ValidateProperty(value);
        }
    }


    public void ApplySchedulesSettings() {
        MaterialSchedulePrefix = MaterialSchedulePrefixTemp;
        MaterialScheduleSuffix = MaterialScheduleSuffixTemp;

        SystemPartsSchedulePrefix = SystemPartsSchedulePrefixTemp;
        SystemPartsScheduleSuffix = SystemPartsScheduleSuffixTemp;

        IfcPartsSchedulePrefix = IfcPartsSchedulePrefixTemp;
        IfcPartsScheduleSuffix = IfcPartsScheduleSuffixTemp;

        SkeletonSchedulePrefix = SkeletonSchedulePrefixTemp;
        SkeletonScheduleSuffix = SkeletonScheduleSuffixTemp;
        SkeletonByElemsSchedulePrefix = SkeletonByElemsSchedulePrefixTemp;
        SkeletonByElemsScheduleSuffix = SkeletonByElemsScheduleSuffixTemp;

        MaterialScheduleName = MaterialScheduleNameTemp;
        SystemPartsScheduleName = SystemPartsScheduleNameTemp;
        IfcPartsScheduleName = IfcPartsScheduleNameTemp;
        SkeletonScheduleName = SkeletonScheduleNameTemp;
        SkeletonByElemsScheduleName = SkeletonByElemsScheduleNameTemp;

        MaterialScheduleDisp1 = MaterialScheduleDisp1Temp;
        SystemPartsScheduleDisp1 = SystemPartsScheduleDisp1Temp;
        IfcPartsScheduleDisp1 = IfcPartsScheduleDisp1Temp;
        SkeletonScheduleDisp1 = SkeletonScheduleDisp1Temp;
        SkeletonByElemsScheduleDisp1 = SkeletonByElemsScheduleDisp1Temp;

        MaterialScheduleDisp2 = MaterialScheduleDisp2Temp;
        SystemPartsScheduleDisp2 = SystemPartsScheduleDisp2Temp;
        IfcPartsScheduleDisp2 = IfcPartsScheduleDisp2Temp;
        SkeletonScheduleDisp2 = SkeletonScheduleDisp2Temp;
        SkeletonByElemsScheduleDisp2 = SkeletonByElemsScheduleDisp2Temp;
    }

    public UserSchedulesSettings GetSettings() {
        var settings = new UserSchedulesSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserSchedulesSettings);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null && vmProp.CanRead && prop.CanWrite) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }
}
