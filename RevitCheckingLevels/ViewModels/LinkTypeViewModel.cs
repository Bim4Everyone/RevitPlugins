using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCheckingLevels.ViewModels {
    internal class LinkTypeViewModel : BaseViewModel {
        private readonly RevitLinkType _linkType;
        private readonly Workset _workset;
        private string _linkLoadToolTip;

        public LinkTypeViewModel(RevitLinkType linkType) {
            _linkType = linkType;
            _workset = _linkType.Document.GetWorksetTable().GetWorkset(_linkType.WorksetId);

            LinkLoadCommand = new RelayCommand(LinkLoad, CanLinkLoad);
        }

        public RevitLinkType Element => _linkType;

        public ElementId Id => _linkType.Id;
        public string Name => _linkType.Name;
        public bool IsLinkLoaded => _linkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;

        public ICommand LinkLoadCommand { get; }

        public string LinkLoadToolTip {
            get => _linkLoadToolTip;
            set => this.RaiseAndSetIfChanged(ref _linkLoadToolTip, value);
        }

        private void LinkLoad(object p) {
            _linkType.Load();
            OnPropertyChanged(nameof(IsLinkLoaded));
        }

        private bool CanLinkLoad(object p) {
            if(IsLinkLoaded) {
                LinkLoadToolTip = "Данная связь уже загружена.";
                return false;
            }

            if(!_workset.IsOpen) {
                LinkLoadToolTip = $"Откройте рабочий набор \"{_workset.Name}\"."
                                  + Environment.NewLine
                                  + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!";

                return false;
            }

            LinkLoadToolTip = "Загрузить координационный файл";
            return true;

        }
    }
}
