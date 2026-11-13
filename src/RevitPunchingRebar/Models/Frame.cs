using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class Frame {
    // Параметры семейства каркаса
    private const string _longRebarCodeParam = "обр_ПР_Код металлопроката";
    private const string _longRebarDiameterTopParam = "мод_ПР_Диаметр_Верх";
    private const string _longRebarDiameterBottomParam = "мод_ПР_Диаметр_Низ";
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

    // Класс продольных стержней каркаса
    private const RebarClass _longRebarClass = RebarClass.A500C;

    /// <summary>
    /// Параметры, которые обязательно должны быть у экземпляра семейства каркаса.
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredParamNames = [
        _longRebarCodeParam,
        _longRebarDiameterTopParam,
        _longRebarDiameterBottomParam,
        _stirrupCodeParam,
        _widthStepParam,
        _heightStepParam,
        _anchorageTop1Param,
        _anchorageTop2Param,
        _anchorageBottom1Param,
        _anchorageBottom2Param,
        _stirrupChangeBindingParam,
        _stirrupDiameterParam,
        _stirrupBendLengthParam,
        _stirrupStepParam,
        _stirrupCountParam,
        _krGroupParam
    ];

    public Frame(FamilyInstance familyInstance, IFrameParams frameParams) {
        FrameInstance = familyInstance;

        //Заполняем необходимые параметры каркаса
        SetParams(frameParams);
    }

    public FamilyInstance FrameInstance { get; }

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
        // В каркасе две пары продольных стержней; выбранный диаметр задается обеим парам.
        SetParam(_longRebarDiameterTopParam, frameParams.LongRebarDiameter);
        SetParam(_longRebarDiameterBottomParam, frameParams.LongRebarDiameter);
        SetParam(_stirrupCodeParam, (int) frameParams.StirrupRebarClass);

        SetParam(_widthStepParam,
                 frameParams.FrameWidth - frameParams.StirrupRebarDiameter - frameParams.LongRebarDiameter);
        SetParam(_heightStepParam, frameParams.GetFrameHeight());

        SetParam(_anchorageTop1Param, frameParams.LongRebarAnchorageLength);
        SetParam(_anchorageTop2Param, frameParams.LongRebarAnchorageLength);
        SetParam(_anchorageBottom1Param, frameParams.LongRebarAnchorageLength);
        SetParam(_anchorageBottom2Param, frameParams.LongRebarAnchorageLength);

        SetParam(_stirrupChangeBindingParam, _stirrupChangeBindingValue);

        SetParam(_stirrupDiameterParam, frameParams.StirrupRebarDiameter);
        SetParam(_stirrupBendLengthParam, frameParams.StirrupBendLength);
        SetParam(_stirrupStepParam, frameParams.StirrupStep);
        SetParam(_stirrupCountParam, frameParams.GetFrameLength() / frameParams.StirrupStep + 1);

        if(frameParams.HostSlab.SlabInstance.InAnyCategory(BuiltInCategory.OST_StructuralFoundation)) {
            SetParam(_krGroupParam, _foundationSlabKrGroup);
        } else {
            SetParam(_krGroupParam, _floorSlabKrGroup);
        }

        CopyParametersFromSlab(frameParams.HostSlab.SlabInstance);
    }

    private void SetParam(string paramName, string paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new InvalidOperationException($"Параметр {paramName} не существует");
        }

        param.Set(paramValue);
    }

    private void SetParam(string paramName, int paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new InvalidOperationException($"Параметр {paramName} не существует");
        }

        param.Set(paramValue);
    }

    private void SetParam(string paramName, double paramValue) {
        Parameter param = FrameInstance.LookupParameter(paramName);

        if(param == null) {
            throw new InvalidOperationException($"Параметр {paramName} не существует");
        }

        param.Set(paramValue);
    }

    private T GetParam<T>(Element hostElement, string paramName) {
        Parameter param = hostElement.LookupParameter(paramName);

        if(param == null) {
            throw new InvalidOperationException($"Параметр {paramName} не существует");
        }

        return (T) param.AsObject();
    }

    /// <summary>
    /// Копирует параметры из опалубки.
    /// </summary>
    /// <remarks>
    /// Каждый параметр копируется независимо: если параметра нет у плиты или у каркаса,
    /// он пропускается, а остальные копируются.
    /// </remarks>
    /// <param name="hostElement">Плита, в которой расставляются каркасы.</param>
    private void CopyParametersFromSlab(Element hostElement) {
        string[] paramNames = [
            SharedParamsConfig.Instance.BuildingWorksBlock.Name,
            SharedParamsConfig.Instance.BuildingWorksSection.Name,
            SharedParamsConfig.Instance.BuildingWorksLevel.Name,
            _projectSectionParam,
            _consumptionMarkParam,
            _orgLevelParam
        ];

        foreach(string paramName in paramNames) {
            CopyParam(hostElement, paramName);
        }
    }

    private void CopyParam(Element sourceElement, string paramName) {
        Parameter sourceParam = sourceElement.LookupParameter(paramName);
        Parameter targetParam = FrameInstance.LookupParameter(paramName);

        if(sourceParam == null
           || targetParam == null
           || targetParam.IsReadOnly
           || !sourceParam.HasValue
           || sourceParam.StorageType != targetParam.StorageType) {
            return;
        }

        switch(sourceParam.StorageType) {
            case StorageType.String:
                targetParam.Set(sourceParam.AsString());
                break;
            case StorageType.Double:
                targetParam.Set(sourceParam.AsDouble());
                break;
            case StorageType.Integer:
                targetParam.Set(sourceParam.AsInteger());
                break;
            case StorageType.ElementId:
                targetParam.Set(sourceParam.AsElementId());
                break;
        }
    }
}
