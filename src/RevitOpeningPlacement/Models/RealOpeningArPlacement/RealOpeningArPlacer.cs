using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement;
/// <summary>
/// Класс для размещения чистовых отверстий АР в активном документе в местах расположений заданий на отверстия из связанных файлов ВИС
/// </summary>
internal class RealOpeningArPlacer {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly OpeningRealsArConfig _config;

    public const string RealOpeningArDiameter = "ФОП_РАЗМ_Диаметр";
    public const string RealOpeningArWidth = "ФОП_РАЗМ_Ширина проёма";
    public const string RealOpeningArHeight = "ФОП_РАЗМ_Высота проёма";
    public const string RealOpeningArThickness = "ФОП_РАЗМ_Глубина проёма";
    public const string RealOpeningIsEom = "ЭОМ";
    public const string RealOpeningIsSs = "СС";
    public const string RealOpeningIsOv = "ОВ";
    public const string RealOpeningIsDu = "ДУ";
    public const string RealOpeningIsVk = "ВК";
    public const string RealOpeningIsTs = "ТС";
    public const string RealOpeningTaskId = "ФОП_ID задания";
    public const string RealOpeningIsManualBimModelPart = "ДИСЦИПЛИНА ВРУЧНУЮ";
    public const string RealOpeningManualBimModelPart = "ДИСЦИПЛИНА ВРУЧНУЮ ТЕКСТ";


    /// <summary>
    /// Конструктор класса для размещения чистовых отверстий АР в активном документе в местах расположений заданий на отверстия из связанных файлов ВИС
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного АР документа ревита, в котором будет происходить размещение чистовых отверстий</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public RealOpeningArPlacer(RevitRepository revitRepository, ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _config = OpeningRealsArConfig.GetOpeningConfig(_revitRepository.Doc);
    }


    /// <summary>
    /// Размещение чистового отверстия по одному заданию на отверстие из связи в одном хосте
    /// </summary>
#pragma warning disable 0618
    public void PlaceSingleOpeningByOneTask() {
        var host = _revitRepository.PickHostForRealOpening();
        var openingTask = _revitRepository.PickSingleOpeningMepTaskIncoming();

        using var transaction = _revitRepository.GetTransaction(
            _localization.GetLocalizedString("Transaction.PlaceSingleOpening"));
        try {
            if(openingTask.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())) {
                PlaceByOneTask(host, openingTask);
            } else {
                _revitRepository.ShowErrorMessage(
                    _localization.GetLocalizedString("Errors.TaskNotIntersectConstruction"));
                throw new OperationCanceledException();
            }
        } catch(OpeningNotPlacedException e) {
            _revitRepository.ShowErrorMessage(e.Message);
            throw new OperationCanceledException();
        }
        transaction.Commit();
    }
#pragma warning restore 0618

    /// <summary>
    /// Размещение объединенного чистового отверстия по одному или нескольким заданиям на отверстия из связи(ей) в одном хосте
    /// </summary>
#pragma warning disable 0618
    public void PlaceUnitedOpeningByManyTasks() {
        var host = _revitRepository.PickHostForRealOpening();
        var openingTasks = _revitRepository.PickManyOpeningMepTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();

        try {
            if(openingTasks.Count > 0) {
                using var transaction = _revitRepository.GetTransaction(
                    _localization.GetLocalizedString("Transaction.PlaceUnitedOpening"));
                PlaceUnitedByManyTasks(host, openingTasks);

                transaction.Commit();
            } else {
                _revitRepository.ShowErrorMessage(
                    _localization.GetLocalizedString("Errors.TasksNotIntersectConstruction"));
                throw new OperationCanceledException();
            }
        } catch(OpeningNotPlacedException e) {
            _revitRepository.ShowErrorMessage(e.Message);
            throw new OperationCanceledException();
        }
    }
#pragma warning restore 0618

    /// <summary>
    /// Размещение нескольких одиночных чистовых отверстий по нескольким заданиям на отверстия из связи(ей) без их объединения в одном хосте
    /// </summary>
#pragma warning disable 0618
    public void PlaceSingleOpeningsInOneHost() {
        var host = _revitRepository.PickHostForRealOpening();
        var openingTasks = _revitRepository.PickManyOpeningMepTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();

        string error = string.Empty;
        using(var transaction = _revitRepository.GetTransaction(
            _localization.GetLocalizedString("Transaction.PlaceSingleOpeningsInOneHost"))) {
            foreach(var openingTask in openingTasks) {
                try {
                    PlaceByOneTask(host, openingTask);

                } catch(OpeningNotPlacedException e) {
                    error = _localization.GetLocalizedString("Errors.CannotAcceptTask",
                        openingTask.Id,
                        openingTask.FileName,
                        e.Message);
                }
            }
            transaction.Commit();
        }
        if(!string.IsNullOrWhiteSpace(error)) {
            _revitRepository.ShowErrorMessage(error);
        }
    }
