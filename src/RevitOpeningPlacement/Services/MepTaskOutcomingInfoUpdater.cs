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


        /// <summary>
        /// Обновляет <see cref="OpeningMepTaskOutcoming.Status">статус</see> 
        /// и <see cref="OpeningMepTaskOutcoming.Host">хост</see>
        /// исходящего задания на отверстие от ВИС из активного файла.
        /// </summary>
        /// <param name="outcomingTask">Исходящее задание н отверстие от ВИС из активного файла.</param>
        public void UpdateInfo(OpeningMepTaskOutcoming outcomingTask) {
            try {
                if(OpeningTaskIsInvalid(outcomingTask)) {
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }
                if(OpeningTaskIsManuallyPlaced(outcomingTask)) {
                    FindAndSetHost(outcomingTask);
                    outcomingTask.Status = OpeningTaskOutcomingStatus.ManuallyPlaced;
                    return;
                }
                if(OpeningTaskIsNotActual(outcomingTask)) {
                    FindAndSetHost(outcomingTask);
                    outcomingTask.Status = OpeningTaskOutcomingStatus.NotActual;
                    return;
                }
                if(OpeningTaskIsIntersecting(outcomingTask)) {
                    FindAndSetHost(outcomingTask);
                    outcomingTask.Status = OpeningTaskOutcomingStatus.Intersects;
                    _outcomingTasksIds.Remove(outcomingTask.Id);
                    return;
                }
                //if(OpeningTaskIsUnited(outcomingTask)) {
                //    FindAndSetHost(outcomingTask);
                //    outcomingTask.Status = OpeningTaskOutcomingStatus.United;
                //    return;
                //}
                SetGeometryStatus(outcomingTask);

            } catch(Exception ex) when(
            ex is NullReferenceException
            || ex is ArgumentException
            || ex is InvalidOperationException
            || ex is Autodesk.Revit.Exceptions.ApplicationException) {
                outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
            }
        }

        /// <summary>
        /// Проверяет на валидность исходящее задание на отверстие
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>True, если задание не валидно, иначе False.</returns>
        private bool OpeningTaskIsInvalid(OpeningMepTaskOutcoming opening) {
            var solid = GetOpeningSolid(opening);
            return opening.IsRemoved || (solid is null) || (solid.Volume < _volumeTolerance);
        }

        /// <summary>
        /// Проверяет, является ли исходящее задание объединенным
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>True, если задание считается объединенным, иначе False</returns>
        private bool OpeningTaskIsUnited(OpeningMepTaskOutcoming opening) {
            // если через задание проходит более 1 элемента ВИС из активного файла, то будем считать его объединенным
            return GetIntersectingMepElementsIds(opening).Count > 1;
        }

        /// <summary>
        /// Проверяет, пересекается ли задание на отверстие с каким-либо другим заданием из активного файла
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие из активного файла</param>
        /// <returns>True, если задание на отверстие пересекается каким-либо другим заданием, иначе False</returns>
        private bool OpeningTaskIsIntersecting(OpeningMepTaskOutcoming opening) {
            return GetIntersectingOpeningsTasks(opening).Count > 0;
        }

        /// <summary>
        /// Назначает заданию статус его геометрии: слишком большое, слишком маленькое и т.п.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        private void SetGeometryStatus(OpeningMepTaskOutcoming opening) {
            Solid openingSolidAfterIntersection = GetOpeningAndMepSolidsDifference(opening);
            if(openingSolidAfterIntersection is null) {
                opening.Status = OpeningTaskOutcomingStatus.Invalid;
                return;
            }
            FindAndSetHost(opening);
            var openingSolid = GetOpeningSolid(opening);
            double volumeRatio =
                (openingSolid.Volume - openingSolidAfterIntersection.Volume) / openingSolid.Volume;
            opening.Status = GetOpeningTaskOutcomingStatus(volumeRatio);
            return;
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
            if(_outcomingTasksIds.Count == 0) {
                return Array.Empty<ElementId>();
            }

            Solid thisOpeningTaskSolid = GetOpeningSolid(mepTaskOutcoming);
            return new FilteredElementCollector(_revitRepository.Doc, _outcomingTasksIds)
                .Excluding(new ElementId[] { mepTaskOutcoming.Id })
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningTaskSolid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningTaskSolid))
                .ToElementIds();
        }

        /// <summary>
        /// Проверяет, является ли данное задание на отверстие НЕ актуальным.
        /// Метод может установить только НЕкорректность задания, но корректность абсолютно точно подтвердить не может.
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие из активного файла</param>
        /// <returns>True - задание точно некорректно; False - задание, возможно, корректно</returns>
        private bool OpeningTaskIsNotActual(OpeningMepTaskOutcoming mepTaskOutcoming) {
            var mepElementsIntersectingThisTask = GetIntersectingMepElementsIds(mepTaskOutcoming);
            if(mepElementsIntersectingThisTask.Count == 0) {
                // задание на отверстие не пересекается ни с одним элементом инженерной системы - задание не актуально
                return true;
            }

            // проверка на то, что:
            // во-первых есть конструкции в связанных файлах, внутри которых расположено задание на отверстие (хостов),
            // во-вторых, что ни один элемент ВИС, проходящий через задание на отверстие,
            // не пересекается с этими конструкциями
            foreach(var link in _constructureLinks) {
                // поиск конструкций из связей, которые можно считать хостами для исходящего задания на отверстие
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    link,
                    out ICollection<IOpeningReal> intersectingOpenings,
                    mepTaskOutcoming);
                if(hostConstructions.Count > 0) {
                    // хост-конструкции найдены.
                    // проверяем, что элементы ВИС из активного файла,
                    // проходящие через исходящее задание на отверстие, не пересекают эти конструкции
                    // и заканчиваем обработку
                    return MepElementsIntersectConstructionsOrOpenings(
                            hostConstructions,
                            intersectingOpenings,
                            link,
                            mepTaskOutcoming);
                } else {
                    // если не найдены конструкции, которые можно считать хостами текущего задания на отверстие,
                    // то либо задание на отверстие висит в воздухе,
                    // либо задание на отверстие пересекается с другой связью. Переходим к следующей связи.
                    continue;
                }
            }

            // корректная ситуация не найдена, отверстие считается не актуальным
            return true;
        }

        /// <summary>
        /// Проверяет, выходят ли элементы ВИС за габариты задания на отверстие.
        /// </summary>
        /// <param name="intersectingLinkConstructions">
        /// Конструкции из <paramref name="link"/>, 
        /// которые пересекаются с текущим заданием на отверстие (кандидаты в хосты этого задания).</param>
        /// <param name="intersectingLinkOpeningsReal">
        /// Чистовые отверстия из <paramref name="link"/>, 
        /// которые пересекаются с текущим заданием на отверстие.</param>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>True, если элементы ВИС выходят за габариты задания на отверстие, иначе False</returns>
        private bool MepElementsIntersectConstructionsOrOpenings(
            ICollection<ElementId> intersectingLinkConstructions,
            ICollection<IOpeningReal> intersectingLinkOpeningsReal,
            IConstructureLinkElementsProvider link,
            OpeningMepTaskOutcoming mepTaskOutcoming) {
            if(link != null) {
                try {
                    Solid mepSolidMinusOpeningTask = GetMepSolidMinusOpeningTask(mepTaskOutcoming, link);

                    // поиск конструкций (стен и перекрытий) и чистовых отверстий из связанного файла,
                    // которые пересекаются с элементами ВИС из активного файла вне задания на отверстие.
                    // эти элементы ВИС проходят через задание.
                    return MepElementsIntersectConstructions(
                            link,
                            intersectingLinkConstructions,
                            mepSolidMinusOpeningTask)
                        || MepElementsIntersectRealOpenings(
                            link,
                            intersectingLinkOpeningsReal,
                            mepSolidMinusOpeningTask,
                            GetIntersectingMepElements(mepTaskOutcoming));
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary>
        /// Находит солид, полученный вычитанием из объединенного солида элементов ВИС солида задания на отверстие 
        /// в координатах связанного файла.
        /// То есть геометрия элементов ВИС, лежащая вне тела задания на отверстие.
        /// </summary>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <param name="link">Связь, в координатах которой надо получить солид</param>
        /// <returns>Солид в координатах связанного файла</returns>
        private Solid GetMepSolidMinusOpeningTask(
            OpeningMepTaskOutcoming mepTaskOutcoming,
            IConstructureLinkElementsProvider link) {

            Solid mepUnitedSolid = GetIntersectingMepUnitedSolid(mepTaskOutcoming);
            // трансформация объединенного солида элементов ВИС в координаты связанного файла с конструкциями
            var mepSolidInLinkCoordinates = SolidUtils
                .CreateTransformed(mepUnitedSolid, link.DocumentTransform.Inverse);
            var openingTaskSolidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
            // вычитание из объединенного солида элементов ВИС солида задания на отверстие
            return BooleanOperationsUtils.ExecuteBooleanOperation(
                mepSolidInLinkCoordinates,
                openingTaskSolidInLinkCoordinates,
                BooleanOperationsType.Difference);
        }

        /// <summary>
        /// Находит элементы ВИС из активного файла, которые пересекаются с заданием на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Задание на отверстие из активного файла</param>
        /// <returns>Коллекция элементов ВИС, которые пересекаются с заданием на отверстие</returns>
        private ICollection<Element> GetIntersectingMepElements(OpeningMepTaskOutcoming mepTaskOutcoming) {
            return GetIntersectingMepElementsIds(mepTaskOutcoming)
                .Select(mepId => _revitRepository.Doc.GetElement(mepId))
                .ToArray();
        }

        /// <summary>
        /// Находит объединенный солид элементов ВИС, которые пересекают задание на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Задание на отверстие</param>
        /// <returns>Объединенный солид элементов ВИС, которые пересекаются с заданием</returns>
        private Solid GetIntersectingMepUnitedSolid(OpeningMepTaskOutcoming mepTaskOutcoming) {
            var mepSolids = GetIntersectingMepElements(mepTaskOutcoming)
                .Select(el => el.GetSolid())
                .Where(solid => (solid != null) && (solid.Volume > 0))
                .ToList();
            return RevitClashDetective.Models.Extensions.ElementExtensions.UniteSolids(mepSolids);
        }

        /// <summary>
        /// Проверяет на пересечение элементы ВИС из активного файла с конструкциями из связи вне задания на отверстие
        /// </summary>
        /// <param name="link">Связь с конструкциями</param>
        /// <param name="intersectingLinkConstructions">Конструкции из связи, для проверки на пересечение</param>
        /// <param name="mepSolidMinusOpeningTask">
        /// Солид, полученный вычитанием из объединенного солида элементов ВИС солида задания на отверстие 
        /// в координатах связанного файла.
        /// То есть геометрия элементов ВИС, лежащая вне тела задания на отверстие.
        /// </param>
        /// <returns>True, если элементы ВИС пересекаются с конструкциями из связи вне задания, иначе False</returns>
        private bool MepElementsIntersectConstructions(
            IConstructureLinkElementsProvider link,
            ICollection<ElementId> intersectingLinkConstructions,
            Solid mepSolidMinusOpeningTask) {

            if(intersectingLinkConstructions.Count == 0) {
                return false;
            }
            return new FilteredElementCollector(link.Document, intersectingLinkConstructions)
                .WherePasses(new BoundingBoxIntersectsFilter(mepSolidMinusOpeningTask.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(mepSolidMinusOpeningTask))
                .Any();
        }

        /// <summary>
        /// Проверяет на пересечение элементы ВИС из активного файла с чистовыми отверстиями из связи вне задания 
        /// на отверстие
        /// </summary>
        /// <param name="link">Связь с чистовыми отверстиями</param>
        /// <param name="intersectingLinkOpeningsReal">Чистовые отверстия из связи для проверки на пересечение</param>
        /// <param name="mepSolidMinusOpeningTask">
        /// Солид, полученный вычитанием из объединенного солида элементов ВИС солида задания на отверстие 
        /// в координатах связанного файла.
        /// То есть геометрия элементов ВИС, лежащая вне тела задания на отверстие.
        /// </param>
        /// <param name="mepElements">Элементы ВИС для проверки на пересечение</param>
        /// <returns>True, если элементы ВИС пересекаются с чистовыми отверстиями из связи вне задания, иначе False
        /// </returns>
        private bool MepElementsIntersectRealOpenings(
            IConstructureLinkElementsProvider link,
            ICollection<IOpeningReal> intersectingLinkOpeningsReal,
            Solid mepSolidMinusOpeningTask,
            ICollection<Element> mepElements) {

            if(mepElements.Count == 0 || intersectingLinkOpeningsReal.Count == 0) {
                return false;
            }
            BoundingBoxXYZ mepElementsBBox = mepElements
                .Select(el => el.GetBoundingBox())
                .GetCommonBoundingBox()
                .TransformBoundingBox(link.DocumentTransform.Inverse);
            return intersectingLinkOpeningsReal
                .Any(openingReal => _solidProviderUtils
                .IntersectsSolid(openingReal, mepSolidMinusOpeningTask, mepElementsBBox));
        }

        /// <summary>
        /// Находит элементы ВИС из активного файла, которые пересекаются с заданием на отверстие
        /// </summary>
        /// <param name="mepTaskOutcoming">Задание на отверстие</param>
        /// <returns>Коллекцию Id элементов ВИС, которые пересекают задание на отверстие</returns>
        private ICollection<ElementId> GetIntersectingMepElementsIds(OpeningMepTaskOutcoming mepTaskOutcoming) {
            if(_mepElementsIds.Count == 0) {
                return Array.Empty<ElementId>();
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
        private bool OpeningTaskIsManuallyPlaced(OpeningMepTaskOutcoming outcomingTask) {
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
                    mepTaskOutcoming.Host = FindHostConstruction(link, hostConstructions, mepTaskOutcoming);
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

        /// <summary>
        /// Поиск конструкций из связанного файла, в которых расположено задание на отверстие.
        /// Конструкций может быть несколько, 
        /// т.к. 1 задание может пересекать сразу несколько стен/перекрытий в многослойных конструкциях.
        /// </summary>
        /// <param name="link">Связь с конструкциями</param>
        /// <param name="intersectingOpeningsReal">
        /// Чистовые отверстия из связи, которые пересекаются заданием на отверстие</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>
        /// Коллекция Id конструкций (стен или перекрытий) из связанного файла,
        /// которые можно считать хостами (основой) для исходящего задания на отверстие.
        /// </returns>
        private ICollection<ElementId> GetHostConstructionsForThisOpeningTask(
            IConstructureLinkElementsProvider link,
            out ICollection<IOpeningReal> intersectingOpeningsReal,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            intersectingOpeningsReal = Array.Empty<IOpeningReal>();

            // поиск конструкций из связи, в которых находится текущее задание на отверстие
            var intersectingConstructions = GetIntersectingLinkConstructions(mepTaskOutcoming, link);
            if(intersectingConstructions.Count == 0) {
                // задание на отверстие не пересекается с конструкциями из связей,
                // ищем чистовые отверстия из связи, которые пересекаются с заданием на отверстие
                intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, mepTaskOutcoming);
                if(intersectingOpeningsReal.Count > 0) {
                    // пересечение с чистовыми отверстиями из связей найдено,
                    // ищем уникальные элементы конструкций - основы чистовых отверстий из связей
                    intersectingConstructions = intersectingOpeningsReal
                        .Select(opening => opening.GetHost().Id)
                        .ToHashSet();
                }
            } else {
                // задание на отверстие пересекается с конструкциями из связей.
                // будем считать,
                // что даже если задание также пересекается с чистовыми отверстиями,
                // то хосты этих чистовых отверстий - это и есть конструкции - основы задания на отверстие.
                // Поэтому поиск пересекающих задание чистовых отверстий из связи можно не делать.

                // Если эта логика окажется неверной,
                // то надо будет искать пересекающие задания чистовые отверстия и их основы.
            }

            return intersectingConstructions;
        }

        /// <summary>
        /// В метод подается несколько кандидатов на хост задания и среди них определяется наиболее подходящий.
        /// </summary>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <param name="hostCandidates">Элементы - кандидаты на хост задания на отверстия</param>
        /// <param name="mepTaskOutcoming">Исходящее задание на отверстие</param>
        /// <returns>Возвращает наиболее подходящий элемент для хоста задания на отверстие. Метод может вернуть null.
        /// </returns>
        private Element FindHostConstruction(
            IConstructureLinkElementsProvider link,
            ICollection<ElementId> hostCandidates,
            OpeningMepTaskOutcoming mepTaskOutcoming) {

            if((link != null) && hostCandidates.Any()) {
                // ищем элемент, с которым пересечение задания на отверстие имеет наибольший объем
                Solid solidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
                double halfOpeningTaskVolume = solidInLinkCoordinates.Volume / 2;
                var elements = hostCandidates.Select(id => link.Document.GetElement(id)).ToArray();
                Element hostCandidate = elements.FirstOrDefault();
                double intersectingVolumePrevious = 0;
                foreach(Element element in elements) {
                    var structureSolid = element?.GetSolid();
                    if((structureSolid != null) && (structureSolid.Volume > _volumeTolerance)) {
                        try {
                            double intersectingVolumeCurrent
                                = BooleanOperationsUtils.ExecuteBooleanOperation(
                                    solidInLinkCoordinates,
                                    structureSolid,
                                    BooleanOperationsType.Intersect)
                                ?.Volume
                                ?? 0;
                            if(intersectingVolumeCurrent >= halfOpeningTaskVolume) {
                                return element;
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
                return hostCandidate;
            } else {
                return default;
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
            OpeningMepTaskOutcoming mepTaskOutcoming) {

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
