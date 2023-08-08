using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Visiter;

namespace RevitClashDetective.Models.Value {
    /// <summary>
    /// Параметр с типом значений ElementId. Например: "Уровень", "Типоразмер из семейства" и т.п.
    /// </summary>
    internal class ElementIdParamValue : ParamValue<string> {

        public ElementIdParamValue() : base() {

        }

        [JsonConstructor]
        public ElementIdParamValue(string value, string stringValue) : base(value, stringValue) { }


        // Примечание: значение параметра не связано с категорией элемента, у которого есть этот параметр.
        // Например, значение параметра "Уровень" какого-то экземпляра семейства это Id конкретного уровня из модели,
        // а значение параметра с типом данных "Типоразмер из семейства" - это Id конкретного типоразмера семейства
        //
        // Поэтому для создания фильтра нужно произвести поиск по элементам заданного документа и найти, если получится, тот элемент/типоразмер/семейство : типоразмер,
        // у которого название совпадает с ParamValue.TValue
        public override FilterRule GetFilterRule(IVisiter visiter, Document doc, RevitParam param) {
            var paramId = GetParamId(doc, param);
            if(paramId == ElementId.InvalidElementId) {
                // в документе не существует данного параметра
                return null;
            }

            if(visiter is EqualsVisiter || visiter is NotEqualsVisister) {
                // попытка получения элемента, являющегося значением параметра
                var value = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .FirstOrDefault(item => item.Name.Equals(TValue, StringComparison.CurrentCultureIgnoreCase));
                if(value is null) {
                    // попытка получения типоразмера, являющегося значением параметра
                    value = new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .WhereElementIsViewIndependent()
                        .FirstOrDefault(item => item.Name.Equals(TValue, StringComparison.CurrentCultureIgnoreCase));
                }
                if(value is null) {
                    // попытка получения семейства и типоразмера, являющегося значением параметра
                    // формат написания семейства и типоразмера в ревите:
                    //     НазваниеСемейства : НазваниеТипа
                    value = new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .WhereElementIsViewIndependent()
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(item => (item.FamilyName + " : " + item.Name).Equals(TValue, StringComparison.CurrentCultureIgnoreCase));
                }

                if(value == null) {
                    // в документе нет элемента/типоразмера с заданным названием
                    return null;
                } else {
                    return visiter.Create(paramId, value.Id);
                }
            }

            return visiter.Create(paramId, TValue);
        }


        public override void SetParamValue(Element element, string paramName) {
            if(element.IsExistsParam(paramName)) {
                element.SetParamValue(paramName, TValue);
            }
        }
    }
}
