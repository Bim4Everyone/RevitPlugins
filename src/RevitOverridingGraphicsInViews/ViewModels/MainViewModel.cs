using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Windows.Input;


using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOverridingGraphicsInViews.Models;
using System.Windows;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Media;
using System.Drawing;

namespace RevitOverridingGraphicsInViews.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ColorHelper _selectedColor = new ColorHelper(0, 0, 0);
        private ColorHelper _color1 = new ColorHelper(15, 150, 190);
        private ColorHelper _color2 = new ColorHelper(250, 190, 20);
        private ColorHelper _color3 = new ColorHelper(240, 100, 70);
        private ColorHelper _color4 = new ColorHelper(150, 190, 50);
        private ColorHelper _color5 = new ColorHelper(230, 70, 90);


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            LoadConfig();

            ChangeColorManuallyCommand = RelayCommand.Create(ChangeColorManually);

            SelectСolor1Command = RelayCommand.Create(SelectСolor1);
            SelectСolor2Command = RelayCommand.Create(SelectСolor2);
            SelectСolor3Command = RelayCommand.Create(SelectСolor3);
            SelectСolor4Command = RelayCommand.Create(SelectСolor4);
            SelectСolor5Command = RelayCommand.Create(SelectСolor5);

            ChangeСolor1Command = RelayCommand.Create(ChangeСolor1);
            ChangeСolor2Command = RelayCommand.Create(ChangeСolor2);
            ChangeСolor3Command = RelayCommand.Create(ChangeСolor3);
            ChangeСolor4Command = RelayCommand.Create(ChangeСolor4);
            ChangeСolor5Command = RelayCommand.Create(ChangeСolor5);

            PaintCommand = RelayCommand.Create(Paint);
        }


        public ICommand PaintCommand { get; }
        public ICommand ChangeColorManuallyCommand { get; }

        public ICommand SelectСolor1Command { get; }
        public ICommand SelectСolor2Command { get; }
        public ICommand SelectСolor3Command { get; }
        public ICommand SelectСolor4Command { get; }
        public ICommand SelectСolor5Command { get; }

        public ICommand ChangeСolor1Command { get; }
        public ICommand ChangeСolor2Command { get; }
        public ICommand ChangeСolor3Command { get; }
        public ICommand ChangeСolor4Command { get; }
        public ICommand ChangeСolor5Command { get; }

        /// <summary>
        /// Штриховка сплошного закрашивания
        /// </summary>
        public FillPatternElement SolidFillPattern { get; set; }

        /// <summary>
        /// Цвет, который будет использован для перекраса
        /// </summary>
        public ColorHelper SelectedColor {
            get => _selectedColor;
            set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
        }

        /// <summary>
        /// Цвет N1 из палитры сохраненных
        /// </summary>
        public ColorHelper Color1 {
            get => _color1;
            set => this.RaiseAndSetIfChanged(ref _color1, value);
        }

        /// <summary>
        /// Цвет N2 из палитры сохраненных
        /// </summary>
        public ColorHelper Color2 {
            get => _color2;
            set => this.RaiseAndSetIfChanged(ref _color2, value);
        }

        /// <summary>
        /// Цвет N3 из палитры сохраненных
        /// </summary>
        public ColorHelper Color3 {
            get => _color3;
            set => this.RaiseAndSetIfChanged(ref _color3, value);
        }

        /// <summary>
        /// Цвет N4 из палитры сохраненных
        /// </summary>
        public ColorHelper Color4 {
            get => _color4;
            set => this.RaiseAndSetIfChanged(ref _color4, value);
        }

        /// <summary>
        /// Цвет N5 из палитры сохраненных
        /// </summary>
        public ColorHelper Color5 {
            get => _color5;
            set => this.RaiseAndSetIfChanged(ref _color5, value);
        }


        /// <summary>
        /// Метод команды по выбору цвета N1 из палитры сохраненных в качестве используемого для перекраса
        /// </summary>
        public void SelectСolor1() {

            SelectedColor = Color1;
        }
        /// <summary>
        /// Метод команды по выбору цвета N2 из палитры сохраненных в качестве используемого для перекраса
        /// </summary>
        public void SelectСolor2() {

            SelectedColor = Color2;
        }
        /// <summary>
        /// Метод команды по выбору цвета N3 из палитры сохраненных в качестве используемого для перекраса
        /// </summary>
        public void SelectСolor3() {

            SelectedColor = Color3;
        }
        /// <summary>
        /// Метод команды по выбору цвета N4 из палитры сохраненных в качестве используемого для перекраса
        /// </summary>
        public void SelectСolor4() {

            SelectedColor = Color4;
        }
        /// <summary>
        /// Метод команды по выбору цвета N5 из палитры сохраненных в качестве используемого для перекраса
        /// </summary>
        public void SelectСolor5() {

            SelectedColor = Color5;
        }
        /// <summary>
        /// Метод команды заданию цвета, используемого для перекраса
        /// </summary>
        private void ChangeColorManually() {

            SelectedColor = ChangeColor();
        }
        /// <summary>
        /// Метод команды по изменению цвета N1 в палитре сохраненных
        /// </summary>
        public void ChangeСolor1() {

            Color1 = ChangeColor();
            SelectСolor1();
        }
        /// <summary>
        /// Метод команды по изменению цвета N2 в палитре сохраненных
        /// </summary>
        public void ChangeСolor2() {

            Color2 = ChangeColor();
            SelectСolor2();
        }
        /// <summary>
        /// Метод команды по изменению цвета N3 в палитре сохраненных
        /// </summary>
        public void ChangeСolor3() {

            Color3 = ChangeColor();
            SelectСolor3();
        }
        /// <summary>
        /// Метод команды по изменению цвета N4 в палитре сохраненных
        /// </summary>
        public void ChangeСolor4() {

            Color4 = ChangeColor();
            SelectСolor4();
        }
        /// <summary>
        /// Метод команды по изменению цвета N5 в палитре сохраненных
        /// </summary>
        public void ChangeСolor5() {

            Color5 = ChangeColor();
            SelectСolor5();
        }





        /// <summary>
        /// Открывает окно выбора цвета и возвращает его в виде объекта ColorHelper
        /// </summary>
        private ColorHelper ChangeColor() {

            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            ColorHelper colorHelper = null;

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {

                colorHelper = new ColorHelper(
                    colorSelectionDialog.SelectedColor.Red,
                    colorSelectionDialog.SelectedColor.Green,
                    colorSelectionDialog.SelectedColor.Blue);
            }

            return colorHelper;
        }


        /// <summary>
        /// Выполняет прямое переопределение видимости графики элементов, выбранных до запуска плагина на всех видах, где видно эти элементы
        /// </summary>
        private void Paint() {

            List<Element> elemsForWork = new List<Element>();
            ICollection<ElementId> selectedIds = _revitRepository.ActiveUIDocument.Selection.GetElementIds();

            foreach(ElementId id in selectedIds) {

                elemsForWork.Add(_revitRepository.Document.GetElement(id));
            }

            SolidFillPattern = FillPatternElement.GetFillPatternElementByName(_revitRepository.Document, FillPatternTarget.Drafting, "<Сплошная заливка>");
            OverrideGraphicSettings settings = GetOverrideGraphicSettings();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор пилонов")) {

                foreach(View view in _revitRepository.AllViews) {

                    foreach(Element el in elemsForWork) {

                        try { 
                            view.SetElementOverrides(el.Id, settings);
                        } catch(Exception) {}
                    }
                }

                transaction.Commit();
            }

            _revitRepository.ActiveUIDocument.Selection.SetElementIds(new List<ElementId>());

            SaveConfig();
            ApplicationCommands.Close.Execute(null, null);
        }


        private OverrideGraphicSettings GetOverrideGraphicSettings() {

            OverrideGraphicSettings settings = new OverrideGraphicSettings();

            settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
            settings.SetSurfaceForegroundPatternColor(SelectedColor.UserColor);
            settings.SetSurfaceBackgroundPatternId(SolidFillPattern.Id);
            settings.SetSurfaceBackgroundPatternColor(SelectedColor.UserColor);


            //settings.SetProjectionLineColor(SelectedColor.UserColor);
            //settings.SetCutLineColor(SelectedColor.UserColor);


            settings.SetCutForegroundPatternId(SolidFillPattern.Id);
            settings.SetCutForegroundPatternColor(SelectedColor.UserColor);
            settings.SetCutBackgroundPatternId(SolidFillPattern.Id);
            settings.SetCutBackgroundPatternColor(SelectedColor.UserColor);

            return settings;
        }


        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting is null) { return; }

            SelectedColor = setting.SelectedColor;
            Color1 = setting.Color1;
            Color2 = setting.Color2;
            Color3 = setting.Color3;
            Color4 = setting.Color4;
            Color5 = setting.Color5;
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting is null) {
                setting = _pluginConfig.AddSettings(_revitRepository.Document);
            }

            setting.SelectedColor = SelectedColor;
            setting.Color1 = Color1;
            setting.Color2 = Color2;
            setting.Color3 = Color3;
            setting.Color4 = Color4;
            setting.Color5 = Color5;

            _pluginConfig.SaveProjectConfig();
        }
    }
}