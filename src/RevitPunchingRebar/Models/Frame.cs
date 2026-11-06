using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class Frame {
    // Параметры семейства каркаса
    private const string _longRebarCodeParam = "обр_ПР_Код металлопроката";
    private const string _stirrupCodeParam = "обр_Х_Код металлопроката";
    private const string _widthStepParam = "мод_ПР_Шаг по ширине";
    private const string _heightStepParam = "мод_ПР_Шаг по высоте";
    private const string _anchorageTop1Param = "мод_ПР_Анкеровка_Верх_1";
    private const string _anchorageTop2Param = "мод_ПР_Анкеровка_Верх_2";
    private const string _anchorageBottom1Param = "мод_ПР_Анкеровка_Низ_1";
    private const string _anchorageBottom2Param = "мод_ПР_Анкеровка_Низ_2";
    private const string _stirrupChangeBindingParam = "мод_Х_Изменить привязку";
    private const string _stirrupDiameterParam = "мод_Х_Диаметр";
    private const string _stirrupBendLengthParam = "мод_Х_Длина отгибов";
    private const string _stirrupStepParam = "мод_Х_Шаг";
    private const string _stirrupCountParam = "мод_Х_Количество";

    // Параметры, общие для каркаса и плиты
    private const string _krGroupParam = "обр_ФОП_Группа КР";
    private const string _projectSectionParam = "обр_ФОП_Раздел проекта";
    private const string _consumptionMarkParam = "обр_ФОП_Марка ведомости расхода";
    private const string _orgLevelParam = "обр_ФОП_Орг. уровень";

    // Значения группы КР для каркасов в фундаментной плите и в плите перекрытия
    private const string _foundationSlabKrGroup = "ФП_Каркасы_Продавливание";
    private const string _floorSlabKrGroup = "ПП_Каркасы_Продавливание";

    // Значение параметра "мод_Х_Изменить привязку"
    private const int _stirrupChangeBindingValue = 1;

    // Длина выпуска продольных стержней за крайние хомуты, мм
    private const double _anchorageLengthMm = 50;

    // Длина отгиба хомута, мм
    private const double _stirrupBendLengthMm = 90;

    // Диаметр продольных стержней каркаса, мм
    private const double _longRebarDiameterMm = 10;

    // Класс продольных стержней каркаса
    private const RebarClass _longRebarClass = RebarClass.A500C;

    // Диаметр продольных стержней каркаса
    public readonly double LongRebarDiameter = UnitUtilsHelper.FromMm(_longRebarDiameterMm);

    private readonly double _anchorageLength = UnitUtilsHelper.FromMm(_anchorageLengthMm);
    private readonly double _stirrupBendLength = UnitUtilsHelper.FromMm(_stirrupBendLengthMm);

    internal Frame(FamilyInstance familyInstance, IFrameParams frameParams) {
        FrameInstance = familyInstance;

        //Заполняем необходимые параметры каркаса
        SetParams(frameParams);
    }

    public FamilyInstance FrameInstance { get; set; }

    public XYZ GetLocation() {
        XYZ location = ((LocationPoint) FrameInstance.Location).Point;
        double halfWidthStep = 0.5 * GetParam<double>(FrameInstance, _widthStepParam);

        XYZ currentLocation = new XYZ
                    (
                        location.X + FrameInstance.FacingOrientation.X * halfWidthStep,
                        location.Y + FrameInstance.FacingOrientation.Y * halfWidthStep,
                        location.Z
                    );

        return currentLocation;
    }

    private void SetParams(IFrameParams frameParams) {
        SetParam(_longRebarCodeParam, (int) _longRebarClass);
        SetParam(_stirrupCodeParam, (int) frameParams.StirrupRebarClass);

        SetParam(_widthStepParam, frameParams.FrameWidth - frameParams.StirrupRebarDiameter - LongRebarDiameter);
        SetParam(_heightStepParam, frameParams.GetFrameHeight(LongRebarDiameter));

        SetParam(_anchorageTop1Param, _anchorageLength);
        SetParam(_anchorageTop2Param, _anchorageLength);
        SetParam(_anchorageBottom1Param, _anchorageLength);
        SetParam(_anchorageBottom2Param, _anchorageLength);

        SetParam(_stirrupChangeBindingParam, _stirrupChangeBindingValue);

        SetParam(_stirrupDiameterParam, frameParams.StirrupRebarDiameter);
        SetParam(_stirrupBendLengthParam, _stirrupBendLength);
        SetParam(_stirrupStepParam, frameParams.StirrupStep);
        SetParam(_stirrupCountParam, frameParams.GetFrameLength() / frameParams.StirrupStep + 1);

        if(frameParams.HostSlab.SlabInstance.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralFoundation) {
            SetParam(_krGroupParam, _foundationSlabKrGroup);
        } else {
            SetParam(_krGroupParam, _floorSlabKrGroup);
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

    private T GetParam<T>(Element hostElement, string paramName) {
        Parameter param = hostElement.LookupParameter(paramName);

        if(param == null) {
            throw new NullReferenceException($"Параметр {paramName} не существет");
        }

        return (T) param.AsObject();
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

            SetParam(_projectSectionParam, GetParam<string>(hostElement, _projectSectionParam));
            SetParam(_consumptionMarkParam, GetParam<string>(hostElement, _consumptionMarkParam));
            SetParam(_orgLevelParam, GetParam<double>(hostElement, _orgLevelParam));
        } catch { }
    }
}
