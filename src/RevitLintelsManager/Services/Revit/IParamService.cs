using System.Collections.Generic;
using Autodesk.Revit.DB;
using dosymep.Bim4Everyone;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Контракт ParamService.</summary>
internal interface IParamService {
    /// <summary>Записывает расчётное значение в однозначный параметр экземпляра.</summary>
    /// <param name="instance">Новая перемычка.</param>
    /// <param name="value">Сопоставленная роль, тип и значение.</param>
    /// <returns>Возможность записи параметра данного экземпляра.</returns>
    bool WriteInstanceValue(FamilyInstance instance, LintelParameterValue value);


    /// <summary>Создаёт каталог параметров семейства в контексте API.</summary>
    /// <param name="document">Исходный документ.</param>
    /// <param name="symbol">Тип семейства.</param>
    /// <param name="instance">Образец экземпляра или null.</param>
    /// <returns>Общий каталог и параметры экземпляра.</returns>
    (IList<RevitParam> Parameters, IList<RevitParam> InstanceParameters) CreateRevitParams(Document document, FamilySymbol symbol, FamilyInstance instance);

    /// <summary>Находит сопоставленный параметр перемычки.</summary>
    /// <param name="family">Семейство.</param>
    /// <param name="paramName">Имя параметра.</param>
    /// <param name="storageType">Тип хранения.</param>
    /// <returns>Параметр или null.</returns>
    RevitParam GetLintelParam(RevitFamily family, string paramName, StorageType storageType);

    /// <summary>Находит общий параметр проёмов.</summary>
    /// <param name="families">Семейства проёмов.</param>
    /// <param name="paramName">Имя параметра.</param>
    /// <param name="storageType">Тип хранения.</param>
    /// <returns>Общий параметр или null.</returns>
    RevitParam GetOpeningParam(RevitFamily[] families, string paramName, StorageType storageType);

    /// <summary>Находит общий параметр фиксации.</summary>
    /// <param name="lintelConfig">Конфигурация перемычек.</param>
    /// <param name="openingConfig">Конфигурация проёмов.</param>
    /// <param name="paramName">Имя параметра.</param>
    /// <returns>Параметр фиксации или null.</returns>
    RevitParam GetFixParam(LintelFamilyConfig lintelConfig, OpeningFamilyConfig openingConfig, string paramName);

    /// <summary>Получает общие параметры экземпляров типа Да/Нет.</summary>
    /// <param name="families">Проверяемые семейства.</param>
    /// <returns>Материализованные общие параметры.</returns>
    IEnumerable<RevitParam> GetCommonFixParams(IEnumerable<RevitFamily> families);

    /// <summary>Читает параметр длины в миллиметрах, сохраняя отсутствие значения.</summary>
    /// <param name="instance">Читаемый экземпляр.</param>
    /// <param name="type">Тип для поиска параметра, отсутствующего у экземпляра.</param>
    /// <param name="name">Имя из сохранённого сопоставления.</param>
    /// <returns>Длина в мм или null при отсутствии доступного значения длины.</returns>
    double? ReadLengthMm(Element instance, Element type, string name);

    /// <summary>Читает параметр с целочисленным хранением.</summary>
    /// <param name="instance">Читаемый экземпляр.</param>
    /// <param name="type">Тип для поиска параметра, отсутствующего у экземпляра.</param>
    /// <param name="name">Имя из сохранённого сопоставления.</param>
    /// <returns>Целое значение или null при отсутствии доступного значения.</returns>
    int? ReadInteger(Element instance, Element type, string name);

    /// <summary>Читает пользовательский параметр Да/Нет только у экземпляра.</summary>
    /// <param name="instance">Читаемый экземпляр.</param>
    /// <param name="name">Имя настроенного параметра фиксации.</param>
    /// <returns>Значение фиксации или null при отсутствии доступного значения.</returns>
    bool? ReadFixation(Element instance, string name);

    /// <summary>Проверяет доступность сопоставлений для будущей записи в экземпляр перемычки.</summary>
    /// <param name="project">Исходная модель для проверки проектных привязок.</param>
    /// <param name="familyDocument">Открытое определение выбранного семейства.</param>
    /// <param name="family">Выбранное семейство из исходной модели.</param>
    /// <param name="sample">Необязательный экземпляр для проверки проектных параметров.</param>
    /// <param name="mappings">Неизменяемые сопоставления сессии.</param>
    /// <returns>Доступность параметров по ролям.</returns>
    IReadOnlyDictionary<string, LintelParameterFact> ReadLintelParameterFacts(Document project,
        Document familyDocument, Family family, FamilyInstance sample, IReadOnlyDictionary<string, string> mappings);
}
