using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.XtraRichEdit.Layout.Engine;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;
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
    private readonly IConstantsProvider _constantsProvider;

    /// <summary>
    /// Обработчик отступов для элементов ВИС, проходящих через задания на отверстия
    /// </summary>
    private readonly IOutcomingTaskOffsetFinder _offsetFinder;

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
    /// которые пересекаются с обрабатываемым исходящим заданием на отверстие
    /// </summary>
    private ICollection<ElementId> _intersectingMepElementsCache;

    /// <summary>
    /// Кэш для хранения найденных конструкций-кандидатов на хосты обрабатываемого задания на отверстия
    /// </summary>
    private (ICollection<ElementId> HostCandidates, IConstructureLinkElementsProvider Link) _hostConstructionsCache;

    /// <summary>
    /// Кэш для хранения солида обрабатываемого исходящего задания на отверстие из активного файла
    /// </summary>
    private Solid _openingSolidCache;


    public MepTaskOutcomingInfoUpdater(
        RevitRepository revitRepository,
        ISolidProviderUtils solidProviderUtils,
        IConstantsProvider constantsProvider,
        IOutcomingTaskOffsetFinder offsetFinder) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _solidProviderUtils = solidProviderUtils ?? throw new ArgumentNullException(nameof(solidProviderUtils));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _offsetFinder = offsetFinder ?? throw new ArgumentNullException(nameof(offsetFinder));
        _outcomingTasksIds = GetOpeningsMepTasksOutcoming(_revitRepository);
        _mepElementsIds = revitRepository.GetMepElementsIds();
        _constructureLinks = GetLinkProviders(revitRepository);
        ClearCache();

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
            if(OpeningTaskInUnacceptableConstructions(outcomingTask)) {
                FindAndSetHost(outcomingTask);
                outcomingTask.Status = OpeningTaskOutcomingStatus.UnacceptableConstructions;
                return;
            }
            if(OpeningTaskInDifferentConstructions(outcomingTask)) {
                FindAndSetHost(outcomingTask);
                outcomingTask.Status = OpeningTaskOutcomingStatus.DifferentConstructions;
                return;
            }
            if(OpeningTaskIsIntersecting(outcomingTask)) {
                FindAndSetHost(outcomingTask);
                outcomingTask.Status = OpeningTaskOutcomingStatus.Intersects;
                return;
            }
            if(OpeningTaskIsUnited(outcomingTask)) {
                FindAndSetHost(outcomingTask);
                outcomingTask.Status = OpeningTaskOutcomingStatus.United;
                return;
            }
            SetSizeStatus(outcomingTask);

        } catch(Exception ex) when(
        ex is NullReferenceException
        or ArgumentException
        or InvalidOperationException
        or Autodesk.Revit.Exceptions.ApplicationException) {
            outcomingTask.Status = OpeningTaskOutcomingStatus.Invalid;
        } finally {
            ClearCache();
        }
    }

    /// <summary>
    /// Проверяет, пересекается ли исходящее задание на отверстие с недопустимыми конструкциями из связей
    /// </summary>
    /// <param name="opening">Исходящее задание на отверстие</param>
    /// <returns>True, если задание на отверстие пересекается с</returns>
    private bool OpeningTaskInUnacceptableConstructions(OpeningMepTaskOutcoming opening) {
        foreach(var link in _constructureLinks) {
            bool intersects = GetIntersectingLinkConstructions(
                opening,
                link,
                RevitRepository.UnacceptableStructureCategories.ToArray())
                .Any();
            if(intersects) {
                return true;
            }
        }
        return false;
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
    private void SetSizeStatus(OpeningMepTaskOutcoming opening) {
        var mepElement = GetIntersectingMepElements(opening).First();

        double minOffset = _offsetFinder.GetMinHorizontalOffsetSum(mepElement);
        double maxOffset = _offsetFinder.GetMaxHorizontalOffsetSum(mepElement);
        double horiz = GetRoundDistance(_offsetFinder.FindHorizontalOffsetsSum(opening, mepElement));
        double vert = GetRoundDistance(_offsetFinder.FindVerticalOffsetsSum(opening, mepElement));

        if((horiz < minOffset) || (vert < minOffset)) {
            opening.Status = OpeningTaskOutcomingStatus.TooSmall;
        } else if((horiz > maxOffset) || (vert > maxOffset)) {
            opening.Status = OpeningTaskOutcomingStatus.TooBig;
        } else if((minOffset <= horiz) && (horiz <= maxOffset)
            && (minOffset <= vert) && (vert <= maxOffset)) {
            opening.Status = OpeningTaskOutcomingStatus.Correct;
        }
        FindAndSetHost(opening);
    }

    /// <summary>
    /// Округляет заданное расстояние кратно допуску на расстояние, 
    /// определенному в <see cref="IConstantsProvider.ToleranceDistanceFeet"/>
    /// </summary>
    /// <param name="distance">Расстояние в единицах Revit</param>
    /// <returns>Расстояние в единицах Revit, кратное допуску.</returns>
    private double GetRoundDistance(double distance) {
        return Math.Round(distance / _constantsProvider.ToleranceDistanceFeet, MidpointRounding.AwayFromZero)
            * _constantsProvider.ToleranceDistanceFeet;
    }

    /// <summary>
    /// Возвращает солид исходящего задания на отверстие в координатах активного файла
    /// </summary>
    /// <param name="opening">Исходящее задание на отверстие</param>
    /// <returns>Солид исходящего задания на отверстие</returns>
    private Solid GetOpeningSolid(OpeningMepTaskOutcoming opening) {
        _openingSolidCache ??= opening.GetSolid();
        return _openingSolidCache;
    }

    /// <summary>
    /// Возвращает солид исходящего задания на отверстие в координатах связанного конструктивного файла
    /// </summary>
    /// <param name="opening">Исходящее задание на отверстие</param>
    /// <param name="link">Связь с конструкциями, в координатах которой нужно получить солид</param>
    /// <returns>Солид исходящего задания на отверстие в координатах связанного файла</returns>
    private Solid GetOpeningSolid(OpeningMepTaskOutcoming opening, IConstructureLinkElementsProvider link) {
        var thisOpeningTaskSolid = GetOpeningSolid(opening);
        return SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);
    }

    private void ClearCache() {
        _intersectingMepElementsCache = null;
        _openingSolidCache = null;
        _hostConstructionsCache = (null, null);
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

        var thisOpeningTaskSolid = GetOpeningSolid(mepTaskOutcoming);
        return new FilteredElementCollector(_revitRepository.Doc, _outcomingTasksIds)
            .Excluding(new ElementId[] { mepTaskOutcoming.Id })
            .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningTaskSolid.GetOutline()))
            .WherePasses(new ElementIntersectsSolidFilter(thisOpeningTaskSolid))
            .ToElementIds();
    }

    /// <summary>
    /// Проверяет, находится ли данное задание на отверстие в разных конструкциях
    /// </summary>
    /// <param name="mepTaskOutcoming">Исходящее задание на отверстие из активного файла</param>
    /// <returns><True - задание на отверстие в разных констркуциях, False - другие случаи/returns>
    private bool OpeningTaskInDifferentConstructions(OpeningMepTaskOutcoming mepTaskOutcoming) {
        if(_hostConstructionsCache.Link != null && _hostConstructionsCache.HostCandidates != null) {
            return _hostConstructionsCache.HostCandidates
                .Select(hostId => _hostConstructionsCache.Link.Document
                    .GetElement(hostId)
                    .Category
                    .GetBuiltInCategory())
                .Distinct()
                .Count() > 1;
        } else {
            foreach(var link in _constructureLinks) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(
                    mepTaskOutcoming,
                    link,
                    out _);
                if(hostConstructions.Count > 0) {
                    return hostConstructions
                        .Select(hostId => link.Document.GetElement(hostId).Category.GetBuiltInCategory())
                        .Distinct()
                        .Count() > 1;
                }
            }
            return false;
        }
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
                mepTaskOutcoming,
                link,
                out var intersectingOpenings);
            if(hostConstructions.Count > 0) {
                // хост-конструкции найдены.
                // проверяем, что элементы ВИС из активного файла,
                // проходящие через исходящее задание на отверстие, не пересекают эти конструкции
                // и заканчиваем обработку
                _hostConstructionsCache = (hostConstructions, link);
                return MepElementsIntersectConstructionsOrOpenings(
                        mepTaskOutcoming,
                        hostConstructions,
                        intersectingOpenings,
                        link);
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
        OpeningMepTaskOutcoming mepTaskOutcoming,
        ICollection<ElementId> intersectingLinkConstructions,
        ICollection<IOpeningReal> intersectingLinkOpeningsReal,
        IConstructureLinkElementsProvider link) {
        if(link != null) {
            try {
                var mepSolidMinusOpeningTask = GetMepSolidMinusOpeningTask(mepTaskOutcoming, link);

                // поиск конструкций (стен и перекрытий) и чистовых отверстий из связанного файла,
                // которые пересекаются с элементами ВИС из активного файла вне задания на отверстие.
                // эти элементы ВИС проходят через задание.
                return MepElementsIntersectConstructions(
                        mepSolidMinusOpeningTask,
                        intersectingLinkConstructions,
                        link)
                    || MepElementsIntersectRealOpenings(
                        mepSolidMinusOpeningTask,
                        GetIntersectingMepElements(mepTaskOutcoming),
                        intersectingLinkOpeningsReal,
                        link);
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

        var mepUnitedSolid = GetIntersectingMepUnitedSolid(mepTaskOutcoming);
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
            .Select(_revitRepository.Doc.GetElement)
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
        Solid mepSolidMinusOpeningTask,
        ICollection<ElementId> intersectingLinkConstructions,
        IConstructureLinkElementsProvider link) {

        return intersectingLinkConstructions.Count != 0 && new FilteredElementCollector(link.Document, intersectingLinkConstructions)
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
        Solid mepSolidMinusOpeningTask,
        ICollection<Element> mepElements,
        ICollection<IOpeningReal> intersectingLinkOpeningsReal,
        IConstructureLinkElementsProvider link) {

        if(mepElements.Count == 0 || intersectingLinkOpeningsReal.Count == 0) {
            return false;
        }
        var candidates = new FilteredElementCollector(
            link.Document,
            intersectingLinkOpeningsReal
                .Select(opening => opening.Id)
                .ToArray())
            .WherePasses(new BoundingBoxIntersectsFilter(mepSolidMinusOpeningTask.GetOutline()))
            .ToElementIds();
        return intersectingLinkOpeningsReal
            .Where(opening => candidates.Contains(opening.Id))
            .Any(openingReal => _solidProviderUtils
                .IntersectsSolid(openingReal, mepSolidMinusOpeningTask, GetUnitedBox(mepElements, link)));
    }

    /// <summary>
    /// Создает объединенный бокс в координатах связанного файла по элементам из активного файла
    /// </summary>
    /// <param name="link">Связанный файл</param>
    /// <param name="elements">Элементы из активного файла</param>
    /// <returns>Бокс, ограничивающий все заданные элементы из активного файла в координатах связанного файла
    /// </returns>
    private BoundingBoxXYZ GetUnitedBox(ICollection<Element> elements, IConstructureLinkElementsProvider link) {
        return elements
            .Select(el => el.GetBoundingBox())
            .GetCommonBoundingBox()
            .TransformBoundingBox(link.DocumentTransform.Inverse);
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
        var thisOpeningSolid = GetOpeningSolid(mepTaskOutcoming);
        _intersectingMepElementsCache ??= new FilteredElementCollector(_revitRepository.Doc, _mepElementsIds)
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                .ToElementIds();
        return _intersectingMepElementsCache;
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
        return revitRepository.GetSelectedRevitLinks()
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
        if(_hostConstructionsCache.Link != null && _hostConstructionsCache.HostCandidates != null) {
            mepTaskOutcoming.Host = FindHostConstruction(
                mepTaskOutcoming,
                _hostConstructionsCache.HostCandidates,
                _hostConstructionsCache.Link);
            return;
        }
        foreach(var link in _constructureLinks) {
            var hostConstructions = GetHostConstructionsForThisOpeningTask(
                mepTaskOutcoming,
                link,
                out _);
            if(hostConstructions.Count > 0) {
                mepTaskOutcoming.Host = FindHostConstruction(mepTaskOutcoming, hostConstructions, link);
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

        var ids = constructureLink.GetConstructureElementIds();
        if(!ids.Any()) {
            return Array.Empty<ElementId>();
        }
        var solidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, constructureLink);
        return new FilteredElementCollector(constructureLink.Document, ids)
            .WherePasses(new BoundingBoxIntersectsFilter(solidInLinkCoordinates.GetOutline()))
            .WherePasses(new ElementIntersectsSolidFilter(solidInLinkCoordinates))
            .ToElementIds();
    }

    /// <summary>
    /// Находит элементы конструкций заданных категорий из связи, с которыми пересекается задание на отверстие
    /// </summary>
    /// <param name="mepTaskOutcoming">Задание на отверстие из активного файла</param>
    /// <param name="constructureLink">Связанный файл с конструкциями</param>
    /// <param name="constructureCategories">Категории конструкций для поиска</param>
    /// <returns>Коллекция Id элементов конструкций заданных категорий из связи, 
    /// с которыми пересекается задание на отверстие</returns>
    private ICollection<ElementId> GetIntersectingLinkConstructions(
        OpeningMepTaskOutcoming mepTaskOutcoming,
        IConstructureLinkElementsProvider constructureLink,
        ICollection<BuiltInCategory> constructureCategories) {

        var solidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, constructureLink);
        return new FilteredElementCollector(constructureLink.Document)
            .WherePasses(new ElementMulticategoryFilter(constructureCategories))
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
        OpeningMepTaskOutcoming mepTaskOutcoming,
        IConstructureLinkElementsProvider link,
        out ICollection<IOpeningReal> intersectingOpeningsReal) {

        // поиск конструкций из связи, в которых находится текущее задание на отверстие
        var intersectingConstructions = GetIntersectingLinkConstructions(mepTaskOutcoming, link);

        // поиск чистовых отверстий из связи, которые пересекаются с заданием на отверстие
        intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(mepTaskOutcoming, link);
        if(intersectingOpeningsReal.Count > 0) {
            // пересечение с чистовыми отверстиями из связей найдено,
            // ищем уникальные элементы конструкций - основы чистовых отверстий из связей
            var openingsRealHostsIds = intersectingOpeningsReal
                .Select(opening => opening.GetHost().Id)
                .ToArray();
            foreach(var hostId in openingsRealHostsIds) {
                intersectingConstructions.Add(hostId);
            }
        }
        return intersectingConstructions.Distinct().ToArray();
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
        OpeningMepTaskOutcoming mepTaskOutcoming,
        ICollection<ElementId> hostCandidates,
        IConstructureLinkElementsProvider link) {

        if((link != null) && hostCandidates.Any()) {
            // ищем элемент, с которым пересечение задания на отверстие имеет наибольший объем
            var solidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
            double halfOpeningTaskVolume = solidInLinkCoordinates.Volume / 2;
            var elements = hostCandidates.Select(link.Document.GetElement).ToArray();
            var hostCandidate = elements.FirstOrDefault();
            double intersectingVolumePrevious = 0;
            foreach(var element in elements) {
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
        OpeningMepTaskOutcoming mepTaskOutcoming,
        IConstructureLinkElementsProvider link) {

        var thisSolidInLinkCoordinates = GetOpeningSolid(mepTaskOutcoming, link);
        var thisBBoxInLinkCoordinates = mepTaskOutcoming.GetTransformedBBoxXYZ()
            .TransformBoundingBox(link.DocumentTransform.Inverse);
        var openings = link.GetOpeningsReal();
        if(openings.Count == 0) {
            return Array.Empty<IOpeningReal>();
        }
        var candidates = new FilteredElementCollector(link.Document, openings.Select(o => o.Id).ToArray())
            .WherePasses(new BoundingBoxIntersectsFilter(thisSolidInLinkCoordinates.GetOutline()))
            .ToElementIds();
        return openings
            .Where(opening => candidates.Contains(opening.Id))
            .Where(realOpening => _solidProviderUtils.IntersectsSolid(
                realOpening,
                thisSolidInLinkCoordinates,
                thisBBoxInLinkCoordinates))
            .ToArray();
    }
}
