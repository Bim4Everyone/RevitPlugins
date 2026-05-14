using System;
using System.Collections.Generic;
using System.IO;

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
}

/// <summary>
/// DTO комплекта листа
/// </summary>
public class SheetData {
    public string Name { get; set; }
    public List<SheetComponentData> Views { get; set; } = [];
}


public abstract class SheetComponentData {
    public abstract string ComponentType { get; }
    public bool? IsModuleCheck { get; set; }
    public string ModuleName { get; set; }
    public string ModuleComment { get; set; }
}

/// <summary>
/// DTO модуля вида в плане
/// </summary>
public class PlanViewData : SheetComponentData {
    public override string ComponentType => "PlanView";

    public string ViewName { get; set; }
    public string ViewFamilyTypeName { get; set; }
    public string ViewTemplateName { get; set; }
    public string ViewportTypeName { get; set; }
    public int? ViewCount { get; set; }
}

/// <summary>
/// DTO модуля вида в плане
/// </summary>
public class SectionViewData : SheetComponentData {
    public override string ComponentType => "SectionView";

    public string ViewName { get; set; }
    public int? ViewCount { get; set; }
    public string ViewFamilyTypeName { get; set; }
    public string ViewTemplateName { get; set; }
    public string ViewportTypeName { get; set; }
}

/// <summary>
/// DTO модуля спецификации
/// </summary>
public class ScheduleViewData : SheetComponentData {
    public override string ComponentType => "ScheduleView";

    public string ReferenceViewName { get; set; }
    public string ViewName { get; set; }
    public int? ViewCount { get; set; }
    public int? ViewColumn { get; set; }
}

/// <summary>
/// DTO модуля текста
/// </summary>
public class TextNoteData : SheetComponentData {
    public override string ComponentType => "TextNote";

    public string Text { get; set; }
    public string TextNoteTypeName { get; set; }
}
