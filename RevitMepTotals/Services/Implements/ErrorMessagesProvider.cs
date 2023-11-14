using System;
using System.Collections.Generic;

namespace RevitMepTotals.Services.Implements {
    internal class ErrorMessagesProvider : IErrorMessagesProvider {
        private readonly IConstantsProvider _constantsProvider;


        public ErrorMessagesProvider(IConstantsProvider constantsProvider) {
            _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        }


        public string GetFileNamesConflictMessage(ICollection<string> conflictNames) {
            return $"{string.Join(Environment.NewLine, conflictNames)}" +
                $"\nЭти документы нельзя выгрузить за один раз, т.к. они образуют конфликт имен в листах Excel." +
                $"\nИмя листа Excel должно быть не более {_constantsProvider.DocNameMaxLength} символа" +
                $"\nи не должно содержать {string.Join(", ", _constantsProvider.ProhibitedExcelChars)} ";
        }

        public string GetFileAlreadyOpenedMessage(string path) {
            return $"Документ \'{path}\' нельзя обработать, т.к. он уже открыт.";
        }

        public string GetFileVersionIsInvalidMessage(string path) {
            return $"Документ \'{path}\' нельзя обработать, т.к. он создан в более поздней версии.";
        }

        public string GetFileDataCorruptionMessage(string path) {
            return $"Документ \'{path}\' нельзя обработать, т.к. в нем слишком много ошибок.";
        }

        public string GetFileCannotBeProcessMessage(string path) {
            return $"Документ \'{path}\' нельзя обработать.";
        }

        public string GetFileRemovedMessage(string path) {
            return $"Документ \'{path}\' удален.";
        }

        public string GetInsufficientResourcesMessage() {
            return "У компьютера недостаточно ресурсов, чтобы открыть модель.";
        }
    }
}
