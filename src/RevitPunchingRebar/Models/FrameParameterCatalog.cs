using System.Collections.Generic;


namespace RevitPunchingRebar.Models;

/// <summary>Назначения параметров; идентификаторы используются в конфигурации и не зависят от языка.</summary>
internal enum FrameParameterRole {
    LongRebarCode,
    LongRebarDiameterTop,
    LongRebarDiameterBottom,
    StirrupCode,
    WidthStep,
    HeightStep,
    AnchorageTop1,
    AnchorageTop2,
    AnchorageBottom1,
    AnchorageBottom2,
    StirrupChangeBinding,
    StirrupDiameter,
    StirrupBendLength,
    StirrupStep,
    StirrupCount,
    KrGroup,
    ProjectSection,
    ConsumptionMark,
    OrgLevel,
}

/// <summary>Описание назначения и имени параметра по умолчанию.</summary>
internal sealed class FrameParameterDefinition {
    public FrameParameterDefinition(FrameParameterRole role, string defaultName, bool isCopy) {
        Role = role;
        DefaultName = defaultName;
        IsCopy = isCopy;
    }

    public FrameParameterRole Role { get; }
    public string DefaultName { get; }
    public bool IsCopy { get; }
    public string TitleKey => $"Parameters.{Role}.Title";
    public string DescriptionKey => $"Parameters.{Role}.Description";
}

/// <summary>Назначения параметров и имена по умолчанию для конфигурации плагина.</summary>
internal static class FrameParameterCatalog {
    public static IReadOnlyList<FrameParameterDefinition> Definitions { get; } = [
        new(FrameParameterRole.LongRebarCode, "обр_ПР_Код металлопроката", false),
        new(FrameParameterRole.LongRebarDiameterTop, "мод_ПР_Диаметр_Верх", false),
        new(FrameParameterRole.LongRebarDiameterBottom, "мод_ПР_Диаметр_Низ", false),
        new(FrameParameterRole.StirrupCode, "обр_Х_Код металлопроката", false),
        new(FrameParameterRole.WidthStep, "мод_ПР_Шаг по ширине", false),
        new(FrameParameterRole.HeightStep, "мод_ПР_Шаг по высоте", false),
        new(FrameParameterRole.AnchorageTop1, "мод_ПР_Анкеровка_Верх_1", false),
        new(FrameParameterRole.AnchorageTop2, "мод_ПР_Анкеровка_Верх_2", false),
        new(FrameParameterRole.AnchorageBottom1, "мод_ПР_Анкеровка_Низ_1", false),
        new(FrameParameterRole.AnchorageBottom2, "мод_ПР_Анкеровка_Низ_2", false),
        new(FrameParameterRole.StirrupChangeBinding, "мод_Х_Изменить привязку", false),
        new(FrameParameterRole.StirrupDiameter, "мод_Х_Диаметр", false),
        new(FrameParameterRole.StirrupBendLength, "мод_Х_Длина отгибов", false),
        new(FrameParameterRole.StirrupStep, "мод_Х_Шаг", false),
        new(FrameParameterRole.StirrupCount, "мод_Х_Количество", false),
        new(FrameParameterRole.KrGroup, "обр_ФОП_Группа КР", false),
        new(FrameParameterRole.ProjectSection, "обр_ФОП_Раздел проекта", true),
        new(FrameParameterRole.ConsumptionMark, "обр_ФОП_Марка ведомости расхода", true),
        new(FrameParameterRole.OrgLevel, "обр_ФОП_Орг. уровень", true)
    ];
}
