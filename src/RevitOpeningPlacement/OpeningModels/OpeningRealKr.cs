using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс, обозначающий чистовое отверстие КР, идущее на чертежи. 
/// Использовать для обертки проемов из активного файла КР
/// </summary>
internal class OpeningRealKr : OpeningRealByMep, IEquatable<OpeningRealKr> {
    public OpeningRealKr(FamilyInstance openingReal) : base(openingReal) {
        Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrDiameter);
        Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrInWallWidth);
        Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrInWallHeight);
        Comment = _familyInstance.GetParamValueStringOrDefault(
            SystemParamsConfig.Instance.CreateRevitParam(
                _familyInstance.Document,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
            string.Empty);
    }


    /// <summary>
    /// Диаметр в мм, если есть
    /// </summary>
    public string Diameter { get; } = string.Empty;

    /// <summary>
    /// Ширина в мм, если есть
    /// </summary>
    public string Width { get; } = string.Empty;

    /// <summary>
    /// Высота в мм, если есть
    /// </summary>
    public string Height { get; } = string.Empty;

    /// <summary>
    /// Комментарий
    /// </summary>
    public string Comment { get; } = string.Empty;

    /// <summary>
    /// Статус текущего отверстия относительно полученных заданий
    /// <para>Для обновления использовать метод <see cref="UpdateStatus"/></para>
    /// </summary>
    public OpeningRealStatus Status { get; private set; } = OpeningRealStatus.NotActual;


    public override bool Equals(object obj) {
        return (obj != null) && (obj is OpeningRealKr opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return Id.GetIdValue();
    }

    public bool Equals(OpeningRealKr other) {
        return (other != null) && (Id == other.Id);
    }

    public override Solid GetSolid() {
        return GetOpeningSolid();
    }

    public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return _boundingBox;
    }

    /// <summary>
    /// Обновляет свойство <see cref="Status"/>
    /// </summary>
    /// <param name="mepLinkElementsProviders">Коллекция связей с элементами ВИС и заданиями на отверстиями</param>
    public void UpdateStatus(ICollection<IMepLinkElementsProvider> mepLinkElementsProviders) {
        try {
            Status = DetermineStatus(mepLinkElementsProviders);
        } catch(Exception ex) when(
            ex is Autodesk.Revit.Exceptions.ApplicationException
            or NullReferenceException
            or ArgumentNullException) {
            Status = OpeningRealStatus.Invalid;
        }
    }

    /// <summary>
    /// Обновляет свойство <see cref="Status"/>
    /// </summary>
    /// <param name="arLinkElementsProviders">АР связи с входящими заданиями</param>
    public void UpdateStatus(ICollection<IConstructureLinkElementsProvider> arLinkElementsProviders) {
        try {
            var thisOpeningRealSolid = GetSolid();
            var thisOpeningRealSolidAfterIntersection = thisOpeningRealSolid;

            foreach(var link in arLinkElementsProviders) {
                thisOpeningRealSolidAfterIntersection = SubtractLinkOpenings(
                    link,
                    thisOpeningRealSolidAfterIntersection,
                    out bool openingIsNotActual);
                if(openingIsNotActual) {
                    Status = OpeningRealStatus.NotActual;
                    return;
                }
            }
            double volumeRatio = GetSolidsVolumesRatio(thisOpeningRealSolid, thisOpeningRealSolidAfterIntersection);
            Status = GetStatusByVolumeRatio(volumeRatio);
        } catch(Exception ex) when(
            ex is Autodesk.Revit.Exceptions.ApplicationException
            or NullReferenceException
            or ArgumentNullException) {
            Status = OpeningRealStatus.Invalid;
        }
    }

    /// <summary>
    /// Возвращает коэффициент, показывающий, какую часть исходного солида пересекают элементы из связей
    /// </summary>
    /// <param name="thisOpeningRealSolid">Исходный солид текущего чистового отверстия</param>
    /// <param name="thisOpeningRealSolidAfterIntersection">
    /// Солид текущего чистового отверстия, 
    /// после вычтенных солидов элементов из связей, которые пересекают это отверстие
    /// </param>
    /// <returns>
    /// 0 - элементы из связей на 100% пересекают солид текущего чистового отверстия, 
    /// 1 - солид текущего чистового отверстия не пересекается ни с одним элементом из связи
    /// </returns>
    private double GetSolidsVolumesRatio(Solid thisOpeningRealSolid, Solid thisOpeningRealSolidAfterIntersection) {
        return thisOpeningRealSolid.Volume == 0 ? 1 : 1 - thisOpeningRealSolidAfterIntersection.Volume / thisOpeningRealSolid.Volume;
    }

    /// <summary>
    /// Возвращает коллекцию всех заданий на отверстия из связи АР, которые пересекаются с текущим отверстием КР
    /// </summary>
    /// <param name="link">Связь АР</param>
    /// <param name="thisOpeningSolidForSubtraction">
    /// Солид текущего отверстия в исходных координатах активного документа, 
    /// из которого вычтены пересекающие его задания на отверстия</param>
    /// <param name="linkOpeningsIntersectConstructions">
    /// Флаг, показывающий, 
    /// полностью ли текущее чистовое отверстие закрывает собой пересекающие его задания на отверстия</param>
