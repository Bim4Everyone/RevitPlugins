using System;
using System.IO;
using System.Linq;
using System.Text;

using dosymep.SimpleServices;

using pyRevitLabs.Json;

namespace RevitDocumenter.Models.RoomDimensions.Settings;

/// <summary>
/// Читает, проверяет и сохраняет JSON-файл настроек образмеривания помещений.
/// </summary>
internal class RoomDimensioningSettingsReader {
    private readonly ILocalizationService _localizationService;

    public RoomDimensioningSettingsReader(ILocalizationService localizationService) {
        _localizationService = localizationService.ThrowIfNull();
    }

    /// <summary>
    /// Загружает настройки из файла.
    /// </summary>
    /// <exception cref="InvalidOperationException">Файл не найден, не читается или содержит ошибки.</exception>
    public RoomDimensioningSettings Read(string path) {
        if(string.IsNullOrWhiteSpace(path) || !File.Exists(path)) {
            throw new InvalidOperationException($"{Localize("RoomDimensioningSettings.FileNotFound")} {path}");
        }

        RoomDimensioningSettings settings;
        try {
            string json = File.ReadAllText(path);
            settings = JsonConvert.DeserializeObject<RoomDimensioningSettings>(json);
        } catch(JsonException ex) {
            throw new InvalidOperationException(
                $"{Localize("RoomDimensioningSettings.DeserializationError")} {path}", ex);
        } catch(IOException ex) {
            throw new InvalidOperationException($"{Localize("RoomDimensioningSettings.ReadError")} {path}", ex);
        }

        if(settings is null) {
            throw new InvalidOperationException($"{Localize("RoomDimensioningSettings.Empty")} {path}");
        }

        // Пропущенные в JSON блоки заменяем значениями по умолчанию
        settings.Layout ??= new LayoutSettings();
        settings.RoomTypes ??= [];
        settings.Debug ??= new DebugSettings();

        Validate(settings);
        return settings;
    }

    /// <summary>
    /// Сохраняет настройки в файл.
    /// </summary>
    /// <exception cref="InvalidOperationException">Файл не удалось записать.</exception>
    public void Save(RoomDimensioningSettings settings, string path) {
        settings.ThrowIfNull();
        try {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json, new UTF8Encoding(false));
        } catch(Exception ex) when(ex is IOException or UnauthorizedAccessException) {
            throw new InvalidOperationException($"{Localize("RoomDimensioningSettings.WriteError")} {path}", ex);
        }
    }

    private void Validate(RoomDimensioningSettings settings) {
        if(string.IsNullOrWhiteSpace(settings.PurposeParameterName)) {
            throw new InvalidOperationException(Localize("RoomDimensioningSettings.PurposeParameterEmpty"));
        }

        if(settings.RoomTypes.Count == 0) {
            throw new InvalidOperationException(Localize("RoomDimensioningSettings.NoRoomTypes"));
        }

        for(int i = 0; i < settings.RoomTypes.Count; i++) {
            var roomType = settings.RoomTypes[i];
            int number = i + 1;
            if(roomType is null) {
                throw new InvalidOperationException(
                    string.Format(Localize("RoomDimensioningSettings.RoomTypeFieldEmpty"), number, "RoomTypes"));
            }
            CheckFilled(roomType.Name, number, nameof(RoomTypeSettings.Name));
            CheckFilled(roomType.PurposeValue, number, nameof(RoomTypeSettings.PurposeValue));
            CheckFilled(roomType.DimensionTypeName, number, nameof(RoomTypeSettings.DimensionTypeName));
        }

        var duplicate = settings.RoomTypes
            .GroupBy(t => t.PurposeValue.Trim())
            .FirstOrDefault(g => g.Count() > 1);
        if(duplicate != null) {
            throw new InvalidOperationException(
                $"{Localize("RoomDimensioningSettings.DuplicatePurposeValue")} {duplicate.Key}");
        }
    }

    private void CheckFilled(string value, int roomTypeNumber, string fieldName) {
        if(string.IsNullOrWhiteSpace(value)) {
            throw new InvalidOperationException(
                string.Format(Localize("RoomDimensioningSettings.RoomTypeFieldEmpty"), roomTypeNumber, fieldName));
        }
    }

    private string Localize(string key) {
        return _localizationService.GetLocalizedString(key);
    }
}
