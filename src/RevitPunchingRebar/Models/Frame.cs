using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class Frame {
    private readonly FrameParameterSettings _parameterSettings;
    private readonly ILocalizationService _localizationService;

    // Значения группы КР для каркасов в фундаментной плите и в плите перекрытия
    private const string _foundationSlabKrGroup = "ФП_Каркасы_Продавливание";
    private const string _floorSlabKrGroup = "ПП_Каркасы_Продавливание";

    // Значение параметра "мод_Х_Изменить привязку"
    private const int _stirrupChangeBindingValue = 1;

    // Класс продольных стержней каркаса
    private const RebarClass _longRebarClass = RebarClass.A500C;

    public Frame(FamilyInstance familyInstance, IFrameParams frameParams,
        ILocalizationService localizationService) {
        FrameInstance = familyInstance;
        _localizationService = localizationService;
        _parameterSettings = frameParams.ParameterSettings;

        //Заполняем необходимые параметры каркаса
        SetParams(frameParams);
    }

    public FamilyInstance FrameInstance { get; }

    public XYZ GetLocation() {
        XYZ location = ((LocationPoint) FrameInstance.Location).Point;
        XYZ direction = FrameInstance.FacingOrientation;
        string widthParamName = _parameterSettings.GetName(FrameParameterRole.WidthStep);
        double halfWidthStep = 0.5 * FrameInstance.GetParamValue<double>(widthParamName);

        return new XYZ(
            location.X + direction.X * halfWidthStep,
            location.Y + direction.Y * halfWidthStep,
            location.Z);
    }

    private void SetParams(IFrameParams frameParams) {
        ApplyParameter(FrameParameterRole.LongRebarCode, name =>
            FrameInstance.SetParamValue(name, (int) _longRebarClass));
        // В каркасе две пары продольных стержней; выбранный диаметр задается обеим парам.
        ApplyParameter(FrameParameterRole.LongRebarDiameterTop, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarDiameter));
        ApplyParameter(FrameParameterRole.LongRebarDiameterBottom, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarDiameter));
        ApplyParameter(FrameParameterRole.StirrupCode, name =>
            FrameInstance.SetParamValue(name, (int) frameParams.StirrupRebarClass));

        ApplyParameter(FrameParameterRole.WidthStep, name =>
            FrameInstance.SetParamValue(
                name, frameParams.FrameWidth - frameParams.StirrupRebarDiameter - frameParams.LongRebarDiameter));
        ApplyParameter(FrameParameterRole.HeightStep, name =>
            FrameInstance.SetParamValue(name, frameParams.GetFrameHeight()));

        ApplyParameter(FrameParameterRole.AnchorageTop1, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarAnchorageLength));
        ApplyParameter(FrameParameterRole.AnchorageTop2, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarAnchorageLength));
        ApplyParameter(FrameParameterRole.AnchorageBottom1, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarAnchorageLength));
        ApplyParameter(FrameParameterRole.AnchorageBottom2, name =>
            FrameInstance.SetParamValue(name, frameParams.LongRebarAnchorageLength));

        ApplyParameter(FrameParameterRole.StirrupChangeBinding, name =>
            FrameInstance.SetParamValue(name, _stirrupChangeBindingValue));

        ApplyParameter(FrameParameterRole.StirrupDiameter, name =>
            FrameInstance.SetParamValue(name, frameParams.StirrupRebarDiameter));
        ApplyParameter(FrameParameterRole.StirrupBendLength, name =>
            FrameInstance.SetParamValue(name, frameParams.StirrupBendLength));
        ApplyParameter(FrameParameterRole.StirrupStep, name =>
            FrameInstance.SetParamValue(name, frameParams.StirrupStep));
        ApplyParameter(FrameParameterRole.StirrupCount, name =>
            FrameInstance.SetParamValue(name, frameParams.GetFrameLength() / frameParams.StirrupStep + 1));

        if(frameParams.HostSlab.SlabInstance.InAnyCategory(BuiltInCategory.OST_StructuralFoundation)) {
            ApplyParameter(FrameParameterRole.KrGroup, name =>
                FrameInstance.SetParamValue(name, _foundationSlabKrGroup));
        } else {
            ApplyParameter(FrameParameterRole.KrGroup, name =>
                FrameInstance.SetParamValue(name, _floorSlabKrGroup));
        }

        CopyParametersFromSlab(frameParams.HostSlab.SlabInstance);
    }

    /// <summary>
    /// Копирует параметры из опалубки.
    /// </summary>
    /// <remarks>
    /// Координаты СМР копируются при наличии. Ошибка пользовательского назначения
    /// передается в итоговый отчет вместе с именем и назначением параметра.
    /// </remarks>
    /// <param name="hostElement">Плита, в которой расставляются каркасы.</param>
    private void CopyParametersFromSlab(Element hostElement) {
        string[] paramNames = [
            SharedParamsConfig.Instance.BuildingWorksBlock.Name,
            SharedParamsConfig.Instance.BuildingWorksSection.Name,
            SharedParamsConfig.Instance.BuildingWorksLevel.Name
        ];

        foreach(string paramName in paramNames) {
            CopyParam(hostElement, paramName, false);
        }

        foreach(FrameParameterDefinition definition in FrameParameterCatalog.Definitions) {
            if(definition.IsCopy) {
                ApplyParameter(definition.Role, name => CopyParam(hostElement, name, true));
            }
        }
    }

    private void CopyParam(Element sourceElement, string paramName, bool required) {
        bool hasSource = sourceElement.IsExistsParam(paramName);
        bool hasTarget = FrameInstance.IsExistsParam(paramName);
        if(!hasSource || !hasTarget) {
            if(required) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Report.CopyParameterMissing"));
            }

            return;
        }

        Parameter sourceParam = sourceElement.GetParam(paramName);
        Parameter targetParam = FrameInstance.GetParam(paramName);
        if(!required && (targetParam.IsReadOnly || sourceParam.StorageType != targetParam.StorageType)) {
            return;
        }

        if(sourceParam.HasValue) {
            targetParam.Set(sourceParam);
        }
    }

    /// <summary>Добавляет назначение и имя к ошибке фактической записи или копирования параметра.</summary>
    private void ApplyParameter(FrameParameterRole role, Action<string> action) {
        string name = _parameterSettings.GetName(role);
        try {
            action(name);
        } catch(Exception ex) when(ex is Autodesk.Revit.Exceptions.ApplicationException
                                     || ex is ArgumentException || ex is InvalidOperationException) {
            string title = _localizationService.GetLocalizedString($"Parameters.{role}.Title");
            throw new InvalidOperationException(string.Format(
                _localizationService.GetLocalizedString("Report.ParameterError"), title, name, ex.Message), ex);
        }
    }
}
