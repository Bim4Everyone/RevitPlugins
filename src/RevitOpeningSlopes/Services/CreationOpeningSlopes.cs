using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Exceptions;

namespace RevitOpeningSlopes.Services {
    internal class CreationOpeningSlopes {
        private readonly RevitRepository _revitRepository;
        private readonly SlopeParams _slopeParams;
        private readonly SlopesDataGetter _slopesDataGetter;

        public CreationOpeningSlopes(
            RevitRepository revitRepository,
            SlopesDataGetter slopesDataGetter,
            SlopeParams slopeParams) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _slopesDataGetter = slopesDataGetter
                ?? throw new ArgumentNullException(nameof(slopesDataGetter));
            _slopeParams = slopeParams
                ?? throw new ArgumentNullException(nameof(slopeParams));
        }

        /// <summary>
        /// Метод для создания откоса с заданными параметрами
        /// </summary>
        /// <param name="slopeCreationData">Информация с необходимыми параметрами для откоса</param>
        public void CreateSlope(SlopeCreationData slopeCreationData) {
            FamilySymbol slopeType = _revitRepository.GetSlopeType(slopeCreationData.SlopeTypeId);
            if(!slopeType.IsActive) {
                slopeType.Activate();
            }
            FamilyInstance slope = _revitRepository
                        .Document
                        .Create
                        .NewFamilyInstance(slopeCreationData.Center, slopeType, StructuralType.NonStructural);
            _slopeParams.SetSlopeParams(slope, slopeCreationData);
        }

        /// <summary>
        /// Метод по созданию откосов по коллекции окон, выбранных пользователем со шкалой прогресса
        /// </summary>
        /// <param name="config">Настройки плагина</param>
        /// <param name="openings">Экземпляры окон, выбранные пользователем</param>
        /// <param name="error">Текст ошибок построения откосов</param>
        /// <param name="progress">Прогресс шкалы выполнения</param>
        /// <param name="ct">Отмена выполнения со шкалой прогресса</param>
        /// <exception cref="ArgumentNullException">Срабатывает, если настройки плагина равны null</exception>
        public void CreateSlopes(PluginConfig config,
            ICollection<FamilyInstance> openings,
            out string error,
            IProgress<int> progress = null,
            CancellationToken ct = default) {
            if(config is null) { throw new ArgumentNullException(nameof(config)); }
            StringBuilder sb = new StringBuilder();
            using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
                int i = 0;
                foreach(FamilyInstance opening in openings) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i++);
                    try {
                        SlopeCreationData slopeCreationData = _slopesDataGetter
                            .GetOpeningSlopeCreationData(config, opening);
                        CreateSlope(slopeCreationData);
                    } catch(OpeningNullSolidException e) {
                        sb.AppendLine($"{e.Message}, Id = {opening.Id}");
                    } catch(ArgumentException e) {
                        sb.AppendLine($"{e.Message}, Id = {opening.Id}");
                    }
                }
                transaction.Commit();
            }
            error = sb.ToString();
        }
    }
}