#pragma warning restore 0618

    /// <summary>
    /// Размещение нескольких одиночных чистовых отверстий в выбранных хостах по всем заданиям на отверстия из связи(ей), которые пересекаются с этими хостами
    /// </summary>
#pragma warning disable 0618
    public void PlaceSingleOpeningsInManyHosts() {
        var hosts = _revitRepository.PickHostsForRealOpenings();
        var allOpeningTasks = _revitRepository.GetOpeningsMepTasksIncoming();

        var sb = new StringBuilder();
        using(var transaction = _revitRepository.GetTransaction(
            _localization.GetLocalizedString("Transaction.PlaceSingleOpeningsInManyHosts"))) {
            using(var progressBar = _revitRepository.GetProgressDialogService()) {
                progressBar.StepValue = 1;
                progressBar.DisplayTitleFormat = _localization.GetLocalizedString("Progress.ProcessConstructions");
                var progress = progressBar.CreateProgress();
                progressBar.MaxValue = hosts.Count();
                var ct = progressBar.CreateCancellationToken();
                progressBar.Show();

                int i = 0;
                foreach(var host in hosts) {
                    ct.ThrowIfCancellationRequested();

                    ICollection<OpeningMepTaskIncoming> openingTasks = allOpeningTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();
                    if(openingTasks.Count == 0) {
                        sb.AppendLine(_localization.GetLocalizedString("Errors.StructureNotIntersectTask", host.Id));
                    }
                    foreach(var openingTask in openingTasks) {
                        try {
                            PlaceByOneTask(host, openingTask);

                        } catch(OpeningNotPlacedException e) {
                            sb.AppendLine(_localization.GetLocalizedString(
                                "Errors.CannotAcceptTask",
                                openingTask.Id,
                                openingTask.FileName,
                                e.Message));
                        }
                    }

                    i++;
                    progress.Report(i);
                }
            }

            transaction.Commit();
        }
        if(sb.Length > 0) {
            _revitRepository.ShowErrorMessage(sb.ToString());
        }
    }
