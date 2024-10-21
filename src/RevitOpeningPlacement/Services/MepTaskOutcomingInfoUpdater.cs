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

        /// <summary>
        /// Обработчик геометрии солидов
        /// </summary>
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
        /// Допустимое расстояние между экземплярами семейств заданий на отверстия, 
        /// при котором считается, что они размещены в одном и том же месте
        /// </summary>
        private readonly double _distance3dTolerance = Math.Sqrt(3 * XYZExtension.FeetRound * XYZExtension.FeetRound);

        /// <summary>
        /// Допустимый объем, равный кубу <see cref="_distance3dTolerance"/>
        /// </summary>
        private readonly double _volumeTolerance;

        /// <summary>
        /// Кэш для хранения коллекции id элементов ВИС из активного файла, 
        /// которые пересекаются с исходящим заданием на отверстие
        /// </summary>
        private readonly Dictionary<ElementId, ICollection<ElementId>> _intersectingMepElementsCache;

        /// <summary>
        /// Кэш для хранения солидов исходящих заданий на отверстия из активного файла
        /// </summary>
        private readonly Dictionary<ElementId, Solid> _openingsSolidCache;


        public MepTaskOutcomingInfoUpdater(RevitRepository revitRepository, ISolidProviderUtils solidProviderUtils) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _solidProviderUtils = solidProviderUtils ?? throw new ArgumentNullException(nameof(solidProviderUtils));

            _outcomingTasksIds = GetOpeningsMepTasksOutcoming(_revitRepository);
            _mepElementsIds = revitRepository.GetMepElementsIds();
            _constructureLinks = GetLinkProviders(revitRepository);
            _intersectingMepElementsCache = new Dictionary<ElementId, ICollection<ElementId>>();
            _openingsSolidCache = new Dictionary<ElementId, Solid>();

            _volumeTolerance = _distance3dTolerance * _distance3dTolerance * _distance3dTolerance;
        }

        // 1. определить, что геометрия валидная
        // 2. проверить значение параметра "размещено вручную"
        // 3. проверить статус "не актуальное" и закэшировать элементы ВИС, которые проходят через задание
        // 4. проверить статус "пересекающееся отверстие"
        // 5. проверить статус "объединенное" - если количество закэшированных элементов ВИС >= 2
        // 6. проверить габаритные статусы. Принять за вводные, что через задание проходит только 1 элемент ВИС.
        //      Найти отступы сверху-снизу, справа-слева, определить для этого элемента ВИС отступы и округление из конфига
        //      по этим значениям определить статус "большое", "маленькое", "корректно".
        /// <summary>
        /// Обновляет <see cref="OpeningMepTaskOutcoming.Status">статус</see> 
        /// и <see cref="OpeningMepTaskOutcoming.Host">хост</see>
        /// исходящего задания на отверстие от ВИС из активного файла.
        /// </summary>
        /// <param name="outcomingTask">Исходящее задание н отверстие от ВИС из активного файла.</param>
        public void UpdateInfo(OpeningMepTaskOutcoming outcomingTask) {
            try {
                if(TaskIsInvalid(outcomingTask)) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }

                var openingSolid = GetOpeningSolid(outcomingTask);
                if(IsManuallyPlaced(outcomingTask)) {
                    FindAndSetHost(outcomingTask);
                    outcomingTask.Status = OpeningTaskOutcomingStatus.ManuallyPlaced;
                    return;
                }

                if(ThisOpeningTaskIsNotActual(outcomingTask)) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.NotActual;
                    return;
                }

                var intersectingOpeningIds = GetIntersectingOpeningsTasks(
                    outcomingTask);
                if(intersectingOpeningIds.Count > 0) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Intersects;
                    return;
                }

                Solid openingSolidAfterIntersection = GetOpeningAndMepSolidsDifference(outcomingTask);
                if(openingSolidAfterIntersection is null) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }
                double volumeRatio =
                    (openingSolid.Volume - openingSolidAfterIntersection.Volume) / openingSolid.Volume;
                outcomingTask.Status = GetOpeningTaskOutcomingStatus(volumeRatio);
                return;
            } catch(Exception ex) when(
            ex is NullReferenceException
            || ex is ArgumentException
            || ex is InvalidOperationException
            || ex is Autodesk.Revit.Exceptions.ApplicationException) {
                outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                return;
            }
        }

        /// <summary>
        /// Проверяет на валидность исходящее задание на отверстие
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>True, если задание не валидно, иначе False.</returns>
        private bool TaskIsInvalid(OpeningMepTaskOutcoming opening) {
            var solid = GetOpeningSolid(opening);
            return opening.IsRemoved || (solid is null) || (solid.Volume < _volumeTolerance);
        }

        /// <summary>
        /// Возвращает солид исходящего задания на отверстие в координатах активного файла
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Солид исходящего задания на отверстие</returns>
        private Solid GetOpeningSolid(OpeningMepTaskOutcoming opening) {
            if(!_openingsSolidCache.ContainsKey(opening.Id)) {
                var solid = opening.GetSolid();
                _openingsSolidCache.Add(opening.Id, solid);
            }
            return _openingsSolidCache[opening.Id];
        }

        /// <summary>
        /// Возвращает солид исходящего задания на отверстие в координатах связанного конструктивного файла
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <param name="link">Связь с конструкциями, в координатах которой нужно получить солид</param>
        /// <returns>Солид исходящего задания на отверстие в координатах связанного файла</returns>
        private Solid GetOpeningSolid(OpeningMepTaskOutcoming opening, IConstructureLinkElementsProvider link) {
            Solid thisOpeningTaskSolid = GetOpeningSolid(opening);
            return SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);
        }

        /// <summary>
        /// Возвращает статус задания на отверстие по отношению объема пересечения задания на отверстие с элементом инженерной системы к исходному объему задания на отверстие
        /// </summary>
        /// <param name="volumeRatio">Отношение объемов, [0; 1]</param>
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

        // TODO too big method
        /// <summary>
        /// Возвращает солид, образованный вычитанием из исходного солида задания на отверстие всех пересекающих его элементов инженерных систем
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <exception cref="InvalidOperationException">Исключение, если не удалось выполнить вычитание солида отверстия и солида элемента инженерной системы</exception>
        private Solid GetOpeningAndMepSolidsDifference(OpeningMepTaskOutcoming mepTaskOutcoming) {
            var intersectingMepSolids = GetIntersectingMepSolids(mepTaskOutcoming);
            Solid thisOpeningSolid = GetOpeningSolid(mepTaskOutcoming);
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
        /// Возвращает список солидов элементов инженерных систем, которые пересекаются с заданием на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>Коллекция солидов элементов ВИС из активного файла, которые пересекаются с заданием на отверстие
        /// </returns>
        private ICollection<Solid> GetIntersectingMepSolids(OpeningMepTaskOutcoming mepTaskOutcoming) {
            return GetIntersectingMepElementsIds(mepTaskOutcoming)
                .Select(elementId => _revitRepository.Doc.GetElement(elementId).GetSolid())
                .ToArray();
        }

        /// <summary>
        /// Находит задания на отверстия из активного файла, которые пересекаются с данным заданием на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>Коллекция Id заданий на отверстия, которые пересекаются с текущим заданием на отверстие</returns>
        private ICollection<ElementId> GetIntersectingOpeningsTasks(OpeningMepTaskOutcoming mepTaskOutcoming) {
            Solid thisOpeningTaskSolid = GetOpeningSolid(mepTaskOutcoming);
            return new FilteredElementCollector(_revitRepository.Doc, _outcomingTasksIds)
                .Excluding(new ElementId[] { mepTaskOutcoming.Id })
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningTaskSolid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningTaskSolid))
                .ToElementIds();
        }

        // TODO too big method
        /// <summary>
        /// Проверяет, является ли данное задание на отверстие НЕ актуальным. Если задание не актуально, возвращается False, иначе True.
        /// Метод может установить только НЕкорректность задания, но корректность абсолютно точно подтвердить не может.
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие из активного файла</param>
        /// <returns>True - задание точно некорректно; False - задание не некорректно, но утверждать, что оно корректно нельзя</returns>
        private bool ThisOpeningTaskIsNotActual(OpeningMepTaskOutcoming mepTaskOutcoming) {
            if(_constructureLinks is null) {
                throw new ArgumentNullException(nameof(_constructureLinks));
            }
            if(_mepElementsIds is null) {
                throw new ArgumentNullException(nameof(_mepElementsIds));
            }

            var mepElementsIntersectingThisTask = GetIntersectingMepElementsIds(mepTaskOutcoming);
            if(mepElementsIntersectingThisTask.Count == 0) {
                // задание на отверстие не пересекается ни с одним элементом инженерной системы - задание не актуально
                return true;
            }

            // проверка на то, что:
            // во-первых есть конструкции в связанных файлах, внутри которых расположено задание на отверстие,
            // во-вторых, что ни один элемент ВИС, проходящий через текущее задание на отверстие не пересекается с этими конструкциями
            foreach(var link in _constructureLinks) {
                // поиск конструкций из связей, которые можно считать хостами для исходящего задания на отверстие
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    link,
                    out ICollection<IOpeningReal> intersectingOpenings,
                    mepTaskOutcoming);
                if(hostConstructions.Count > 0) {
                    // хост-конструкции найдены, теперь проверяем, что элементы ВИС из активного файла,
                    // проходящие через исходящее задание на отверстие не пересекают эти конструкции
                    return MepElementsIntersectConstructionsOrOpenings(
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

        // TODO too big method
        /// <summary>
        /// Проверяет, пересекаются ли элементы ВИС из текущего файла с конструкциями или чистовыми отверстиями, причем место пересечения находится вне солида задания на отверстие.
        /// <para>При этом проверяемые элементы ВИС, а также проверяемые конструкции и чистовые отверстия из связи должны пересекать солид текущего задания на отверстие</para>
        /// </summary>
        /// <param name="intersectingLinkConstructions">Конструкции из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="intersectingLinkOpeningsReal">Чистовые отверстия из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>True, если найдено пересечение элементов ВИС и конструкций (или чистовых отверстий) вне солида задания на отверстие, иначе False</returns>
        private bool MepElementsIntersectConstructionsOrOpenings(
            ICollection<ElementId> intersectingLinkConstructions,
            ICollection<IOpeningReal> intersectingLinkOpeningsReal,
            IConstructureLinkElementsProvider link,
            OpeningMepTaskOutcoming mepTaskOutcoming
            ) {
            if((link != null)
                && (intersectingLinkConstructions != null)
                && (intersectingLinkConstructions.Count > 0)
                && (intersectingLinkOpeningsReal != null)) {

                ICollection<ElementId> intersectingMepElementsIds = GetIntersectingMepElementsIds(mepTaskOutcoming);
                var thisSolidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);

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
        /// Находит элементы ВИС из активного файла, которые пересекаются с заданием на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Задание на отверстие</param>
        /// <returns>Коллекцию Id элементов ВИС, которые пересекают задание на отверстие</returns>
        private ICollection<ElementId> GetIntersectingMepElementsIds(OpeningMepTaskOutcoming mepTaskOutcoming) {
            if(_mepElementsIds is null) {
                throw new ArgumentNullException(nameof(_mepElementsIds));
            }
            Solid thisOpeningSolid = GetOpeningSolid(mepTaskOutcoming);
            if(!_intersectingMepElementsCache.ContainsKey(mepTaskOutcoming.Id)) {
                var ids = new FilteredElementCollector(_revitRepository.Doc, _mepElementsIds)
                    .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                    .ToElementIds();
                _intersectingMepElementsCache.Add(mepTaskOutcoming.Id, ids);
            }
            return _intersectingMepElementsCache[mepTaskOutcoming.Id];
        }

        /// <summary>
        /// Проверяет, размещено ли задание на отверстие вручную
        /// </summary>
        /// <param name="outcomingTask">Задание на отверстие</param>
        /// <returns>True, если задание размещено вручную, иначе False</returns>
        private bool IsManuallyPlaced(OpeningMepTaskOutcoming outcomingTask) {
            try {
                return outcomingTask.GetFamilyInstance()
                    .GetSharedParamValue<int>(RevitRepository.OpeningIsManuallyPlaced) == 1;
            } catch(ArgumentException) {
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

        // TODO strange method
        /// <summary>
        /// Назначает хост задания на отверстие
        /// </summary>
        private void FindAndSetHost(OpeningMepTaskOutcoming mepTaskOutcoming) {
            foreach(var link in _constructureLinks) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    link,
                    out _,
                    mepTaskOutcoming);
                if(hostConstructions.Count > 0) {
                    break;
                }
            }
        }

        /// <summary>
        /// Находит элементы конструкций из связи, с которыми пересекается задание на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Задание на отверстие из активного файла</param>
        /// <param name="constructureLink">Связанный файл с конструкциями</param>
        /// <returns>Коллекция Id элементов конструкций из связи, с которыми пересекается задание на отверстие</returns>
        private ICollection<ElementId> GetIntersectingLinkConstructions(
            OpeningMepTaskOutcoming mepTaskOutcoming,
            IConstructureLinkElementsProvider constructureLink) {

            ICollection<ElementId> ids = constructureLink.GetConstructureElementIds();
            if(!ids.Any()) {
                return Array.Empty<ElementId>();
            }
            Solid solidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, constructureLink);
            return new FilteredElementCollector(constructureLink.Document, ids)
                .WherePasses(new BoundingBoxIntersectsFilter(solidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(solidInLinkCoordinates))
                .ToElementIds();
        }

        // TODO too big method
        /// <summary>
        /// Поиск конструкций из связанного файла, в которых расположено задание на отверстие.
        /// Если конструкции будут найдены, то также будет вызван метод <see cref="SetHostConstruction"/>
        /// </summary>
        /// <param name="link">Связь с конструкциями</param>
        /// <param name="intersectingOpeningsReal">Чистовые отверстия из связи, которые пересекаются с солидом текущего задания на отверстие</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>
        /// Коллекция Id конструкций (стен или перекрытий) из связанного файла, которые пересекаются с солидом текущего задания на отверстие
        /// <para>или коллекция Id конструкций, которые являются хостами чистовых отверстий, с которыми пересекается солид текущего задания на отверстие</para> 
        /// </returns>
        private ICollection<ElementId> GetHostConstructionsForThisOpeningTask(
            IConstructureLinkElementsProvider link,
            out ICollection<IOpeningReal> intersectingOpeningsReal,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, mepTaskOutcoming);

            // поиск конструкций из связи, в которых находится текущее задание на отверстие
            var intersectingConstructions = GetIntersectingLinkConstructions(
                mepTaskOutcoming,
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
                // Поэтому можно не делать поиск пересекающих чистовых отверстий (см. ниже).

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
                SetHostConstruction(link, intersectingConstructions, mepTaskOutcoming);
            }

            return intersectingConstructions;
        }

        // TODO too big method
        /// <summary>
        /// Назначает свойство хоста текущего задания на отверстие <see cref="OpeningMepTaskOutcoming.Host"/>
        /// </summary>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <param name="intersectingLinkedElements">Элементы из связанного файла с конструкциями, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        private void SetHostConstruction(
            IConstructureLinkElementsProvider link,
            ICollection<ElementId> intersectingLinkedElements,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if((link != null)
                && intersectingLinkedElements.Any()) {

                Solid thisOpeningTaskSolidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
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
        /// Поиск чистовых отверстий из связанного файла, которые пересекают задание на отверстие
        /// </summary>
        /// <param name="link">Связь с чистовыми отверстиями</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>Коллекция чистовых отверстий из связи, которые пересекаются с текущим заданием на отверстие
        /// </returns>
        private ICollection<IOpeningReal> GetIntersectingLinkOpeningsReal(
            IConstructureLinkElementsProvider link,
            OpeningMepTaskOutcoming mepTaskOutcoming
            ) {

            var thisSolidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
            var thisBBoxInLinkCoordinates = mepTaskOutcoming.GetTransformedBBoxXYZ()
                .TransformBoundingBox(link.DocumentTransform.Inverse);
            return link.GetOpeningsReal()
                .Where(realOpening => _solidProviderUtils.IntersectsSolid(
                    realOpening,
                    thisSolidInLinkCoordinates,
                    thisBBoxInLinkCoordinates))
                .ToHashSet();
        }
    }
}
