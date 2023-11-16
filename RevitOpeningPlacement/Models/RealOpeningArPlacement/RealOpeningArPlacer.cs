using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement {
    /// <summary>
    /// Класс для размещения чистовых отверстий АР в активном документе в местах расположений заданий на отверстия из связанных файлов ВИС
    /// </summary>
    internal class RealOpeningArPlacer {
        private readonly RevitRepository _revitRepository;

        public const string RealOpeningArDiameter = "ФОП_РАЗМ_Диаметр";
        public const string RealOpeningArWidth = "ФОП_РАЗМ_Ширина проёма";
        public const string RealOpeningArHeight = "ФОП_РАЗМ_Высота проёма";
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
        /// <exception cref="ArgumentNullException"></exception>
        public RealOpeningArPlacer(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        /// <summary>
        /// Размещение чистового отверстия по одному заданию на отверстие из связи в одном хосте
        /// </summary>
        public void PlaceSingleOpeningByOneTask() {
            Element host = _revitRepository.PickHostForRealOpening();
            OpeningMepTaskIncoming openingTask = _revitRepository.PickSingleOpeningMepTaskIncoming();

            using(var transaction = _revitRepository.GetTransaction("Размещение одиночного отверстия")) {
                try {
                    if(openingTask.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())) {
                        PlaceByOneTask(host, openingTask);
                    } else {
                        _revitRepository.ShowErrorMessage("Выбранное задание на отверстие не пересекается c выбранной основой");
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
        /// Размещение объединенного чистового отверстия по одному или нескольким заданиям на отверстия из связи(ей) в одном хосте
        /// </summary>
        public void PlaceUnitedOpeningByManyTasks() {
            Element host = _revitRepository.PickHostForRealOpening();
            HashSet<OpeningMepTaskIncoming> openingTasks = _revitRepository.PickManyOpeningMepTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();

            try {
                if(openingTasks.Count > 0) {
                    using(var transaction = _revitRepository.GetTransaction("Размещение объединенного отверстия")) {
                        PlaceUnitedByManyTasks(host, openingTasks);

                        transaction.Commit();
                    }
                } else {
                    _revitRepository.ShowErrorMessage("Ни одно из выбранных заданий на отверстия не пересекается c выбранной основой");
                    throw new OperationCanceledException();
                }
            } catch(OpeningNotPlacedException e) {
                _revitRepository.ShowErrorMessage(e.Message);
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Размещение нескольких одиночных чистовых отверстий по нескольким заданиям на отверстия из связи(ей) без их объединения в одном хосте
        /// </summary>
        public void PlaceSingleOpeningsInOneHost() {
            Element host = _revitRepository.PickHostForRealOpening();
            HashSet<OpeningMepTaskIncoming> openingTasks = _revitRepository.PickManyOpeningMepTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();

            StringBuilder sb = new StringBuilder();
            using(var transaction = _revitRepository.GetTransaction("Одиночные отверстия в одном хосте")) {
                foreach(OpeningMepTaskIncoming openingTask in openingTasks) {
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
        /// Размещение нескольких одиночных чистовых отверстий в выбранных хостах по всем заданиям на отверстия из связи(ей), которые пересекаются с этими хостами
        /// </summary>
        public void PlaceSingleOpeningsInManyHosts() {
            ICollection<Element> hosts = _revitRepository.PickHostsForRealOpenings();
            ICollection<OpeningMepTaskIncoming> allOpeningTasks = _revitRepository.GetOpeningsMepTasksIncoming();

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

                        ICollection<OpeningMepTaskIncoming> openingTasks = allOpeningTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();
                        if(openingTasks.Count == 0) {
                            sb.AppendLine($"Конструкция с ID: {host.Id} не пересекается ни с одним заданием");
                        }
                        foreach(OpeningMepTaskIncoming openingTask in openingTasks) {
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
        /// Размещает экземпляр семейства чистового отверстия по одному заданию на отверстие, принимая точку вставки и параметры габаритов из задания на отверстие.
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия - стена или перекрытие</param>
        /// <param name="openingTask">Входящее задание на отверстие</param>
        /// <exception cref="OpeningNotPlacedException"/>
        private void PlaceByOneTask(Element host, OpeningMepTaskIncoming openingTask) {
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
        }

        /// <summary>
        /// Размещает объединенное прямоугольное чистовое отверстие по одному или нескольким заданиям на отверстия из коллекции
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия</param>
        /// <param name="openingTasks">Входящие задания на отверстия</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceUnitedByManyTasks(Element host, ICollection<OpeningMepTaskIncoming> openingTasks) {
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
            var provider = new RectangleFamilySymbolProvider(_revitRepository, host);
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
        /// Возвращает интерфейс, предоставляющий точку вставки
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
