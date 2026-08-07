using Autodesk.Revit.DB;

namespace RevitClassifierParameters.Models.MaterialClassifier;

/// <summary>
/// Класс-оболочка для хранения информации о материале Revit и связанной с ним работе из Классификатора.
/// </summary>
public class RevitMaterial {
    private const char _mark = '_';

    /// <summary>
    /// Создает обёртку материала.
    /// </summary>
    /// <param name="keynote">Ключевая заметка материала (код работы).</param>
    /// <param name="material">Материал Revit.</param>
    /// <param name="work">Работа из Классификатора, параметры которой нужно назначить.</param>
    public RevitMaterial(string keynote, Material material, Work.Work work) {
        Keynote = keynote;
        Material = material;
        Work = work;
        MaterialDescription = GetMaterialDescription();
    }

    /// <summary>
    /// Ключевая заметка материала (код работы).
    /// </summary>
    public string Keynote { get; }

    /// <summary>
    /// Материал Revit.
    /// </summary>
    public Material Material { get; }

    /// <summary>
    /// Работа из Классификатора, параметры которой нужно назначить.
    /// </summary>
    public Work.Work Work { get; }

    /// <summary>
    /// Описание материала для параметра "Материал: Описание".
    /// </summary>
    public string MaterialDescription { get; }

    /// <summary>
    /// Возвращает строку для заполнения параметра "Материал: Описание".
    /// Строка содержится в имени материала между первым и вторым нижним подчеркиванием.
    /// Пример: "г02.04.01.04_Бетон_Выше 0_Устройство монолитных ж/б пилонов" -> "Бетон".
    /// </summary>
    private string GetMaterialDescription() {
        string materialName = Material.Name ?? string.Empty;

        int firstIndex = materialName.IndexOf(_mark);
        if(firstIndex < 0) {
            return string.Empty;
        }

        int secondIndex = materialName.IndexOf(_mark, firstIndex + 1);
        if(secondIndex < 0) {
            return string.Empty;
        }

        return materialName.Substring(firstIndex + 1, secondIndex - firstIndex - 1).Trim();
    }
}
