using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitCreatingFiltersByValues.Models {
    internal class PossibleValue {
        public PossibleValue(Element elem, ParametersHelper parameter) {
            ElementsInPj.Add(elem);
            SelectedFilterableParameter = parameter;

            if(SelectedFilterableParameter.IsBInParam) {
                StorageParamType = elem.Document.GetStorageType(SelectedFilterableParameter.BInParameter);
            } else {
                StorageParamType = SelectedFilterableParameter.ParamElement.GetStorageType();
            }
        }


        public List<Element> ElementsInPj { get; set; } = new List<Element>();
        public ParametersHelper SelectedFilterableParameter { get; set; }

        public bool IsCheck { get; set; } = false;


        public StorageType StorageParamType { get; set; }

        public string ValueAsString { get; set; }
        public int ValueAsInteger { get; set; }
        public ElementId ValueAsElementId { get; set; }
        public double ValueAsDouble { get; set; }


        /// <summary>
        /// Метод анализирует тип параметра и производит запись значения параметра как строки и в зависимости от типа
        /// </summary>
        public void GetValue() {

            Element elem = ElementsInPj.FirstOrDefault();

            // Сначала работаем по исключениям - атрибутам, которые нельзя запросить через параметры
            if(SelectedFilterableParameter.BInParameter == BuiltInParameter.ALL_MODEL_TYPE_NAME) {
                ValueAsString = elem.Name;

            } else if(SelectedFilterableParameter.BInParameter == BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM) {
                ElementType type = elem.Document.GetElement(elem.GetTypeId()) as ElementType;
                ValueAsString = type.FamilyName;

            } else {

                // Теперь получаем значения через параметры (сначала пробуем на экземпляре, потом на типе)
                if(SelectedFilterableParameter.IsBInParam) {
                    GetValueFromBInParam(elem);

                } else {
                    GetValueFromParamElemet(elem);
                }
            }
        }



        private void GetValueFromBInParam(Element elem) {

            // Пытаемся получить параметр на экземпляре
            Parameter param = elem.get_Parameter(SelectedFilterableParameter.BInParameter);
            if(param is null) {
                //TaskDialog.Show("f", "Не нашли параметр на экземпляре");


                // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
                ElementType elementType = elem.Document.GetElement(elem.GetTypeId()) as ElementType;
                if(elementType is null) { return; }

                param = elementType.get_Parameter(SelectedFilterableParameter.BInParameter);
                if(param is null) { return;}
            }

            // Если дошли до сюда, то параметр нашли
            WriteValues(param);
        }


        private void GetValueFromParamElemet(Element elem) {

            // Пытаемся получить параметр на экземпляре
            Parameter param = elem.LookupParameter(SelectedFilterableParameter.ParamName);
            if(param is null) {

                // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
                ElementType elementType = elem.Document.GetElement(elem.GetTypeId()) as ElementType;
                if(elementType is null) { return; }

                param = elementType.LookupParameter(SelectedFilterableParameter.ParamName);
                if(param is null) { return; }
            }

            // Если дошли до сюда, то параметр нашли
            WriteValues(param);
        }

        private void WriteValues(Parameter param) {

            // В любом случае заполняем string значение для отображения в GUI
            ValueAsString = param.AsValueString();


            if(StorageParamType.Equals(StorageType.Double)) {
                ValueAsDouble = param.AsDouble();

            } else if(StorageParamType.Equals(StorageType.ElementId)) {
                ValueAsElementId = param.AsElementId();

            } else if(StorageParamType.Equals(StorageType.Integer)) {
                ValueAsInteger = param.AsInteger();

            }
        }
    }
}