#pragma warning disable 0618
    private Solid SubtractLinkOpenings(
        IConstructureLinkElementsProvider link,
        Solid thisOpeningSolidForSubtraction,
        out bool linkOpeningsIntersectConstructions) {

        var thisOpeningRealSolid = GetSolid();
        var thisOpeningSolidInLinkCoordinates = SolidUtils.CreateTransformed(
            thisOpeningRealSolid, link.DocumentTransform.Inverse);
        var thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ()
            .GetTransformedBoundingBox(link.DocumentTransform.Inverse);

        ICollection<Solid> intersectingTasksSolidsInActiveDocCoordinates = link
            .GetOpeningsReal()
            .Where(openingTask => openingTask.IntersectsSolid(
                thisOpeningSolidInLinkCoordinates,
                thisBBoxInLinkCoordinates))
            .Select(task => SolidUtils.CreateTransformed(task.GetSolid(), link.DocumentTransform))
            .ToHashSet();

        var solidAfterSubtraction = thisOpeningSolidForSubtraction;
        linkOpeningsIntersectConstructions = false;
        foreach(var solid in intersectingTasksSolidsInActiveDocCoordinates) {
            try {
                solidAfterSubtraction = BooleanOperationsUtils.ExecuteBooleanOperation(
                    solidAfterSubtraction,
                    solid,
                    BooleanOperationsType.Difference);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) { }
            try {
                linkOpeningsIntersectConstructions =
                    linkOpeningsIntersectConstructions
                    || (BooleanOperationsUtils.ExecuteBooleanOperation(
                        solid,
                        thisOpeningRealSolid,
                        BooleanOperationsType.Difference)?.Volume > 0);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) { }
        }
        return solidAfterSubtraction;
    }
#pragma warning restore 0618

    /// <summary>
    /// Возвращает статус текущего чистового отверстия по коэффициенту пересекаемого объема.
    /// </summary>
    /// <param name="volumeRatio">
    /// Отношение объема солида текущего чистового отверстия, который пересекается с элементами из связей, 
    /// к исходному объему этого солида.
    /// <para>
    /// 0 - элементы из связей на 100% пересекают солид текущего чистового отверстия, 
    /// 1 - солид текущего чистового отверстия не пересекается ни с одним элементом из связи
    /// </para>
    /// </param>
    private OpeningRealStatus GetStatusByVolumeRatio(double volumeRatio) {
        return volumeRatio < 0.01 ? OpeningRealStatus.Empty : volumeRatio < 0.5 ? OpeningRealStatus.TooBig : OpeningRealStatus.Correct;
    }
}
