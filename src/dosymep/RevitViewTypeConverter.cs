using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using Autodesk.Revit.DB;

namespace dosymep.WPF.Converters {
    [ValueConversion(typeof(ViewType), typeof(string))]
    internal class RevitViewTypeConverter : MarkupExtension, IValueConverter {
        private const string _areaPlan = "План зонирования";
        private const string _ceilingPlan = "План потолка";
        private const string _columnSchedule = "Графическая спецификация колонн";
        private const string _costReport = "Отчет о стоимости";
        private const string _detail = "Вид узла";
        private const string _draftingView = "Чертежный вид";
        private const string _drawingSheet = "Лист";
        private const string _elevation = "Фасад";
        private const string _engineeringPlan = "План несущих конструкций";
        private const string _floorPlan = "План этажа";
        private const string _internal = "Внутренний вид Revit";
        private const string _legend = "Легенда";
        private const string _loadsReport = "Отчет о нагрузках";
        private const string _panelSchedule = "Принципиальная схема щита/панели";
        private const string _presureLossReport = "Отчет о потерях давления";
        private const string _projectBrowser = "Диспетчер проекта";
        private const string _rendering = "Визуализация";
        private const string _report = "Отчет";
        private const string _schedule = "Спецификация";
        private const string _section = "Разрез";
        private const string _systemBrowser = "Диспетчер инженерных систем";
        private const string _systemsAnalysisReport = "Отчет по результатам анализа";
        private const string _threeD = "3D вид";
        private const string _undefined = "Не определен";
        private const string _walkthrough = "Обход";
        private const string _stringEmpty = "";


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ViewType viewType = (ViewType?) value ?? ViewType.Undefined;
            switch(viewType) {
                case ViewType.Undefined:
                    return _undefined;
                case ViewType.FloorPlan:
                    return _floorPlan;
                case ViewType.EngineeringPlan:
                    return _engineeringPlan;
                case ViewType.AreaPlan:
                    return _areaPlan;
                case ViewType.CeilingPlan:
                    return _ceilingPlan;
                case ViewType.Elevation:
                    return _elevation;
                case ViewType.Section:
                    return _section;
                case ViewType.Detail:
                    return _detail;
                case ViewType.ThreeD:
                    return _threeD;
                case ViewType.Schedule:
                    return _schedule;
                case ViewType.DraftingView:
                    return _draftingView;
                case ViewType.DrawingSheet:
                    return _drawingSheet;
                case ViewType.Legend:
                    return _legend;
                case ViewType.Report:
                    return _report;
                case ViewType.ProjectBrowser:
                    return _projectBrowser;
                case ViewType.SystemBrowser:
                    return _systemBrowser;
                case ViewType.CostReport:
                    return _costReport;
                case ViewType.LoadsReport:
                    return _loadsReport;
                case ViewType.PresureLossReport:
                    return _presureLossReport;
                case ViewType.PanelSchedule:
                    return _panelSchedule;
                case ViewType.ColumnSchedule:
                    return _columnSchedule;
                case ViewType.Walkthrough:
                    return _walkthrough;
                case ViewType.Rendering:
                    return _rendering;
                case ViewType.SystemsAnalysisReport:
                    return _systemsAnalysisReport;
                case ViewType.Internal:
                    return _internal;
                default:
                    throw new InvalidCastException($"\'{viewType}\' не поддерживается");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string viewType = (string) value;
            switch(viewType) {
                case _areaPlan:
                    return ViewType.AreaPlan;
                case _ceilingPlan:
                    return ViewType.CeilingPlan;
                case _columnSchedule:
                    return ViewType.ColumnSchedule;
                case _costReport:
                    return ViewType.CostReport;
                case _detail:
                    return ViewType.Detail;
                case _draftingView:
                    return ViewType.DraftingView;
                case _drawingSheet:
                    return ViewType.DrawingSheet;
                case _elevation:
                    return ViewType.Elevation;
                case _engineeringPlan:
                    return ViewType.EngineeringPlan;
                case _floorPlan:
                    return ViewType.FloorPlan;
                case _internal:
                    return ViewType.Internal;
                case _legend:
                    return ViewType.Legend;
                case _loadsReport:
                    return ViewType.LoadsReport;
                case _panelSchedule:
                    return ViewType.PanelSchedule;
                case _presureLossReport:
                    return ViewType.PresureLossReport;
                case _projectBrowser:
                    return ViewType.ProjectBrowser;
                case _rendering:
                    return ViewType.Rendering;
                case _report:
                    return ViewType.Report;
                case _schedule:
                    return ViewType.Schedule;
                case _section:
                    return ViewType.Section;
                case _systemBrowser:
                    return ViewType.SystemBrowser;
                case _systemsAnalysisReport:
                    return ViewType.SystemsAnalysisReport;
                case _threeD:
                    return ViewType.ThreeD;
                case _undefined:
                case _stringEmpty:
                    return ViewType.Undefined;
                case _walkthrough:
                    return ViewType.Walkthrough;
                default:
                    throw new InvalidCastException($"Не удалось преобразовать строку \'{viewType}\' в ViewType");
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}
