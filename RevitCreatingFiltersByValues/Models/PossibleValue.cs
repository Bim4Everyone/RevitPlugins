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
            ElementInPj = elem;
            SelectedFilterableParameter = parameter;

            if(SelectedFilterableParameter.IsBInParam) {
                StorageParamType = SelectedFilterableParameter.BInParameter.GetStorageType();
            } else {
                StorageParamType = SelectedFilterableParameter.ParamElement.GetStorageType();
            }
        }


        public Element ElementInPj { get; set; }
        public ParametersHelper SelectedFilterableParameter { get; set; }


        public StorageType StorageParamType { get; set; }

        public string ValueAsString { get; set; }// = string.Empty;
        public int ValueAsInteger { get; set; }
        public ElementId ValueAsElementId { get; set; }
        public double ValueAsDouble { get; set; }


        /// <summary>
        /// Метод анализирует тип параметра и производит запись значения параметра как строки и в зависимости от типа
        /// </summary>
        public void GetValue() {


            //TaskDialog.Show("f", "На начало метода получения, имя параметра - " + SelectedFilterableParameter.ParamName);


            // Сначала работаем по исключениям - атрибутам, которые нельзя запросить через параметры
            if(SelectedFilterableParameter.BInParameter == BuiltInParameter.ALL_MODEL_TYPE_NAME) {
                ValueAsString = ElementInPj.Name;

                //TaskDialog.Show("f", "Это параметр имени типа, значение - " + ValueAsString);


            } else if(SelectedFilterableParameter.BInParameter == BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM) {
                ElementType type = ElementInPj.Document.GetElement(ElementInPj.GetTypeId()) as ElementType;
                ValueAsString = type.FamilyName;


                //TaskDialog.Show("f", "Это параметр имени семейства, значение - " + ValueAsString);

            } else {
                //TaskDialog.Show("f", "Это не параметр исключение");


                // Теперь получаем значения через параметры (сначала пробуем на экземпляре, потом на типе)
                if(SelectedFilterableParameter.IsBInParam) {
                    //TaskDialog.Show("f", "Это встроенный параметр");

                    GetValueFromBInParam();

                } else {
                    //TaskDialog.Show("f", "Это не встроенный параметр");

                    GetValueFromParamElemet();
                }
            }
        }



        private void GetValueFromBInParam() {
            
            // Пытаемся получить параметр на экземпляре
            Parameter param = ElementInPj.get_Parameter(SelectedFilterableParameter.BInParameter);
            if(param is null) {
                //TaskDialog.Show("f", "Не нашли параметр на экземпляре");


                // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
                ElementType elementType = ElementInPj.Document.GetElement(ElementInPj.GetTypeId()) as ElementType;
                if(elementType is null) { return; }

                param = elementType.get_Parameter(SelectedFilterableParameter.BInParameter);
                if(param is null) {
                    //TaskDialog.Show("f", "Не ашли на типе, но ValueAsString=" + ValueAsString);

                    return; }
                //TaskDialog.Show("f", "Нашли на типе");
            }

            //TaskDialog.Show("f", "Приступаем к получению значений");

            // Если дошли до сюда, то параметр нашли
            WriteValues(param);
        }


        private void GetValueFromParamElemet() {

            // Пытаемся получить параметр на экземпляре
            Parameter param = ElementInPj.LookupParameter(SelectedFilterableParameter.ParamName);
            if(param is null) {

                // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
                ElementType elementType = ElementInPj.Document.GetElement(ElementInPj.GetTypeId()) as ElementType;
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

            //TaskDialog.Show("f", "ValueAsString = " + ValueAsString);


            if(StorageParamType.Equals(StorageType.Double)) {
                ValueAsDouble = param.AsDouble();
                //TaskDialog.Show("f", "ValueAsDouble = " + ValueAsDouble.ToString());

            } else if(StorageParamType.Equals(StorageType.ElementId)) {
                ValueAsElementId = param.AsElementId();
                //TaskDialog.Show("f", "ValueAsElementId = " + ValueAsElementId.ToString());

            } else if(StorageParamType.Equals(StorageType.Integer)) {
                ValueAsInteger = param.AsInteger();
                //TaskDialog.Show("f", "ValueAsInteger = " + ValueAsInteger.ToString());

            }
        }
    }
}
