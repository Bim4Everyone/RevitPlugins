using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement {
    /// <summary>
    /// Класс для размещения чистовых отверстий КР в активном документе в местах расположений заданий на отверстия из связанных файлов АР
    /// </summary>
    internal class RealOpeningKrPlacer {
        private readonly RevitRepository _revitRepository;

        public const string RealOpeningKrDiameter = "ФОП_РАЗМ_Диаметр";
        public const string RealOpeningKrInWallWidth = "ФОП_РАЗМ_Ширина";
        public const string RealOpeningKrInWallHeight = "ФОП_РАЗМ_Высота";
        public const string RealOpeningKrInFloorWidth = "мод_ФОП_Габарит А";
        public const string RealOpeningKrInFloorHeight = "мод_ФОП_Габарит Б";


        /// <summary>
        /// Конструктор класса для размещения чистовых отверстий КР в активном документе в местах расположений заданий на отверстия из связанных файлов АР
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного КР документа ревита, в котором будет происходить размещение чистовых отверстий</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public RealOpeningKrPlacer(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }

        /// <summary>
        /// Размещение чистового отверстия КР по одному заданию на отверстие из связи АР в одном хосте
        /// </summary>
        public void PlaceSingleOpeningByOneTask() {
            Element host = _revitRepository.PickHostForRealOpening();
            OpeningArTaskIncoming openingTask = _revitRepository.PickSingleOpeningArTaskIncoming();

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
        /// Размещение объединенного чистового отверстия КР по одному или нескольким заданиям на отверстия из связи(ей) АР в одном хосте
        /// </summary>
        public void PlaceUnitedOpeningByManyTasks() {
            Element host = _revitRepository.PickHostForRealOpening();
            HashSet<OpeningArTaskIncoming> openingTasks = _revitRepository
                .PickManyOpeningArTasksIncoming()
                .Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox()))
                .ToHashSet();
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
        /// Размещение нескольких одиночных чистовых отверстий КР по нескольким заданиям на отверстия из связи(ей) АР без их объединения
        /// </summary>
        public void PlaceSingleOpeningsInOneHost() {
            Element host = _revitRepository.PickHostForRealOpening();
            HashSet<OpeningArTaskIncoming> openingTasks = _revitRepository
                .PickManyOpeningArTasksIncoming()
                .Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox()))
                .ToHashSet();

            StringBuilder sb = new StringBuilder();
            using(var transaction = _revitRepository.GetTransaction("Одиночные отверстия в одном хосте")) {
                foreach(OpeningArTaskIncoming openingTask in openingTasks) {
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
        /// Размещение нескольких одиночных чистовых отверстий КР в выбранных хостах по всем заданиям на отверстия из связи(ей) АР, которые пересекаются с этими хостами
        /// </summary>
        public void PlaceSingleOpeningsInManyHosts() {
            ICollection<Element> hosts = _revitRepository.PickHostsForRealOpenings();
            ICollection<OpeningArTaskIncoming> allOpeningTasks = _revitRepository.GetOpeningsArTasksIncoming();

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

                        ICollection<OpeningArTaskIncoming> openingTasks = allOpeningTasks.Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToHashSet();
                        foreach(OpeningArTaskIncoming openingTask in openingTasks) {
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
        /// Размещает экземпляр семейства чистового отверстия по одному заданию на отверстие из АР
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия КР - стена или перекрытие</param>
        /// <param name="openingTask">Входящее задание на отверстие из АР</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceByOneTask(Element host, OpeningArTaskIncoming openingTask) {

        }

        /// <summary>
        /// Размещает объединенное прямоугольное чистовое отверстие по одному или нескольким заданиям на отверстия АР из коллекции
        /// <para>Транзакция внутри метода не запускается</para>
        /// </summary>
        /// <param name="host">Основа для чистового отверстия КР</param>
        /// <param name="openingTasks">Входящие задания на отверстия из АР</param>
        /// <exception cref="OpeningNotPlacedException"></exception>
        private void PlaceUnitedByManyTasks(Element host, HashSet<OpeningArTaskIncoming> openingTasks) {

        }
    }
}
