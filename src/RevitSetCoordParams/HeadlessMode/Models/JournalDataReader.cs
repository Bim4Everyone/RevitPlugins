using System.Collections.Generic;

using RevitSetCoordParams.HeadlessMode.Enums;

namespace RevitSetCoordParams.HeadlessMode.Models;
internal class JournalDataReader {
    // Поля соответствующие свойствам класса GonfigSettings
    private const string _defaultsSettingsValue = "DefaultSettings";
    private const string _configSettingsValue = "ConfigSettings";
    private const string _elementsProviderKey = "ElementsProvider";
    private const string _positionProviderKey = "PositionProvider";
    private const string _sourceFileKey = "SourceFile";
    private const string _typeModelsKey = "TypeModels";
    private const string _paramMapsKey = "ParamMaps";
    private const string _categoriesKey = "Categories";
    private const string _maxDiameterSearchSphereMmKey = "MaxDiameterSearchSphereMm";
    private const string _stepDiameterSearchSphereMmKey = "StepDiameterSearchSphereMm";
    private const string _searchKey = "Search";
    private const string _saveConfigSettingsKey = "saveConfigSettings";

    private readonly IDictionary<string, string> _journalData;

    public JournalDataReader(IDictionary<string, string> journalData) {
        _journalData = journalData;
    }

    public JournalContainer ElementsProvider => GetValue(_elementsProviderKey);
    public JournalContainer PositionProvider => GetValue(_positionProviderKey);
    public JournalContainer SourceFile => GetValue(_sourceFileKey);
    public JournalContainer TypeModels => GetValue(_typeModelsKey);
    public JournalContainer ParamMaps => GetValue(_paramMapsKey);
    public JournalContainer Categories => GetValue(_categoriesKey);
    public JournalContainer MaxDiameterSearchSphereMm => GetValue(_maxDiameterSearchSphereMmKey);
    public JournalContainer StepDiameterSearchSphereMm => GetValue(_stepDiameterSearchSphereMmKey);
    public JournalContainer Search => GetValue(_searchKey);

    // Метод получения JournalContainer и распределения по типам настроек
    private JournalContainer GetValue(string key) {
        return !_journalData.TryGetValue(key, out string value)
            ? null
            : value == _defaultsSettingsValue
            ? new JournalContainer() {
                SettingType = SettingType.DefaultSettings,
                JournalValue = default
            }
            : value == _configSettingsValue
            ? new JournalContainer() {
                SettingType = SettingType.ConfigSettings,
                JournalValue = default
            }
            : new JournalContainer() {
                SettingType = SettingType.ExplicitSettings,
                JournalValue = value
            };
    }
}
