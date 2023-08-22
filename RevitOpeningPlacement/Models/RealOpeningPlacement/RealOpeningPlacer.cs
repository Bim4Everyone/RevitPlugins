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
                ShowErrorMessage(e.Message);
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
                } else {
                    PlaceByComplexAlgorithm(host, openingTasks);
                }
            } catch(OpeningNotPlacedException e) {
                ShowErrorMessage(e.Message);
                throw new OperationCanceledException();
            }
        }


        /// <summary>
        /// Выводит сообщение об ошибке пользователю
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        private void ShowErrorMessage(string message) {
            IMessageBoxService dialog = _revitRepository.GetMessageBoxService();
            dialog.Show(
                $"{message}",
                "Задания на отверстия",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }

        /// <summary>
        /// Размещает экземпляр семейства чистового отверстия, принимая точку вставки и параметры габаритов из задания на отверстия
        /// </summary>
        /// <param name="host">Основа для чистового отверстия - стена или перекрытие</param>
        /// <param name="openingTask">Входящее задание на отверстие</param>
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

        /// <summary>
        /// Размещает прямоугольное чистовое отверстие на месте одного нескольких заданий на отверстия
        /// </summary>
        /// <param name="host">Основа для чистового отверстия</param>
        /// <param name="openingTasks">Входящие задания на отверстия</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceByComplexAlgorithm(Element host, ICollection<OpeningMepTaskIncoming> openingTasks) {
            using(var transaction = _revitRepository.GetTransaction("Размещение чистового отверстия")) {
                try {
                    var symbol = GetFamilySymbol(host);
                    var pointFinder = GetPointFinder(host, openingTasks);
                    var point = pointFinder.GetPoint();

                    var instance = _revitRepository.CreateInstance(point, symbol, host) ?? throw new OpeningNotPlacedException("Не удалось создать экземпляр семейства");

                    var parameterGetter = GetParameterGetter(host, openingTasks, pointFinder);
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

                transaction.Commit();
            }
        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия на основе хоста и входящего задания на отверстие
        /// </summary>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private FamilySymbol GetFamilySymbol(Element host, OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskFamilySymbolProvider(_revitRepository, host, incomingTask);
            return provider.GetFamilySymbol();
        }

        /// <summary>
        /// Возвращает типоразмер семейства чистового отверстия на основе хоста
        /// </summary>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <returns></returns>
        private FamilySymbol GetFamilySymbol(Element host) {
            var provider = new FamilySymbolProvider(_revitRepository, host);
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
        /// Возвращает интерфейс, предоставляющий параметры экземпляра чистового отверстия для размещения по одному входящему заданию
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        /// <returns></returns>
        private IParametersGetter GetParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            var provider = new SingleOpeningTaskParameterGettersProvider(incomingTask, pointFinder);
            return provider.GetParametersGetter();
        }

        /// <summary>
        /// Возвращает интерфейс, предоставляющий параметры экземпляра чистового отверстия для размещения по нескольким входящим заданиям
        /// </summary>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        /// <returns></returns>
        private IParametersGetter GetParameterGetter(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            return new ManyOpeningTasksParameterGettersProvider(host, incomingTasks, pointFinder).GetParametersGetter();
        }

        /// <summary>
        /// Возвращает интерфейс, предоставляющий угол поворота чистового отверстия для размещения по одному входящему заданию
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IAngleFinder GetAngleFinder(OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskAngleFinderProvider(incomingTask);
            return provider.GetAngleFinder();
        }

        /// <summary>
        /// Возвращает интерфейс, предоставлящий точку вставки
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IPointFinder GetPointFinder(OpeningMepTaskIncoming incomingTask) {
            var provider = new SingleOpeningTaskPointFinderProvider(incomingTask);
            return provider.GetPointFinder();
        }


        /// <summary>
        /// Возвращает интерфейс, предоставляющий точку вставки чистового отверстия для размещения по нескольким входящим заданиям
        /// </summary>
        /// <param name="host">Хост чистового отверстия - стена или перекрытие</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <returns></returns>
        private IPointFinder GetPointFinder(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks) {
            var provider = new ManyOpeningTasksPointFinderProvider(host, incomingTasks);
            return provider.GetPointFinder();
        }
    }
}
