using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class Frame {
    //Длина выпуска продольных стрежней за крайние хомуты
    private readonly double _addLength = UnitUtils.ConvertToInternalUnits(50, UnitTypeId.Millimeters);

    //Длина отгиба хомута
    private readonly double _bendLegth = UnitUtils.ConvertToInternalUnits(90, UnitTypeId.Millimeters);

    //Класс армирования продольных стержней
    private readonly int _longRebarClass = 501;

    //Диаметр продольных стержней каркаса
    public readonly double LongRebarDiameter = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters);

    public FamilyInstance FrameInstance { get; set; }

    internal Frame(FamilyInstance familyInstance, IFrameParams frameParams) {
        FrameInstance = familyInstance;

        //Заполняем необходимые параметры каркаса
        SetParams(frameParams);
    }

    public XYZ GetLocation() {
        XYZ location = ((LocationPoint) FrameInstance.Location).Point;

        XYZ currentLocation = new XYZ
                    (
                        location.X + FrameInstance.FacingOrientation.X * 0.5 * GetParam<double>(FrameInstance, "мод_ПР_Шаг по ширине"),
                        location.Y + FrameInstance.FacingOrientation.Y * 0.5 * GetParam<double>(FrameInstance, "мод_ПР_Шаг по ширине"),
                        location.Z
                    );

        return currentLocation;
    }

    private void SetParams(IFrameParams frameParams) {
        SetParam("обр_ПР_Код металлопроката", _longRebarClass);
        SetParam("обр_Х_Код металлопроката", frameParams.StirrupRebarClass);

        SetParam("мод_ПР_Шаг по ширине", frameParams.FrameWidth - frameParams.StirrupRebarDiameter - LongRebarDiameter);
        SetParam("мод_ПР_Шаг по высоте", frameParams.GetFrameHeight(LongRebarDiameter));

        SetParam("мод_ПР_Анкеровка_Верх_1", _addLength);
        SetParam("мод_ПР_Анкеровка_Верх_2", _addLength);
        SetParam("мод_ПР_Анкеровка_Низ_1", _addLength);
        SetParam("мод_ПР_Анкеровка_Низ_2", _addLength);

        SetParam("мод_Х_Изменить привязку", 1);

        SetParam("мод_Х_Диаметр", frameParams.StirrupRebarDiameter);
        SetParam("мод_Х_Длина отгибов", _bendLegth);
        SetParam("мод_Х_Шаг", frameParams.StirrupStep);
        SetParam("мод_Х_Количество", frameParams.GetFrameLength() / frameParams.StirrupStep + 1);

        if (frameParams.HostSlab.SlabInstance.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralFoundation) {
            SetParam("обр_ФОП_Группа КР", "ФП_Каркасы_Продавливание");
        } else {
            SetParam("обр_ФОП_Группа КР", "ПП_Каркасы_Продавливание");
        }

        CopyParametersFromSlab(frameParams.HostSlab.SlabInstance);
    }

    private void SetParam(string paramName, string paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new NullReferenceException($"Параметр {paramName} не существет");
        }

        param.Set(paramValue);
    }

    private void SetParam(string paramName, int paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new NullReferenceException($"Параметр {paramName} не существет");
        }

        param.Set(paramValue);
    }

    private void SetParam(string paramName, double paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new NullReferenceException($"Параметр {paramName} не существет");
        }

        param.Set(paramValue);
    }

    private T GetParam<T>(Element hostElement, string paramName){
        Parameter param = hostElement.LookupParameter(paramName);

        if(param == null) {
            throw new NullReferenceException($"Параметр {paramName} не существет");
        }

        return (T)param.AsObject();
    }

    /// <summary>
    /// Копируем параметры из опалубки
    /// </summary>
    /// <param name="hostElement"></param>
    private void CopyParametersFromSlab(Element hostElement) {
        try {
            FrameInstance.SetParamValue(SharedParamsConfig.Instance.BuildingWorksBlock, hostElement.GetParamValue<string>(SharedParamsConfig.Instance.BuildingWorksBlock));
            FrameInstance.SetParamValue(SharedParamsConfig.Instance.BuildingWorksSection, hostElement.GetParamValue<string>(SharedParamsConfig.Instance.BuildingWorksSection));
            FrameInstance.SetParamValue(SharedParamsConfig.Instance.BuildingWorksLevel, hostElement.GetParamValue<string>(SharedParamsConfig.Instance.BuildingWorksLevel));

            SetParam("обр_ФОП_Раздел проекта", GetParam<string>(hostElement, "обр_ФОП_Раздел проекта"));
            SetParam("обр_ФОП_Марка ведомости расхода", GetParam<string>(hostElement, "обр_ФОП_Марка ведомости расхода"));
            SetParam("обр_ФОП_Орг. уровень", GetParam<double>(hostElement, "обр_ФОП_Орг. уровень"));
        }
        catch { }
    }
}
