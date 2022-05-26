using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashViewModel : BaseViewModel {
        private string _firstName;
        private string _firstDocumentName;
        private string _secondName;
        private string _secondDocumentName;
        private string _firstLevel;
        private string _secondLevel;
        private readonly ClashModel _clash;

        public ClashViewModel(ClashModel clash) {
            var levelPropertyName = LabelUtils.GetLabelFor(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM);

            FirstCategory = clash.MainElement.Category?.Name;
            FirstName = clash.MainElement.Name;
            FirstDocumentName = clash.MainElement.Document.Title;
            FirstLevel = GetLevel(clash.MainElement, levelPropertyName);

            SecondCategory = clash.OtherElement.Category?.Name;
            SecondName = clash.OtherElement.Name;
            SecondLevel = GetLevel(clash.OtherElement, levelPropertyName);
            SecondDocumentName = clash.OtherElement.Document.Title;

            _clash = clash;
        }

        public string FirstName {
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        public string FirstDocumentName {
            get => _firstDocumentName;
            set => this.RaiseAndSetIfChanged(ref _firstDocumentName, value);
        }

        public string FirstLevel {
            get => _firstLevel;
            set => this.RaiseAndSetIfChanged(ref _firstLevel, value);
        }

        public string FirstCategory { get; set; }

        public string SecondName {
            get => _secondName;
            set => this.RaiseAndSetIfChanged(ref _secondName, value);
        }

        public string SecondLevel {
            get => _secondLevel;
            set => this.RaiseAndSetIfChanged(ref _secondLevel, value);
        }

        public string SecondDocumentName {
            get => _secondDocumentName;
            set => this.RaiseAndSetIfChanged(ref _secondDocumentName, value);
        }

        public string SecondCategory { get; set; }

        public ElementId GetElementId() {
            return _clash.MainElement.Id;
        }

        private string GetLevel(Element element, string levelPropertyName) {
            var level = element.IsExistsParam(levelPropertyName) ? element.GetParam(levelPropertyName).AsValueString() : null;
            if(level == null) {
                level = element.LevelId == null ? null : element.Document.GetElement(element.LevelId)?.Name;
            }

            return level;
        }
    }
}
