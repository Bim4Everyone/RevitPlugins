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
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashViewModel : BaseViewModel {
        private bool _isSolved;

        public ClashViewModel(ClashModel clash) {
            FirstCategory = clash.MainElement.Category;
            FirstName = clash.MainElement.Name;
            FirstDocumentName = clash.MainElement.DocumentName;
            FirstLevel = clash.MainElement.Level;

            SecondCategory = clash.OtherElement.Category;
            SecondName = clash.OtherElement.Name;
            SecondLevel = clash.OtherElement.Level;
            SecondDocumentName = clash.OtherElement.DocumentName;

            IsSolved = clash.IsSolved;

            Clash = clash;
        }

        public bool IsSolved {
            get => _isSolved;
            set => this.RaiseAndSetIfChanged(ref _isSolved, value);
        }

        public string FirstName { get; }

        public string FirstDocumentName { get; }

        public string FirstLevel { get; }

        public string FirstCategory { get; }

        public string SecondName { get; }

        public string SecondLevel { get; }

        public string SecondDocumentName { get; }

        public string SecondCategory { get; set; }
        public ClashModel Clash { get; }

        public ElementId GetElementId(string docTitle) {
            if(docTitle.Contains(FirstDocumentName)) {
                return new ElementId(Clash.MainElement.Id);
            }
            if(docTitle.Contains(SecondDocumentName)) {
                return new ElementId(Clash.OtherElement.Id);
            }
            return ElementId.InvalidElementId;
        }
    }
}