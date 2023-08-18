using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningPlacement.Providers;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement {
    /// <summary>
    /// Класс для размещения чистовых отверстий в активном документа в местах расположений заданий на отверстия из связанных файлов
    /// </summary>
    internal class RealOpeningPlacer {
        private readonly RevitRepository _revitRepository;

        public const string RealOpeningDiameter = "ФОП_РАЗМ_Диаметр";
        public const string RealOpeningWidth = "ФОП_РАЗМ_Ширина проёма";
        public const string RealOpeningHeight = "ФОП_РАЗМ_Высота проёма";


        /// <summary>
        /// Конструктор класса для размещения чистовых отверстий в активном документа в местах расположений заданий на отверстия из связанных файлов
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа ревита, в котором будет происходить размещение чистовых отверстийЫ</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RealOpeningPlacer(RevitRepository revitRepository) {
            if(revitRepository == null) { throw new ArgumentNullException(nameof(revitRepository)); }
            _revitRepository = revitRepository;
        }


        /// <summary>
        /// Размещение чистового отверстия по одному заданию на отверстие из связи
        /// </summary>
        public void PlaceBySingleTask() {
            Element host = _revitRepository.PickHostForRealOpening();
            OpeningMepTaskIncoming openingTask = _revitRepository.PickSingleOpeningTaskIncoming();

            try {
                if(openingTask.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())) {
                    PlaceBySimpleAlgorithm(host, openingTask);
                } else {
                    _revitRepository.GetMessageBoxService().Show(
                        "Выбранное задание на отверстие не пересекается c выбранной основой",
                        "Задания на отверстия",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error,
                        System.Windows.MessageBoxResult.OK);
                    throw new OperationCanceledException();
                }
            } catch(OpeningNotPlacedException e) {
                IMessageBoxService dialog = _revitRepository.GetMessageBoxService();
                dialog.Show(
                    $"{e.Message}",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Размещение чистового отверстия по одному или нескольким заданиям на отверстия из связи(ей)
        /// </summary>
        public void PlaceByManyTasks() {
            Element host = _revitRepository.PickHostForRealOpening();
            List<OpeningMepTaskIncoming> openingTasks = _revitRepository.PickManyOpeningTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToList();

            try {
                if(openingTasks.Count == 0) {
                    _revitRepository.GetMessageBoxService().Show(
                        "Ни одно из выбранных заданий на отверстия не пересекается c выбранной основой",
                        "Задания на отверстия",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error,
                        System.Windows.MessageBoxResult.OK);
                    throw new OperationCanceledException();
                } else if(openingTasks.Count == 1) {
                    var openingTask = openingTasks.First();
                    PlaceBySimpleAlgorithm(host, openingTask);
                } else {
                    PlaceByComplexAlgorithm(host, openingTasks);
                }
            } catch(OpeningNotPlacedException e) {
                IMessageBoxService dialog = _revitRepository.GetMessageBoxService();
                dialog.Show(
                    $"{e.Message}",
                    "Задания на отверстия",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error,
                    System.Windows.MessageBoxResult.OK);
                throw new OperationCanceledException();
            }
        }


        /// <summary>
        /// Размещает экземпляр семейства чистового отверстия, принимая расположение по точке вставки задания на отверстия
        /// </summary>
        /// <param name="host"></param>
        /// <param name="openingTask"></param>
        /// <exception cref="OpeningNotPlacedException"/>
        private void PlaceBySimpleAlgorithm(Element host, OpeningMepTaskIncoming openingTask) {
            using(var transaction = _revitRepository.GetTransaction("Размещение чистового отверстия")) {
                try {
                    var symbol = GetFamilySymbol(host, openingTask);
                    var pointFinder = GetPointFinder(openingTask);
                    var point = pointFinder.GetPoint();

                    var instance = _revitRepository.CreateInstance(point, symbol, host) ?? throw new OpeningNotPlacedException("Не удалось создать экземпляр семейства");

                    var parameterGetter = GetParameterGetter(openingTask, pointFinder);
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

                transaction.Commit();
            }
        }

        private void PlaceByComplexAlgorithm(Element host, ICollection<OpeningMepTaskIncoming> openingTasks) {

        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия
        /// </summary>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private FamilySymbol GetFamilySymbol(Element host, OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskFamilySymbolProvider(_revitRepository, host, incomingTask);
            return provider.GetFamilySymbol();
        }

        /// <summary>
        /// Назначает параметры экземпляра семейства чистового отверстия
        /// </summary>
        /// <param name="opening">Размещенное чистовое отверстие</param>
        /// <param name="parameterGetter">Класс, предоставляющий параметры</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void SetParamValues(FamilyInstance opening, IParametersGetter parameterGetter) {
            if(opening is null) { throw new ArgumentNullException(nameof(opening)); }
            if(parameterGetter is null) { throw new ArgumentNullException(nameof(parameterGetter)); }

            foreach(var paramValue in parameterGetter.GetParamValues()) {
                paramValue.Value.SetParamValue(opening, paramValue.ParamName);
            }
        }

        /// <summary>
        /// Возвращает класс, предоставляющий параметры экземпляра чистового отверстия
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отврестия</param>
        /// <returns></returns>
        private IParametersGetter GetParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            var provider = new SingleOpeningTaskParameterGettersProvider(incomingTask, pointFinder);
            return provider.GetParametersGetter();
        }

        /// <summary>
        /// Возвращает угол поворота чистового отверстия
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IAngleFinder GetAngleFinder(OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskAngleFinderProvider(incomingTask);
            return provider.GetAngleFinder();
        }

        /// <summary>
        /// Возвращает точку вставки
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IPointFinder GetPointFinder(OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskPointFinderProvider(incomingTask);
            return provider.GetPointFinder();
        }
    }
}
