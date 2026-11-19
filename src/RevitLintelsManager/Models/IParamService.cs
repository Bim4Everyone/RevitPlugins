using System.Collections.Generic;
using Autodesk.Revit.DB;
using dosymep.Bim4Everyone;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Контракт ParamService.</summary>
internal interface IParamService {

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
}
