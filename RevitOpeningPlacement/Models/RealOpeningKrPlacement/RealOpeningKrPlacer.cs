using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.PointFinders;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement {
    /// <summary>
    /// Класс для размещения чистовых отверстий КР в активном документе в местах расположений заданий на отверстия из связанных файлов АР или ВИС
    /// </summary>
    internal class RealOpeningKrPlacer {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningRealsKrConfig _config;

        public const string RealOpeningKrDiameter = "ФОП_РАЗМ_Диаметр";
        public const string RealOpeningKrInWallWidth = "ФОП_РАЗМ_Ширина";
        public const string RealOpeningKrInWallHeight = "ФОП_РАЗМ_Высота";
        public const string RealOpeningKrInFloorHeight = "мод_ФОП_Габарит А";
        public const string RealOpeningKrInFloorWidth = "мод_ФОП_Габарит Б";
        public const string RealOpeningTaskId = "ФОП_ID задания";

        private const string _configErrorMessage = "Настройки расстановки отверстий КР некорректны. Пересохраните их.";


        /// <summary>
        /// Конструктор класса для размещения чистовых отверстий КР в активном документе в местах расположений заданий на отверстия из связанных файлов АР или ВИС
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного КР документа ревита, в котором будет происходить размещение чистовых отверстий</param>
        /// <param name="config">Конфигурация расстановки заданий на отверстия в файле КР, в которой задается обработка заданий ВИС или АР</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public RealOpeningKrPlacer(RevitRepository revitRepository, OpeningRealsKrConfig config) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }


        public void PlaceSingleOpeningByOneTask() {
            Element host = _revitRepository.PickHostForRealOpening();
            IOpeningTaskIncoming openingTask = PickOpeningTaskIncoming(_config);
            PlaceSingleOpeningByOneIncomingTask(host, openingTask);
        }

        public void PlaceUnitedOpeningByManyTasks() {
            Element host = _revitRepository.PickHostForRealOpening();
            ICollection<IOpeningTaskIncoming> openingTasks = PickOpeningsTaskIncoming(_config);
            PlaceUnitedOpeningByManyIncomingTasks(
                host,
                openingTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToArray()
                );
        }

        public void PlaceSingleOpeningsInOneHost() {
            Element host = _revitRepository.PickHostForRealOpening();
            ICollection<IOpeningTaskIncoming> openingTasks = PickOpeningsTaskIncoming(_config);
            PlaceSingleOpeningsInOneHostByIncomingTasks(
                host,
                openingTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToArray()
                );
        }

        public void PlaceSingleOpeningsInManyHosts() {
            ICollection<Element> hosts = _revitRepository.PickHostsForRealOpenings();
            ICollection<IOpeningTaskIncoming> allOpeningTasks = GetAllOpeningsTaskIncoming(_config);
            PlaceSingleOpeningsInManyHostsByIncomingTasks(hosts, allOpeningTasks);
        }

        /// <summary>
        /// Возвращает входящее задание на отверстие, выбранное пользователем в соответствии с настройками
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private IOpeningTaskIncoming PickOpeningTaskIncoming(OpeningRealsKrConfig config) {
            if(config.PlacementType == OpeningRealKrPlacementType.PlaceByAr) {
                return _revitRepository.PickSingleOpeningArTaskIncoming();
            } else if(config.PlacementType == OpeningRealKrPlacementType.PlaceByMep) {
                return _revitRepository.PickSingleOpeningMepTaskIncoming();
            } else {
                _revitRepository.ShowErrorMessage(_configErrorMessage);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Возвращает входящие задания на отверстия, выбранные пользователем в соответствии с настройками
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private ICollection<IOpeningTaskIncoming> PickOpeningsTaskIncoming(OpeningRealsKrConfig config) {
            if(config.PlacementType == OpeningRealKrPlacementType.PlaceByAr) {
                return _revitRepository.PickManyOpeningArTasksIncoming().ToArray<IOpeningTaskIncoming>();
            } else if(config.PlacementType == OpeningRealKrPlacementType.PlaceByMep) {
                return _revitRepository.PickManyOpeningMepTasksIncoming().ToArray<IOpeningTaskIncoming>();
            } else {
                _revitRepository.ShowErrorMessage(_configErrorMessage);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Возвращает все входящие задания на отверстия в соответствии с заданными настройками
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        private ICollection<IOpeningTaskIncoming> GetAllOpeningsTaskIncoming(OpeningRealsKrConfig config) {
            if(config.PlacementType == OpeningRealKrPlacementType.PlaceByAr) {
                return _revitRepository.GetOpeningsArTasksIncoming().ToArray<IOpeningTaskIncoming>();
            } else if(config.PlacementType == OpeningRealKrPlacementType.PlaceByMep) {
                return _revitRepository.GetOpeningsMepTasksIncoming().ToArray<IOpeningTaskIncoming>();
            } else {
                _revitRepository.ShowErrorMessage(_configErrorMessage);
                throw new OperationCanceledException();
            }
        }


        /// <summary>
        /// Размещение чистового отверстия КР по одному заданию на отверстие из связи в одном хосте
        /// </summary>
        private void PlaceSingleOpeningByOneIncomingTask(Element host, IOpeningTaskIncoming openingTask) {
            using(var transaction = _revitRepository.GetTransaction("Размещение одиночного отверстия")) {
                try {
                    if(openingTask.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())) {
                        PlaceByOneTask(host, openingTask);
                    } else {
                        _revitRepository.ShowErrorMessage("Выбранное задание не пересекается с выбранной основой");
                        throw new OperationCanceledException();
                    }
                } catch(OpeningNotPlacedException e) {
                    _revitRepository.ShowErrorMessage(e.Message);
                    throw new OperationCanceledException();
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Размещение объединенного чистового отверстия КР по одному или нескольким заданиям на отверстия из связи(ей) в одном хосте
        /// </summary>
        private void PlaceUnitedOpeningByManyIncomingTasks(Element host, ICollection<IOpeningTaskIncoming> openingTasks) {
            try {
                if(openingTasks.Count > 0) {
                    using(var transaction = _revitRepository.GetTransaction("Размещение объединенного отверстия")) {
                        PlaceUnitedByManyTasks(host, openingTasks);
                        transaction.Commit();
                    }
                } else {
                    _revitRepository.ShowErrorMessage("Ни одно из выбранных заданий на отверстия не пересекается с выбранной основой");
                    throw new OperationCanceledException();
                }
            } catch(OpeningNotPlacedException e) {
                _revitRepository.ShowErrorMessage(e.Message);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Размещение нескольких одиночных чистовых отверстий КР по нескольким заданиям на отверстия из связи(ей) без их объединения
        /// </summary>
        private void PlaceSingleOpeningsInOneHostByIncomingTasks(Element host, ICollection<IOpeningTaskIncoming> openingTasks) {
            StringBuilder sb = new StringBuilder();
            using(var transaction = _revitRepository.GetTransaction("Одиночные отверстия в одном хосте")) {
                foreach(IOpeningTaskIncoming openingTask in openingTasks) {
                    try {
                        PlaceByOneTask(host, openingTask);

                    } catch(OpeningNotPlacedException e) {
                        sb.AppendLine($"Задание с ID: {openingTask.Id} из файла: \'{openingTask.FileName}\' не удалось принять. Информация об ошибке:");
                        sb.AppendLine(e.Message);
                        sb.AppendLine();
                    }
                }
                transaction.Commit();
            }
            if(sb.Length > 0) {
                _revitRepository.ShowErrorMessage(sb.ToString());
            }
        }

        /// <summary>
        /// Размещение нескольких одиночных чистовых отверстий КР в выбранных хостах по всем заданиям на отверстия из связи(ей), которые пересекаются с этими хостами
        /// </summary>
        private void PlaceSingleOpeningsInManyHostsByIncomingTasks(ICollection<Element> hosts, ICollection<IOpeningTaskIncoming> allOpeningTasks) {
            StringBuilder sb = new StringBuilder();
            using(var transaction = _revitRepository.GetTransaction("Одиночные отверстия в нескольких хостах")) {
                using(var progressBar = _revitRepository.GetProgressDialogService()) {
                    progressBar.StepValue = 1;
                    progressBar.DisplayTitleFormat = "Обработка конструкций... [{0}\\{1}]";
                    var progress = progressBar.CreateProgress();
                    progressBar.MaxValue = hosts.Count();
                    var ct = progressBar.CreateCancellationToken();
                    progressBar.Show();

                    int i = 0;
                    foreach(Element host in hosts) {
                        ct.ThrowIfCancellationRequested();

                        ICollection<IOpeningTaskIncoming> openingTasks = allOpeningTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();
                        if(openingTasks.Count == 0) {
                            sb.AppendLine($"Конструкция с ID: {host.Id} не пересекается ни с одним заданием");
                        }
                        foreach(IOpeningTaskIncoming openingTask in openingTasks) {
                            try {
                                PlaceByOneTask(host, openingTask);
                            } catch(OpeningNotPlacedException e) {
                                sb.AppendLine($"Задание с ID: {openingTask.Id} из файла: \'{openingTask.FileName}\' не удалось принять. Информация об ошибке:");
                                sb.AppendLine(e.Message);
                                sb.AppendLine();
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


        /// <summary>
        /// Размещает экземпляр семейства чистового отверстия по одному заданию на отверстие
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия КР - стена или перекрытие</param>
        /// <param name="openingTask">Входящее задание на отверстие</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceByOneTask(Element host, IOpeningTaskIncoming openingTask) {
            try {
                var symbol = GetFamilySymbol(host, openingTask.OpeningType);
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
        }

        /// <summary>
        /// Размещает объединенное прямоугольное чистовое отверстие по одному или нескольким заданиям на отверстия из коллекции
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия КР</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceUnitedByManyTasks(Element host, ICollection<IOpeningTaskIncoming> incomingTasks) {
            try {
                var symbol = GetFamilySymbol(host);
                var pointFinder = GetPointFinder(host, incomingTasks);
                var point = pointFinder.GetPoint();

                var instance = _revitRepository.CreateInstance(point, symbol, host);

                var parameterGetter = GetParameterGetter(host, incomingTasks, pointFinder);
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
        /// Возвращает интерфейс, предоставляющий угол поворота размещаемого КР отверстия
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IAngleFinder GetAngleFinder(IOpeningTaskIncoming incomingTask) {
            var provider = new SingleOpeningArTaskAngleFinderProvider(incomingTask);
            return provider.GetAngleFinder();
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
        /// Возвращает интерфейс, предоставляющий значения параметров для размещаемого отверстия КР
        /// </summary>
        /// <param name="incomingTask"></param>
        /// <param name="pointFinder"></param>
        /// <returns></returns>
        private IParametersGetter GetParameterGetter(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder) {
            var provider = new SingleOpeningArTaskParameterGettersProvider(incomingTask, pointFinder);
            return provider.GetParametersGetter();
        }

        /// <summary>
        /// Возвращает интерфейс, предоставляющий точку вставки отверстия КР для размещения по нескольким заданиям
        /// </summary>
        /// <param name="host">Основа для отверстия КР</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="pointFinder"></param>
        /// <returns></returns>
        private IParametersGetter GetParameterGetter(Element host, ICollection<IOpeningTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            var provider = new ManyOpeningArTasksParameterGettersProvider(host, incomingTasks, pointFinder);
            return provider.GetParametersGetter();
        }

        /// <summary>
        /// Возвращает интерфейс, предоставляющий точку вставки отверстия КР
        /// </summary>
        /// <param name="openingTask">Входящее задание на отверстие</param>
        /// <returns></returns>
        private IPointFinder GetPointFinder(IOpeningTaskIncoming openingTask) {
            return new SingleOpeningArTaskPointFinder(openingTask);
        }

        /// <summary>
        /// Возвращает интерфейс, предоставляющий точку вставки отверстия КР по нескольким заданиям
        /// </summary>
        /// <param name="host">Основа для отверстия КР</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <returns></returns>
        private IPointFinder GetPointFinder(Element host, ICollection<IOpeningTaskIncoming> incomingTasks) {
            var provider = new ManyOpeningArTasksPointFinderProvider(host, incomingTasks);
            return provider.GetPointFinder();
        }

        /// <summary>
        /// Возвращает типоразмер чистового отверстия КР на основе хоста и типа входящего задания на отверстие
        /// </summary>
        /// <param name="host"></param>
        /// <param name="openingTaskType"></param>
        /// <returns></returns>
        private FamilySymbol GetFamilySymbol(Element host, OpeningType openingTaskType) {
            var provider = new SingleOpeningArTaskFamilySymbolProvider(_revitRepository, host, openingTaskType);
            return provider.GetFamilySymbol();
        }

        /// <summary>
        /// Возвращает типоразмер чистового отверстия КР на основе хоста
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private FamilySymbol GetFamilySymbol(Element host) {
            var provider = new ManyOpeningArTasksFamilySymbolProvider(_revitRepository, host);
            return provider.GetFamilySymbol();
        }
    }
}
