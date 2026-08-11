using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitMechanicalSpecification.Service {
    internal class ElementEditChecker {
        private const string UnknownEditor = "неизвестный пользователь";

        private readonly Document _document;
        private readonly IMessageBoxService _messageBoxService;
        private readonly HashSet<string> _editors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _hasUpdatedInCentralElements;

        public ElementEditChecker(
            Document document,
            IMessageBoxService messageBoxService) {
            _document = document;
            _messageBoxService = messageBoxService;
        }

        public bool IsUnavailableForEdit(Element element) {
            if(!CanCheckElement(element)) {
                return false;
            }

            if(RegisterEditor(element)) {
                return true;
            }

            ModelUpdatesStatus updateStatus = WorksharingUtils.GetModelUpdatesStatus(_document, element.Id);
            bool isUpdatedInCentral = updateStatus == ModelUpdatesStatus.UpdatedInCentral;
            if(isUpdatedInCentral) {
                _hasUpdatedInCentralElements = true;
            }

            return isUpdatedInCentral;
        }

        public bool IsEditedByOtherUser(Element element) {
            return CanCheckElement(element) && RegisterEditor(element);
        }

        private bool CanCheckElement(Element element) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            return _document.IsWorkshared;
        }

        private bool RegisterEditor(Element element) {
            CheckoutStatus status = WorksharingUtils.GetCheckoutStatus(
                _document,
                element.Id,
                out string editor);

            bool isEditedByOtherUser = status == CheckoutStatus.OwnedByOtherUser;
            if(isEditedByOtherUser) {
                _editors.Add(string.IsNullOrWhiteSpace(editor) ? UnknownEditor : editor);
            }

            return isEditedByOtherUser;
        }

        public void ShowReport() {
            if(!_hasUpdatedInCentralElements && _editors.Count == 0) {
                return;
            }

            List<string> reports = new List<string>();
            if(_hasUpdatedInCentralElements) {
                reports.Add("Вы владеете элементами, но ваш файл устарел. Выполните синхронизацию.");
            }

            if(_editors.Count > 0) {
                string editors = string.Join(", ",
                    _editors.OrderBy(item => item, StringComparer.OrdinalIgnoreCase));
                reports.Add(
                    "Некоторые элементы не были обработаны, так как заняты пользователем/пользователями: "
                    + editors);
            }

            _messageBoxService.Show(
                string.Join(Environment.NewLine, reports),
                "Обновление спецификации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            _hasUpdatedInCentralElements = false;
            _editors.Clear();
        }
    }
}