#pragma warning restore 0618


    /// <summary>
    /// Размещает экземпляр семейства чистового отверстия по одному заданию на отверстие, принимая точку вставки и параметры габаритов из задания на отверстие.
    /// <para>Транзакция внутри метода не запускается</para>
    /// </summary>
    /// <param name="host">Основа для чистового отверстия - стена или перекрытие</param>
    /// <param name="openingTask">Входящее задание на отверстие</param>
    /// <exception cref="OpeningNotPlacedException"/>
    private void PlaceByOneTask(Element host, OpeningMepTaskIncoming openingTask) {
        try {
            var symbol = GetFamilySymbol(host, openingTask);
            var pointFinder = GetPointFinder(openingTask, _config.ElevationRounding);
            var point = pointFinder.GetPoint();

            var instance = _revitRepository.CreateInstance(point, symbol, host) ?? throw new OpeningNotPlacedException("Не удалось создать экземпляр семейства");

            var parameterGetter = GetParameterGetter(openingTask, pointFinder, _config.Rounding);
            SetParamValues(instance, parameterGetter);

            var angleFinder = GetAngleFinder(openingTask);
            _revitRepository.RotateElement(instance, point, angleFinder.GetAngle());

            _revitRepository.SetSelection(instance.Id);

        } catch(Autodesk.Revit.Exceptions.ArgumentNullException exAutodeskNull) {
            throw new OpeningNotPlacedException(exAutodeskNull.Message);
        } catch(Autodesk.Revit.Exceptions.ArgumentException exAutodeskArg) {
            throw new OpeningNotPlacedException(exAutodeskArg.Message);
        } catch(ArgumentNullException exFrameworkNull) {
            throw new OpeningNotPlacedException(exFrameworkNull.Message);
        } catch(ArgumentException exFrameworkArg) {
            throw new OpeningNotPlacedException(exFrameworkArg.Message);
        }
    }

    /// <summary>
    /// Размещает объединенное прямоугольное чистовое отверстие по одному или нескольким заданиям на отверстия из коллекции
    /// <para>Транзакция внутри метода не запускается</para>
    /// </summary>
    /// <param name="host">Основа для чистового отверстия</param>
    /// <param name="openingTasks">Входящие задания на отверстия</param>
    /// <exception cref="OpeningNotPlacedException">Исключение, если не удалось разместить объединенное отверстие</exception>
    private void PlaceUnitedByManyTasks(Element host, ICollection<OpeningMepTaskIncoming> openingTasks) {
        try {
            var symbol = GetFamilySymbol(host);
            var pointFinder = GetPointFinder(host, openingTasks, _config.ElevationRounding);
            var point = pointFinder.GetPoint();

            var instance = _revitRepository.CreateInstance(point, symbol, host) ?? throw new OpeningNotPlacedException("Не удалось создать экземпляр семейства");

            var parameterGetter = GetParameterGetter(host, openingTasks, pointFinder, _config.Rounding);
            SetParamValues(instance, parameterGetter);

            _revitRepository.SetSelection(instance.Id);

        } catch(Autodesk.Revit.Exceptions.ArgumentNullException exAutodeskNull) {
            throw new OpeningNotPlacedException(exAutodeskNull.Message);
        } catch(Autodesk.Revit.Exceptions.ArgumentException exAutodeskArg) {
            throw new OpeningNotPlacedException(exAutodeskArg.Message);
        } catch(ArgumentNullException exFrameworkNull) {
            throw new OpeningNotPlacedException(exFrameworkNull.Message);
        } catch(ArgumentException exFrameworkArg) {
            throw new OpeningNotPlacedException(exFrameworkArg.Message);
        }
    }

    /// <summary>
    /// Возвращает типоразмер семейства чистового отверстия на основе хоста и входящего задания на отверстие
    /// </summary>
    /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    private FamilySymbol GetFamilySymbol(Element host, OpeningMepTaskIncoming incomingTask) {
        var provider = new SingleOpeningTaskFamilySymbolProvider(_revitRepository, host, incomingTask);
        return provider.GetFamilySymbol();
    }

    /// <summary>
    /// Возвращает типоразмер семейства чистового отверстия на основе хоста
    /// </summary>
    /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
    private FamilySymbol GetFamilySymbol(Element host) {
        var provider = new RectangleFamilySymbolProvider(_revitRepository, host);
        return provider.GetFamilySymbol();
    }

    /// <summary>
    /// Назначает параметры экземпляра семейства чистового отверстия
    /// </summary>
    /// <param name="opening">Размещенное чистовое отверстие</param>
    /// <param name="parameterGetter">Класс, предоставляющий параметры</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private void SetParamValues(FamilyInstance opening, IParametersGetter parameterGetter) {
        if(opening is null) { throw new ArgumentNullException(nameof(opening)); }
        if(parameterGetter is null) { throw new ArgumentNullException(nameof(parameterGetter)); }

        foreach(var paramValue in parameterGetter.GetParamValues()) {
            paramValue.Value.SetParamValue(opening, paramValue.ParamName);
        }
    }

    /// <summary>
    /// Возвращает интерфейс, предоставляющий параметры экземпляра чистового отверстия для размещения по одному входящему заданию
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    private IParametersGetter GetParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder, int rounding) {
        var provider = new SingleOpeningTaskParameterGettersProvider(incomingTask, pointFinder, rounding);
        return provider.GetParametersGetter();
    }

    /// <summary>
    /// Возвращает интерфейс, предоставляющий параметры экземпляра чистового отверстия для размещения по нескольким входящим заданиям
    /// </summary>
    /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    private IParametersGetter GetParameterGetter(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder, int rounding) {
        return new ManyOpeningTasksParameterGettersProvider(host, incomingTasks, pointFinder, rounding).GetParametersGetter();
    }

    /// <summary>
    /// Возвращает интерфейс, предоставляющий угол поворота чистового отверстия для размещения по одному входящему заданию
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    private IAngleFinder GetAngleFinder(OpeningMepTaskIncoming incomingTask) {
        var provider = new SingleOpeningTaskAngleFinderProvider(incomingTask);
        return provider.GetAngleFinder();
    }

    /// <summary>
    /// Возвращает интерфейс, предоставляющий точку вставки
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление высотной отметки в мм</param>
    private IPointFinder GetPointFinder(OpeningMepTaskIncoming incomingTask, int rounding) {
        var provider = new SingleOpeningTaskPointFinderProvider(incomingTask, rounding);
        return provider.GetPointFinder();
    }


    /// <summary>
    /// Возвращает интерфейс, предоставляющий точку вставки чистового отверстия для размещения по нескольким входящим заданиям
    /// </summary>
    /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="rounding">Округление высотной отметки в мм</param>
    private IPointFinder GetPointFinder(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, int rounding) {
        var provider = new ManyOpeningTasksPointFinderProvider(host, incomingTasks, rounding);
        return provider.GetPointFinder();
    }
}
