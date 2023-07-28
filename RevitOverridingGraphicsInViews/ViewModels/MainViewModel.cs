using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Windows.Input;


using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOverridingGraphicsInViews.Models;

namespace RevitOverridingGraphicsInViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;



        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            PaintCommand = RelayCommand.Create(Paint);
            SelectColorCommand = RelayCommand.Create(SelectColor);
            TestCommand = RelayCommand.Create(Test);
            Test2Command = RelayCommand.Create(Test2);

        }


        public void Test() {

            TaskDialog.Show("ds", "test");
        }
        public void Test2() {

            TaskDialog.Show("ds", "test2");
        }


        public ICommand PaintCommand { get; }
        public ICommand SelectColorCommand { get; }
        public ICommand TestCommand { get; }
        public ICommand Test2Command { get; }


        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        private ColorHelper _color;
        public ColorHelper Color {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }


        public FillPatternElement SolidFillPattern { get; set; }


        private void SelectColor() {

            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {

                Color = new ColorHelper(
                    colorSelectionDialog.SelectedColor.Red,
                    colorSelectionDialog.SelectedColor.Green,
                    colorSelectionDialog.SelectedColor.Blue);
            }
        }



        private void Paint() {

            List<Element> elemsForWork = new List<Element>();

            ICollection<ElementId> selectedIds = _revitRepository.ActiveUIDocument.Selection.GetElementIds();

            foreach(ElementId id in selectedIds) {

                elemsForWork.Add(_revitRepository.Document.GetElement(id));
            }


            SolidFillPattern = FillPatternElement.GetFillPatternElementByName(_revitRepository.Document, FillPatternTarget.Drafting, "<Сплошная заливка>");

            // Либо выполняем прямое переопределение графики элементов на виде
            OverrideGraphicSettings settings = GetOverrideGraphicSettings();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор пилонов")) {

                foreach(View view in _revitRepository.AllViews) {

                    foreach(Element el in elemsForWork) {

                        try {
                            view.SetElementOverrides(el.Id, settings);

                        } catch(Exception) {

                        }
                    }
                }

                transaction.Commit();
            }
        }



        private OverrideGraphicSettings GetOverrideGraphicSettings() {

            OverrideGraphicSettings settings = new OverrideGraphicSettings();

            settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
            settings.SetSurfaceForegroundPatternColor(Color.UserColor);
            settings.SetSurfaceBackgroundPatternId(SolidFillPattern.Id);
            settings.SetSurfaceBackgroundPatternColor(Color.UserColor);


            //settings.SetProjectionLinePatternId(LineFillPattern.Id);
            settings.SetProjectionLineColor(Color.UserColor);
            //settings.SetCutLinePatternId(LineFillPattern.Id);
            settings.SetCutLineColor(Color.UserColor);


            settings.SetCutForegroundPatternId(SolidFillPattern.Id);
            settings.SetCutForegroundPatternColor(Color.UserColor);
            settings.SetCutBackgroundPatternId(SolidFillPattern.Id);
            settings.SetCutBackgroundPatternColor(Color.UserColor);

            return settings;
        }

    }
}