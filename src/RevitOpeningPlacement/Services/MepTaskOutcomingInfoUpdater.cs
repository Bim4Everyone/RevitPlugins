using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Класс для обновления информации по исходящим заданиям на отверстия от ВИС из активного файла
    /// </summary>
    internal class MepTaskOutcomingInfoUpdater : IOpeningInfoUpdater<OpeningMepTaskOutcoming> {
        /// <summary>
        /// Репозиторий активного файла
        /// </summary>
        private readonly RevitRepository _revitRepository;
        private readonly ISolidProviderUtils _solidProviderUtils;

        /// <summary>
        /// Все исходящие задания на отверстия от ВИС из активного файла
        /// </summary>
        private readonly ICollection<ElementId> _outcomingTasksIds;

        /// <summary>
        /// Все элементы ВИС из активного файла
        /// </summary>
        private readonly ICollection<ElementId> _mepElementsIds;

        /// <summary>
        /// Все связи с конструкциями, загруженные в активный файл
        /// </summary>
        private readonly ICollection<IConstructureLinkElementsProvider> _constructureLinks;

        /// <summary>
        /// Допустимое расстояние между экземплярами семейств заданий на отверстия, при котором считается, что они размещены в одном и том же месте
        /// </summary>
        private readonly double _distance3dTolerance = Math.Sqrt(3 * XYZExtension.FeetRound * XYZExtension.FeetRound);

        /// <summary>
        /// Допустимый объем, равный кубу <see cref="_distance3dTolerance"/>
        /// </summary>
        private readonly double _volumeTolerance;

        private readonly Dictionary<ElementId, ICollection<ElementId>> _intersectingMepElementsCache;


        public MepTaskOutcomingInfoUpdater(RevitRepository revitRepository, ISolidProviderUtils solidProviderUtils) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _solidProviderUtils = solidProviderUtils ?? throw new ArgumentNullException(nameof(solidProviderUtils));

            _outcomingTasksIds = GetOpeningsMepTasksOutcoming(_revitRepository);
            _mepElementsIds = revitRepository.GetMepElementsIds();
            _constructureLinks = GetLinkProviders(revitRepository);
            _intersectingMepElementsCache = new Dictionary<ElementId, ICollection<ElementId>>();

            _volumeTolerance = _distance3dTolerance * _distance3dTolerance * _distance3dTolerance;
        }


        /// <summary>
        /// Обновляет <see cref="OpeningMepTaskOutcoming.Status">статус</see> и <see cref="OpeningMepTaskOutcoming.Host">хост</see>
        /// исходящего задания на отверстие от ВИС из активного файла.
        /// </summary>
        /// <param name="outcomingTask">Исходящее задание н отверстие от ВИС из активного файла.</param>
        public void UpdateInfo(OpeningMepTaskOutcoming outcomingTask) {
            if(!outcomingTask.IsRemoved) {
                var openingSolid = outcomingTask.GetSolid();
                if((openingSolid is null) || (openingSolid.Volume < _volumeTolerance)) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }

                if(IsManuallyPlaced(outcomingTask)) {
                    FindAndSetHost(openingSolid, _constructureLinks, outcomingTask);
                    outcomingTask.Status = OpeningTaskOutcomingStatus.ManuallyPlaced;
                    return;
                }

                if(ThisOpeningTaskIsNotActual(openingSolid, _constructureLinks, _mepElementsIds, outcomingTask)) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.NotActual;
                    return;
                }

                var intersectingOpeningIds = GetIntersectingOpeningsTasks(
                    openingSolid,
                    _outcomingTasksIds,
                    outcomingTask);
                if(intersectingOpeningIds.Count > 0) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Intersects;
                    return;
                } else {
                    _outcomingTasksIds.Remove(outcomingTask.Id);
                }

                try {
                    Solid openingSolidAfterIntersection = GetOpeningAndMepsSolidsDifference(
                        openingSolid,
                        _mepElementsIds,
                        outcomingTask);
                    if(openingSolidAfterIntersection is null) {
                        outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                        return;
                    }
                    double volumeRatio =
                        (openingSolid.Volume - openingSolidAfterIntersection.Volume) / openingSolid.Volume;
                    outcomingTask.Status = GetOpeningTaskOutcomingStatus(volumeRatio);
                    return;

                } catch(InvalidOperationException) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }
            } else {
                outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                return;
            }
        }

        /// <summary>
        /// Возвращает статус задания на отверстие по отношению объема пересечения задания на отверстие с элементом инженерной системы к исходному объему задания на отверстие
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Исключение, если коэффициент отношения объемов меньше 0 или больше 1</exception>
        private OpeningTaskOutcomingStatus GetOpeningTaskOutcomingStatus(double volumeRatio) {
            if((volumeRatio < 0) || (volumeRatio > 1)) {
                throw new ArgumentOutOfRangeException(
                    $"Значение параметра {nameof(volumeRatio)} должно находиться в интервале [0; 1]");
            }
            OpeningTaskOutcomingStatus status;
            if(0.95 <= volumeRatio) {
                status = OpeningTaskOutcomingStatus.TooSmall;
            } else if((0.2 <= volumeRatio) && (volumeRatio < 0.95)) {
                status = OpeningTaskOutcomingStatus.Correct;
            } else if((0.001 <= volumeRatio) && (volumeRatio < 0.2)) {
                status = OpeningTaskOutcomingStatus.TooBig;
            } else {
                status = OpeningTaskOutcomingStatus.NotActual;
            }
            return status;
        }

        /// <summary>
        /// Возвращает солид, образованный вычитанием из исходного солида задания на отверстие всех пересекающих его элементов инженерных систем
        /// </summary>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие</param>
        /// <param name="allMepElementsIds">Коллекция Id всех элементов инженерных систем из файла, в котором размещено задание на отверстие</param>
        /// <exception cref="InvalidOperationException">Исключение, если не удалось выполнить вычитание солида отверстия и солида элемента инженерной системы</exception>
        private Solid GetOpeningAndMepsSolidsDifference(
            Solid thisOpeningSolid,
            ICollection<ElementId> allMepElementsIds,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            var intersectingMepSolids = GetIntersectingMepSolids(thisOpeningSolid, allMepElementsIds, mepTaskOutcoming);
            Solid openingSolidAfterIntersection = thisOpeningSolid;
            foreach(var mepSolid in intersectingMepSolids) {
                //здесь определяется солид, образованный вычитанием из исходного солида задания на отверстие всех пересекающих его элементов инженерных систем
                try {
                    openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                        openingSolidAfterIntersection,
                        mepSolid, BooleanOperationsType.Difference);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    try {
                        // если один солид касается другого солида изнутри, то будет исключение InvalidOperationException
                        // здесь производится попытка слегка подвинуть один из солидов, чтобы избежать этого касания
                        double coordinateOffset = 0.001;
                        var mepTransformedSolid = SolidUtils.CreateTransformed(
                            mepSolid,
                            Transform.CreateTranslation(
                                new XYZ(coordinateOffset, coordinateOffset, coordinateOffset)));
                        openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                            openingSolidAfterIntersection,
                            mepTransformedSolid,
                            BooleanOperationsType.Difference);

                    } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                        try {
                            // здесь производится попытка слегка подвинуть один из солидов в другую сторону
                            double coordinateOffset = -0.001;
                            var mepTransformedSolid = SolidUtils.CreateTransformed(
                                mepSolid,
                                Transform.CreateTranslation(
                                    new XYZ(coordinateOffset, coordinateOffset, coordinateOffset)));
                            openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                                openingSolidAfterIntersection,
                                mepTransformedSolid,
                                BooleanOperationsType.Difference);

                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            return openingSolidAfterIntersection;
        }


        /// <summary>
        /// Возвращает список солидов элементов инженерных систем, которые пересекаются с данным заданием на отверстие, из текущего документа
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="allMepElementsIds">Коллекция Id всех элементов инженерных систем из файла, в котором размещено задание на отверстие</param>
        private ICollection<Solid> GetIntersectingMepSolids(
            Solid thisOpeningTaskSolid,
            ICollection<ElementId> allMepElementsIds,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                return Array.Empty<Solid>();
            } else {
                return GetIntersectingMepElementsIds(thisOpeningTaskSolid, allMepElementsIds, mepTaskOutcoming)
                    .Select(elementId => _revitRepository.Doc.GetElement(elementId).GetSolid())
                    .ToHashSet();
            }
        }

        /// <summary>
        /// Возвращает Id всех исходящих заданий на отверстия, которые пересекаются с данным заданием на отверстие из текущего документа
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="allOpeningTasksInDoc">Id всех экземпляров семейств заданий на отверстия из активного документа ревита для проверки</param>
        /// <returns>Коллекция Id экземпляров семейств исходящих заданий на отверстия, которые пересекаются с текущим заданием на отверстие</returns>
        private ICollection<ElementId> GetIntersectingOpeningsTasks(
            Solid thisOpeningTaskSolid,
            ICollection<ElementId> allOpeningTasksInDoc,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0) || (!allOpeningTasksInDoc.Any())) {
                return Array.Empty<ElementId>();
            } else {
                return new FilteredElementCollector(_revitRepository.Doc, allOpeningTasksInDoc)
                    .Excluding(new ElementId[] { mepTaskOutcoming.Id })
                    .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningTaskSolid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(thisOpeningTaskSolid))
                    .ToElementIds();
            }
        }

        /// <summary>
        /// Проверяет, является ли данное задание на отверстие НЕ актуальным. Если задание не актуально, возвращается False, иначе True.
        /// Метод может установить только НЕкорректность задания, но корректность абсолютно точно подтвердить не может.
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="constructureLinkElementsProviders">Коллекция объектов-оберток связанных файлов с элементами конструкций: стенами, перекрытиями и т.п.</param>
        /// <returns>True - задание точно некорректно; False - задание не некорректно, но утверждать, что оно корректно нельзя</returns>
        private bool ThisOpeningTaskIsNotActual(
            Solid thisOpeningTaskSolid,
            ICollection<IConstructureLinkElementsProvider> constructureLinkElementsProviders,
            ICollection<ElementId> allMepElementsIds,
            OpeningMepTaskOutcoming mepTaskOutcoming
            ) {

            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                // геометрия задания на отверстие не корректна - задание не актуально
                return true;
            }
            if(constructureLinkElementsProviders is null) {
                throw new ArgumentNullException(nameof(constructureLinkElementsProviders));
            }
            if(allMepElementsIds is null) {
                throw new ArgumentNullException(nameof(allMepElementsIds));
            }

            bool mepElementsNotIntersectThisTask = false;
            var mepElementsIntersectingThisTask = GetIntersectingMepElementsIds(
                thisOpeningTaskSolid,
                allMepElementsIds,
                mepTaskOutcoming);
            if(mepElementsIntersectingThisTask.Count == 0) {
                // задание на отверстие не пересекается ни с одним элементом инженерной системы - задание не актуально
                mepElementsNotIntersectThisTask = true;
            }

            // проверка на то, что:
            // во-первых есть конструкции в связанных файлах, внутри которых расположено задание на отверстие,
            // во-вторых, что ни один элемент ВИС, проходящий через текущее задание на отверстие не пересекается с этими конструкциями
            foreach(var link in constructureLinkElementsProviders) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    thisOpeningTaskSolid, link,
                    out ICollection<IOpeningReal> intersectingOpenings,
                    mepTaskOutcoming);
                if(hostConstructions.Count > 0) {
                    return mepElementsNotIntersectThisTask
                        || MepElementsIntersectConstructionsOrOpenings(
                            thisOpeningTaskSolid,
                            mepElementsIntersectingThisTask,
                            hostConstructions,
                            intersectingOpenings,
                            link,
                            mepTaskOutcoming
                            );
                } else {
                    // если не найдены конструкции, которые можно считать хостами текущего задания на отверстие,
                    // то либо задание на отверстие висит в воздухе, либо задание на отверстие пересекается с другой связью
                    continue;
                }
            }

            // корректная ситуация не найдена, отверстие считается не актуальным
            return true;
        }

        /// <summary>
        /// Проверяет, пересекаются ли элементы ВИС из текущего файла с конструкциями или чистовыми отверстиями, причем место пересечения находится вне солида задания на отверстие.
        /// <para>При этом проверяемые элементы ВИС, а также проверяемые конструкции и чистовые отверстия из связи должны пересекать солид текущего задания на отверстие</para>
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="intersectingMepElementsIds">Элементы ВИС из текущего файла с заданиями на отверстия, которые пересекают солид текущего задания на отверстие</param>
        /// <param name="intersectingLinkConstructions">Конструкции из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="intersectingLinkOpeningsReal">Чистовые отверстия из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <returns>True, если найдено пересечение элементов ВИС и конструкций (или чистовых отверстий) вне солида задания на отверстие, иначе False</returns>
        private bool MepElementsIntersectConstructionsOrOpenings(
            Solid thisOpeningTaskSolid,
            ICollection<ElementId> intersectingMepElementsIds,
            ICollection<ElementId> intersectingLinkConstructions,
            ICollection<IOpeningReal> intersectingLinkOpeningsReal,
            IConstructureLinkElementsProvider link,
            OpeningMepTaskOutcoming mepTaskOutcoming
            ) {
            if(!mepTaskOutcoming.IsRemoved
                && (link != null)
                && (intersectingMepElementsIds != null)
                && (intersectingMepElementsIds.Count > 0)
                && (intersectingLinkConstructions != null)
                && (intersectingLinkConstructions.Count > 0)
                && (intersectingLinkOpeningsReal != null)
                && (thisOpeningTaskSolid != null)
                && (thisOpeningTaskSolid.Volume > 0)) {

                var thisSolidInLinkCoordinates = SolidUtils
                    .CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);

                // получение объединенного солида для элементов ВИС в координатах текущего файла с заданиями на отверстия
                var mepElements = intersectingMepElementsIds
                    .Select(mepId => _revitRepository.Doc.GetElement(mepId))
                    .ToHashSet();
                var mepSolids = mepElements
                    .Select(el => el.GetSolid())
                    .Where(solid => (solid != null) && (solid.Volume > 0))
                    .ToList();
                var mepUnitedSolid = RevitClashDetective.Models.Extensions.ElementExtensions.UniteSolids(mepSolids);
                if((mepUnitedSolid is null) || (mepUnitedSolid.Volume < _volumeTolerance)) {
                    return false;
                }

                try {
                    // трансформация объединенного солида элементов ВИС в координаты связанного файла с конструкциями
                    var mepSolidInLinkCoordinates = SolidUtils
                        .CreateTransformed(mepUnitedSolid, link.DocumentTransform.Inverse);
                    // вычитание из объединенного солида элементов ВИС солида задания на отверстие,
                    // чтобы исключить из проверки места пересечения элементов ВИС с конструкциями внутри тела солида задания на отверстие
                    var mepSolidMinusOpeningTask = BooleanOperationsUtils
                        .ExecuteBooleanOperation(
                        mepSolidInLinkCoordinates,
                        thisSolidInLinkCoordinates,
                        BooleanOperationsType.Difference);

                    // поиск конструкций (стен и перекрытий) и чистовых отверстий из связанного файла, которые пересекаются с проходящими через задание на отверстие элементами ВИС из текущего файла,
                    // с учетом того, что место пересечения находится вне тела задания на отверстие.
                    BoundingBoxXYZ mepElementsBBox = mepElements
                        .Select(el => el.GetBoundingBox())
                        .GetCommonBoundingBox()
                        .TransformBoundingBox(link.DocumentTransform.Inverse);
                    bool mepElementsIntersectConstructions =
                        new FilteredElementCollector(link.Document, intersectingLinkConstructions)
                        .WherePasses(new ElementIntersectsSolidFilter(mepSolidMinusOpeningTask))
                        .Any();
                    bool mepElementsIntersectOpeningsReal = intersectingLinkOpeningsReal
                        .Any(openingReal => _solidProviderUtils
                        .IntersectsSolid(openingReal, mepSolidMinusOpeningTask, mepElementsBBox));
                    return mepElementsIntersectConstructions || mepElementsIntersectOpeningsReal;

                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary>
        /// Возвращает список Id элементов инженерных систем, которые пересекаются с данным заданием на отверстие, из текущего документа
        /// </summary>
        private ICollection<ElementId> GetIntersectingMepElementsIds(
            Solid thisOpeningSolid,
            ICollection<ElementId> allMepElementsIds,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if(!mepTaskOutcoming.IsRemoved
                && (thisOpeningSolid != null)
                && (allMepElementsIds != null)
                && allMepElementsIds.Any()) {

                if(!_intersectingMepElementsCache.ContainsKey(mepTaskOutcoming.Id)) {
                    var ids = new FilteredElementCollector(_revitRepository.Doc, allMepElementsIds)
                        .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                        .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                        .ToElementIds();
                    _intersectingMepElementsCache.Add(mepTaskOutcoming.Id, ids);
                }
                return _intersectingMepElementsCache[mepTaskOutcoming.Id];
            } else {
                return Array.Empty<ElementId>();
            }
        }

        /// <summary>
        /// Проверяет, размещено ли текущее задание на отверстие вручную
        /// </summary>
        /// <returns>True, если присутствует и включен параметр <see cref="RevitRepository.OpeningIsManuallyPlaced"/>,
        /// иначе False</returns>
        private bool IsManuallyPlaced(OpeningMepTaskOutcoming outcomingTask) {
            if(!outcomingTask.IsRemoved) {
                try {
                    return outcomingTask.GetFamilyInstance()
                        .GetSharedParamValue<int>(RevitRepository.OpeningIsManuallyPlaced) == 1;

                } catch(ArgumentException) {
                    return false;
                }
            } else {
                return false;
            }
        }

        private ICollection<IConstructureLinkElementsProvider> GetLinkProviders(RevitRepository revitRepository) {
            return revitRepository.GetConstructureLinks()
                .Select(link => new ConstructureLinkElementsProvider(revitRepository, link))
                .ToArray();
        }

        private ICollection<ElementId> GetOpeningsMepTasksOutcoming(RevitRepository revitRepository) {
            var openingInWalls = revitRepository.GetWallOpeningsMepTasksOutcoming();
            var openingsInFloor = revitRepository.GetFloorOpeningsMepTasksOutcoming();
            openingsInFloor.AddRange(openingInWalls);
            return openingsInFloor.Select(famInst => famInst.Id).ToHashSet();
        }

        /// <summary>
        /// Назначает хост задания на отверстие
        /// </summary>
        private void FindAndSetHost(
            Solid thisOpeningTaskSolid,
            ICollection<IConstructureLinkElementsProvider> constructureLinkElementsProviders,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            foreach(var link in constructureLinkElementsProviders) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    thisOpeningTaskSolid,
                    link,
                    out _,
                    mepTaskOutcoming);
                if(hostConstructions.Count > 0) {
                    break;
                }
            }
        }

        /// <summary>
        /// Возвращает коллекцию Id элементов конструкций из <see cref="IConstructureLinkElementsProvider">связанного файла</see>, которые пересекаются с текущим заданием на отверстие
        /// </summary>
        /// <param name="thisOpeningSolidInLinkCoordinates">Солид текущего задания на отверстие в координатах связанного файла <paramref name="constructureLinkElementsProvider"/></param>
        /// <param name="constructureLinkElementsProvider">Связанный файл для проверки на пересечение</param>
        private ICollection<ElementId> GetIntersectingLinkConstructionElementsIds(
            Solid thisOpeningSolidInLinkCoordinates,
            IConstructureLinkElementsProvider constructureLinkElementsProvider) {

            ICollection<ElementId> ids = constructureLinkElementsProvider.GetConstructureElementIds();
            if(!ids.Any()) {
                return Array.Empty<ElementId>();
            }
            return new FilteredElementCollector(constructureLinkElementsProvider.Document, ids)
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolidInLinkCoordinates))
                .ToElementIds();
        }

        /// <summary>
        /// Поиск конструкций из связанного файла, в которых расположено задание на отверстие.
        /// Если конструкции будут найдены, то также будет вызван метод <see cref="SetHostConstruction"/>
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="link">Связь с конструкциями</param>
        /// <param name="intersectingOpeningsReal">Чистовые отверстия из связи, которые пересекаются с солидом текущего задания на отверстие</param>
        /// <returns>
        /// Коллекция Id конструкций (стен или перекрытий) из связанного файла, которые пересекаются с солидом текущего задания на отверстие
        /// <para>или коллекция Id конструкций, которые являются хостами чистовых отверстий, с которыми пересекается солид текущего задания на отверстие</para> 
        /// </returns>
        private ICollection<ElementId> GetHostConstructionsForThisOpeningTask(
            Solid thisOpeningTaskSolid,
            IConstructureLinkElementsProvider link,
            out ICollection<IOpeningReal> intersectingOpeningsReal,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(
                thisOpeningTaskSolid,
                link.DocumentTransform.Inverse);

            intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, thisOpeningTaskSolid, mepTaskOutcoming);

            // поиск конструкций из связи, в которых находится текущее задание на отверстие
            var intersectingConstructions = GetIntersectingLinkConstructionElementsIds(
                thisSolidInLinkCoordinates,
                link);
            if(intersectingConstructions.Count == 0) {
                // задание на отверстие не пересекается с конструкциями из связей

                // поиск чистовых отверстий из связи, которые пересекаются с текущим заданием на отверстие
                if(intersectingOpeningsReal.Count > 0) {
                    // пересечение с чистовыми отверстиями из связей найдено

                    // поиск элементов конструкций - основ чистовых отверстий из связей
                    intersectingConstructions = intersectingOpeningsReal
                        .Select(opening => opening.GetHost().Id)
                        .Distinct()
                        .ToHashSet();
                }
            } else {
                // если задание на отверстие пересекается с конструкциями из связей, то будем считать, что даже если задание также пересекается с чистовыми отверстиями,
                // то хосты этих чистовых отверстий - это и есть конструкции, с которым пересекается само задание на отверстие.
                // Поэтому можно не делать долгую проверку на поиск пересекающих чистовых отверстий (см. ниже).

                // Если эта логика окажется неверной, то надо убрать комментарии ниже

                // поиск конструкций - основ чистовых отверстий, в которых находится текущее задание на отверстие
                //intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, thisOpeningTaskSolid);
                //if(intersectingOpeningsReal.Count > 0) {

                //    var openingsHosts = intersectingOpeningsReal.Select(opening => opening.GetHost().Id).Distinct().ToHashSet();
                //    var constructions = new List<ElementId>(intersectingConstructions);
                //    constructions.AddRange(openingsHosts);
                //    intersectingConstructions = constructions;
                //}
            }
            if(intersectingConstructions.Count > 0) {
                SetHostConstruction(link, intersectingConstructions, thisSolidInLinkCoordinates, mepTaskOutcoming);
            }

            return intersectingConstructions;
        }

        /// <summary>
        /// Назначает свойство хоста текущего задания на отверстие <see cref="OpeningMepTaskOutcoming.Host"/>
        /// </summary>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <param name="intersectingLinkedElements">Элементы из связанного файла с конструкциями, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="thisOpeningTaskSolidInLinkCoordinates">Солид текущего задания на отверстие в координатах связанного файла</param>
        private void SetHostConstruction(
            IConstructureLinkElementsProvider link,
            ICollection<ElementId> intersectingLinkedElements,
            Solid thisOpeningTaskSolidInLinkCoordinates,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if((link != null)
                && intersectingLinkedElements.Any()
                && (thisOpeningTaskSolidInLinkCoordinates != null)
                && (thisOpeningTaskSolidInLinkCoordinates.Volume >= _volumeTolerance)) {

                // поиск элемента конструкции, с которым пересечение текущего задания на отверстие имеет наибольший объем
                double halfOpeningTaskVolume = thisOpeningTaskSolidInLinkCoordinates.Volume / 2;
                var elements = intersectingLinkedElements.Select(id => link.Document.GetElement(id)).ToHashSet();
                Element hostCandidate = elements.FirstOrDefault();
                double intersectingVolumePrevious = 0;
                foreach(Element element in elements) {
                    var structureSolid = element?.GetSolid();
                    if((structureSolid != null) && (structureSolid.Volume > _volumeTolerance)) {
                        try {
                            double intersectingVolumeCurrent
                                = BooleanOperationsUtils.ExecuteBooleanOperation(
                                    thisOpeningTaskSolidInLinkCoordinates,
                                    structureSolid,
                                    BooleanOperationsType.Intersect)?.Volume
                                    ?? 0;
                            if(intersectingVolumeCurrent >= halfOpeningTaskVolume) {
                                mepTaskOutcoming.Host = element;
                                return;
                            }
                            if(intersectingVolumeCurrent > intersectingVolumePrevious) {
                                intersectingVolumePrevious = intersectingVolumeCurrent;
                                hostCandidate = element;
                            }
                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            continue;
                        }
                    }
                }
                mepTaskOutcoming.Host = hostCandidate;
            } else {
                return;
            }
        }

        /// <summary>
        /// Поиск чистовых отверстий из связанного файла, в которых расположено задание на отверстие
        /// </summary>
        /// <param name="link">Связь с конструкциями и чистовыми отверстиями</param>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <returns>Коллекция чистовых отверстий из связи, которые пересекаются с солидом текущего задания на отверстие</returns>
        private ICollection<IOpeningReal> GetIntersectingLinkOpeningsReal(
            IConstructureLinkElementsProvider link,
            Solid thisOpeningTaskSolid,
            OpeningMepTaskOutcoming mepTaskOutcoming
            ) {

            var thisSolidInLinkCoordinates = SolidUtils
                .CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);
            var thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ(mepTaskOutcoming)
                .TransformBoundingBox(link.DocumentTransform.Inverse);
            return link.GetOpeningsReal()
                .Where(realOpening => _solidProviderUtils.IntersectsSolid(
                    realOpening,
                    thisSolidInLinkCoordinates,
                    thisBBoxInLinkCoordinates))
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает BoundingBoxXYZ с учетом расположения в файле Revit, 
        /// если элемент был удален из документа, будет возвращено значение по умолчанию
        /// </summary>
        public BoundingBoxXYZ GetTransformedBBoxXYZ(OpeningMepTaskOutcoming mepTaskOutcoming) {
            if(mepTaskOutcoming.IsRemoved) {
                return default;
            }
            return mepTaskOutcoming.GetFamilyInstance().GetBoundingBox();
        }
    }
}
