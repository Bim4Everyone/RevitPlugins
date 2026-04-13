using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitCreatingFiltersByValues.Models;

namespace RevitCreatingFiltersByValues.ViewModels;
internal class PossibleValueVM : BaseViewModel{
    private ParametersHelper _selectedFilterableParameter;
    private StorageType _storageParamType;
    
    private List<Element> _elementsInPj = [];
    private bool _isCheck = false;
    private string _valueAsString;
    private int _valueAsInteger;
    private ElementId _valueAsElementId;
    private double _valueAsDouble;

    public PossibleValueVM(Element elem, ParametersHelper parameter) {
        ElementsInPj.Add(elem);
        SelectedFilterableParameter = parameter;

        StorageParamType = SelectedFilterableParameter.IsBInParam
            ? elem.Document.GetStorageType(SelectedFilterableParameter.BInParameter)
            : SelectedFilterableParameter.ParamElement.GetStorageType();
    }

    
    public ParametersHelper SelectedFilterableParameter {
        get => _selectedFilterableParameter;
        set => RaiseAndSetIfChanged(ref _selectedFilterableParameter, value);
    }        
    
    public StorageType StorageParamType {
        get => _storageParamType;
        set => RaiseAndSetIfChanged(ref _storageParamType, value);
    }    
    
    public bool IsCheck {
        get => _isCheck;
        set => RaiseAndSetIfChanged(ref _isCheck, value);
    }    
    
    public List<Element> ElementsInPj {
        get => _elementsInPj;
        set => RaiseAndSetIfChanged(ref _elementsInPj, value);
    }    
    
    public string ValueAsString {
        get => _valueAsString;
        set => RaiseAndSetIfChanged(ref _valueAsString, value);
    }    
    
    public int ValueAsInteger {
        get => _valueAsInteger;
        set => RaiseAndSetIfChanged(ref _valueAsInteger, value);
    }       
    
    public ElementId ValueAsElementId {
        get => _valueAsElementId;
        set => RaiseAndSetIfChanged(ref _valueAsElementId, value);
    }   
    
    public double ValueAsDouble {
        get => _valueAsDouble;
        set => RaiseAndSetIfChanged(ref _valueAsDouble, value);
    }    

    /// <summary>
    /// Метод анализирует тип параметра и производит запись значения параметра как строки и в зависимости от типа
    /// </summary>
    public void GetValue() {

        var elem = ElementsInPj.FirstOrDefault();

        // Сначала работаем по исключениям - атрибутам, которые нельзя запросить через параметры
        if(SelectedFilterableParameter.BInParameter == BuiltInParameter.ALL_MODEL_TYPE_NAME) {
            ValueAsString = elem.Name;

        } else if(SelectedFilterableParameter.BInParameter == BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM) {
            var type = elem.Document.GetElement(elem.GetTypeId()) as ElementType;
            ValueAsString = type.FamilyName;

        } else {

            // Теперь получаем значения через параметры (сначала пробуем на экземпляре, потом на типе)
            if(SelectedFilterableParameter.IsBInParam) {
                GetValueFromBInParam(elem);

            } else {
                GetValueFromParamElement(elem);
            }
        }
    }



    private void GetValueFromBInParam(Element elem) {

        // Пытаемся получить параметр на экземпляре
        var param = elem.get_Parameter(SelectedFilterableParameter.BInParameter);
        if(param is null) {
            //TaskDialog.Show("f", "Не нашли параметр на экземпляре");


            // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
            if(elem.Document.GetElement(elem.GetTypeId()) is not ElementType elementType) { return; }

            param = elementType.get_Parameter(SelectedFilterableParameter.BInParameter);
            if(param is null) { return; }
        }

        // Если дошли до сюда, то параметр нашли
        WriteValues(param);
    }


    private void GetValueFromParamElement(Element elem) {

        // Пытаемся получить параметр на экземпляре
        var param = elem.LookupParameter(SelectedFilterableParameter.ParamName);
        if(param is null) {
            // Значит мы не нашли параметр на экземпляре и ищем параметр на типе
            if(elem.Document.GetElement(elem.GetTypeId()) is not ElementType elementType) { return; }

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
