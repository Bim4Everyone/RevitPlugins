using dosymep.Bim4Everyone.CustomParams;

using Autodesk.Revit.DB;

namespace RevitClashDetective;

/// <summary>
/// Заглушка параметра, которая записывается в устаревшее свойство <see cref="Models.Filtration.FiltersConfig.Filters"/>
/// при сохранении конфига поисковых наборов.
/// <para/>
/// Старые версии плагина не знают этот тип, поэтому падают при десериализации конфига
/// с сообщением "Не поддерживаемое название типа параметра: RevitClashDetective.You_must_update_plugin"
/// <see cref="RevitClashDetective.Models.RevitParamConverter"/> и не могут перезаписать конфиг старым форматом.
/// <para/>
/// Название класса намеренно нарушает соглашения об именовании и лежит в корневом пространстве имен:
/// оно целиком попадает в текст ошибки, которую видит пользователь.
/// </summary>
internal sealed class You_must_update_plugin : CustomParam {
    public You_must_update_plugin()
        : base(nameof(You_must_update_plugin)) {
        Name = nameof(You_must_update_plugin);
        StorageType = StorageType.String;
        Description = nameof(You_must_update_plugin);
#if REVIT_2020
        UnitType = UnitType.UT_Number;
#elif REVIT_2021
        UnitType = SpecTypeId.Number;
#else
        UnitType = SpecTypeId.String.Text;
#endif
    }
}
