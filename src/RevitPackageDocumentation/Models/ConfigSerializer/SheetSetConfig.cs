using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

public class SheetSetConfig {
    private readonly ISheetSetSerializer _serializer;

    public SheetSetConfig(ISheetSetSerializer sheetSetSerializer) {
        _serializer = sheetSetSerializer;
    }

    /// <summary>
    /// Загрузить конфигурацию из файла.
    /// </summary>
    public SheetSetData Import(string path) {
        if(string.IsNullOrEmpty(path) || !File.Exists(path)) {
            return new SheetSetData();
        }

        try {
            string json = File.ReadAllText(path);
            return _serializer.Deserialize<SheetSetData>(json);
        } catch(JsonSerializationException ex) {
            throw new InvalidOperationException($"Ошибка десериализации файла {path}", ex);
        } catch(IOException ex) {
            throw new InvalidOperationException($"Ошибка чтения файла {path}", ex);
        }
    }

    /// <summary>
    /// Сохранить конфигурацию в файл.
    /// </summary>
    public void Export(SheetSetData data, string path) {
        if(string.IsNullOrEmpty(path))
            throw new ArgumentException("Путь сохранения не корректен!", nameof(path));

        if(data == null)
            throw new ArgumentNullException(nameof(data));

        try {
            string json = _serializer.Serialize(data);
            File.WriteAllText(path, json);
        } catch(IOException ex) {
            throw new InvalidOperationException($"Ошибка записи файла {path}", ex);
        }
    }
}


/// <summary>
/// DTO комплекта листов
/// </summary>
public class SheetSetData {
    public string Name { get; set; } = "Новая конфигурация";
    public List<SheetData> Sheets { get; set; } = [];
    public List<PluginParamData> Params { get; set; } = [];
}

public abstract class ParamContainerModuleData {
    public bool? IsModuleCheck { get; set; }
    public string ModuleName { get; set; }
    public string ModuleComment { get; set; }
    public CustomParametersListData CustomParamsList { get; set; }
}

/// <summary>
/// DTO комплекта листа
/// </summary>
public class SheetData : ParamContainerModuleData {
    public string SheetNameFormula { get; set; }
    public string SheetSize { get; set; }
    public string SheetCoefficient { get; set; }
    public string TitleBlockFamilyName { get; set; }
    public string TitleBlockTypeName { get; set; }
    public List<SheetComponentData> Views { get; set; } = [];
}

public abstract class SheetComponentData : ParamContainerModuleData {
    public abstract string ComponentType { get; }
}

/// <summary>
/// DTO модуля вида в плане несущих конструкций
/// </summary>
public class StructuralPlanViewData : SheetComponentData {
    public override string ComponentType => "StructuralPlanView";

    public string ViewNameFormula { get; set; }
    public string ViewFamilyTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewFamilyTypeFilterValues { get; set; }
    public string ViewTemplateName { get; set; }
    public FiltrationComboBoxFilterListData ViewTemplateFilterValues { get; set; }
    public string ViewportTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewportTypeFilterValues { get; set; }
    public string ViewCount { get; set; }
    public string SelectedSelectElemParamName { get; set; }
}

/// <summary>
/// DTO модуля фрагмента плана несущих конструкций
/// </summary>
public class StructuralCalloutViewData : SheetComponentData {
    public override string ComponentType => "StructuralCalloutView";

    public string ViewNameFormula { get; set; }
    public string ViewFamilyTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewFamilyTypeFilterValues { get; set; }
    public string ViewTemplateName { get; set; }
    public FiltrationComboBoxFilterListData ViewTemplateFilterValues { get; set; }
    public string ViewportTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewportTypeFilterValues { get; set; }
    public string ViewCount { get; set; }
    public string SelectedSelectElemParamName { get; set; }
}


/// <summary>
/// DTO модуля вида в сечения
/// </summary>
public class SectionViewData : SheetComponentData {
    public override string ComponentType => "SectionView";

    public string ViewNameFormula { get; set; }
    public string ViewCount { get; set; }
    public string ViewFamilyTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewFamilyTypeFilterValues { get; set; }
    public string ViewTemplateName { get; set; }
    public FiltrationComboBoxFilterListData ViewTemplateFilterValues { get; set; }
    public string ViewportTypeName { get; set; }
    public FiltrationComboBoxFilterListData ViewportTypeFilterValues { get; set; }
    public string SelectedSelectElemParamName { get; set; }
}

/// <summary>
/// DTO модуля спецификации
/// </summary>
public class ScheduleViewData : SheetComponentData {
    public override string ComponentType => "ScheduleView";

    public string ReferenceViewName { get; set; }
    public string ViewNameFormula { get; set; }
    public string ViewCount { get; set; }
    public string ViewColumn { get; set; }
    public ScheduleFilterListData ScheduleFilterList { get; set; }
}


/// <summary>
/// DTO модуля текста
/// </summary>
public class TextNoteData : SheetComponentData {
    public override string ComponentType => "TextNote";

    public string TextFormula { get; set; }
    public string TextNoteTypeName { get; set; }
}

/// <summary>
/// DTO модуля типовой аннотации
/// </summary>
public class TypicalAnnotationData : SheetComponentData {
    public override string ComponentType => "TypicalAnnotation";

    public string AnnotationFamilyName { get; set; }
    public string AnnotationTypeName { get; set; }
}

/// <summary>
/// DTO модуля легенды
/// </summary>
public class LegendViewData : SheetComponentData {
    public override string ComponentType => "LegendView";

    public string ViewName { get; set; }
    public string ViewportTypeName { get; set; }
}

/// <summary>
/// DTO списка фильтров спецификации
/// </summary>
public class ScheduleFilterListData {
    public List<ScheduleFilterRuleData> ScheduleFilterRules { get; set; }
}

/// <summary>
/// DTO фильтра спецификации
/// </summary>
public class ScheduleFilterRuleData {
    public string FieldName { get; set; }
    public ScheduleFilterType FilterType { get; set; }
    public string FilterValueFormula { get; set; }
}

/// <summary>
/// DTO списка дополнительных параметров
/// </summary>
public class CustomParametersListData {
    public List<CustomParameterData> Params { get; set; }
}

/// <summary>
/// DTO дополнительного параметра
/// </summary>
public class CustomParameterData {
    public string ParamName { get; set; }
    public string ParamValueFormula { get; set; }
}

/// <summary>
/// DTO списка фильтров для свойства
/// </summary>
public class FiltrationComboBoxFilterListData {
    public List<FiltrationComboBoxFilterData> ValueList { get; set; }
}

/// <summary>
/// DTO дополнительного параметра
/// </summary>
public class FiltrationComboBoxFilterData {
    public string ValueFormula { get; set; }
}


/// <summary>
/// DTO параметров
/// </summary>
public abstract class PluginParamData {
    public abstract string PluginParamType { get; }
    public string ParamName { get; set; }
    public string ParamComment { get; set; }
}

public class StringParamData : PluginParamData {
    public override string PluginParamType => "StringParam";

    public string StringValue { get; set; }
}

public class SelectElemParamData : PluginParamData {
    public override string PluginParamType => "SelectElem";
}
