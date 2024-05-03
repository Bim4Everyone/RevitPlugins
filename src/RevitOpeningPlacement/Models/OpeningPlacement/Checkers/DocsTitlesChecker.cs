using System;
using System.Collections.Generic;

using dosymep.Bim4Everyone.SimpleServices;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    /// <summary>
    /// Класс для проверки заголовков связанных файлов ревита на соответствие БИМ стандарту А101
    /// https://kb.a101.ru/pages/viewpage.action?pageId=82635552
    /// </summary>
    internal class DocsTitlesChecker : IChecker {
        private readonly RevitRepository _revitRepository;

        public DocsTitlesChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public string GetErrorMessage() {
            var docs = _revitRepository.DocInfos;
            IBimModelPartsService bimPartsService = RevitRepository.GetBimModelPartsService();
            HashSet<string> notValidDocTitles = new HashSet<string>();
            foreach(var doc in docs) {
                if(bimPartsService.GetBimModelPart(doc.Name) is null) {
                    notValidDocTitles.Add(doc.Name);
                }
            }
            return $"Следующие названия файлов не соответствуют BIM стандарту А101:\n{string.Join(";\n", notValidDocTitles)}";
        }

        public bool IsCorrect() {
            var docs = _revitRepository.DocInfos;
            IBimModelPartsService bimPartsService = RevitRepository.GetBimModelPartsService();
            foreach(var doc in docs) {
                if(bimPartsService.GetBimModelPart(doc.Name) is null) {
                    return false;
                }
            }
            return true;
        }
    }
}
