using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class VisElementsCalculator {

        private readonly SpecConfiguration _specConfiguration;
        private readonly UnitConverter _unitConverter;
        private readonly Document _document;

        public VisElementsCalculator(SpecConfiguration config, Document document) {
            _specConfiguration = config;
            _unitConverter = new UnitConverter();
            _document = document;
        }

        public string GetDuctName(Element element) {
            return ", с толщиной стенки " +
                    GetDuctThikness(element) +
                    " мм, " +
                    element.GetParamValue(BuiltInParameter.RBS_CALCULATED_SIZE);
        }

        //Формируем диаметр трубы для наименования в зависимости от того что включено у нее в типе
        public string GetPipeSize(Element element) {
            Element elemType = element.GetElementType();

            bool dy = elemType.GetSharedParamValueOrDefault<int>("ФОП_ВИС_Ду") == 1;
            bool dyWall = elemType.GetSharedParamValueOrDefault<int>("ФОП_ВИС_Ду х Стенка") == 1;
            bool dExternalWall = elemType.GetSharedParamValueOrDefault<int>("ФОП_ВИС_Днар х Стенка") == 1;


            double externalSize = _unitConverter.DoubleToMilimeters(element.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER));
            double internalSize = _unitConverter.DoubleToMilimeters(element.GetParamValue<double>(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM));

            string pipeThickness = ((externalSize - internalSize) / 2).ToString();
            string pipeDiameter = _unitConverter.DoubleToMilimeters(element.GetParamValue<double>(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)).ToString();

            if(dy) { return " ⌀" + pipeDiameter; }
            if(dyWall) { return " ⌀" + pipeDiameter + "x" + pipeThickness; }
            if(dExternalWall) { return " ⌀" + externalSize.ToString() + "x" + pipeThickness; }

            return " НЕ ЗАДАН ТИП ДИАМЕТРА";
        }

        //Проверяем по галочкам в проекте и типе нужно ли специфицировать фиттинг трубы
        public bool IsSpecifyPipeFittingName(Element element) {
            List<Connector> connectors = GetConnectors(element);

            if(_specConfiguration.IsSpecifyPipeFittings) {
                foreach(Connector connector in connectors) {
                    foreach(Connector reference in connector.AllRefs) {

                        if(reference.Owner.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                            Element elemType = reference.Owner.GetElementType();
                            return elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.ParamNameIsSpecifyPipeFittingsFromPype) == 1;
                        }
                    }
                }
            }

            return _specConfiguration.IsSpecifyPipeFittings;
        }

        //Базовая толщина воздуховодов по ГОСТ
        private double GetBaseDuctThikness(Element element) {
            var curve = element as MEPCurve;
            Connector connector = GetConnectors(element).FirstOrDefault();
            if(connector.Shape == ConnectorProfileType.Rectangular || connector.Shape == ConnectorProfileType.Oval) {
                double sizeA = curve.Width;
                double sizeB = curve.Height;
                double size = Math.Max(sizeA, sizeB);
                //ссылка откуда цифры
                if(size < 251) { return 0.5; }
                if(size < 1001) { return 0.7; }
                if(size < 2001) { return 0.9; }
                if(size > 2001) { return 1.4; }
            }
            if(connector.Shape == ConnectorProfileType.Round) {
                double size = curve.Diameter;
                if(size < 201) { return 0.5; }
                if(size < 451) { return 0.6; }
                if(size < 801) { return 0.7; }
                if(size < 1251) { return 1.0; }
                if(size < 1601) { return 1.2; }
                if(size > 1601) { return 1.4; }
            }
            return 0;
        }

        //Толщина воздуховодов с учетом ограничителей в изоляции и в типе воздуховода
        public string GetDuctThikness(Element element) {
            var filter = new ElementCategoryFilter(BuiltInCategory.OST_DuctInsulations);
            Element elemType = element.GetElementType();


            double minDuctThikness = elemType.GetSharedParamValueOrDefault<double>(_specConfiguration.MinDuctThikness, 0);
            double maxDuctThikness = elemType.GetSharedParamValueOrDefault<double>(_specConfiguration.MaxDuctThikness, 0);
            double minInsulThikness = 0;
            double maxInsulThikness = 0;



            //Возвращается лист элементID, но тяжело представить ситуацию, где у нас больше изоляции на воздуховоде, чем 1 штука
            var dependent = element.GetDependentElements(filter).ToList();
            ElementId insulationId = dependent.FirstOrDefault();


            if(insulationId.IsNotNull()) {
                Element insulation = _document.GetElement(insulationId);
                Element insulType = insulation.GetElementType();
                minInsulThikness = insulType.GetSharedParamValueOrDefault<double>(_specConfiguration.MinDuctThikness, 0);
                maxInsulThikness = insulType.GetSharedParamValueOrDefault<double>(_specConfiguration.MaxDuctThikness, 0);
            }

            double thikness = GetBaseDuctThikness(element);
            double upCriteria = Math.Max(maxInsulThikness, maxDuctThikness);
            double minCriteria = Math.Max(minInsulThikness, minDuctThikness);


            //currentculture передаем
            if(thikness > upCriteria && upCriteria != 0) { return Math.Max(maxInsulThikness, maxDuctThikness).ToString(); }
            if(thikness < minCriteria && minCriteria != 0) { return Math.Max(minInsulThikness, minDuctThikness).ToString(); }

            return thikness.ToString();
        }

        //поиск воздуховода из коннекторов фитинга
        private Element GetDuctFromFitting(List<Connector> connectors) {
            var subConnectors = new List<Connector>();
            foreach(Connector connector in connectors) {
                foreach(Connector reference in connector.AllRefs) {
                    if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctCurves)) { return reference.Owner; }
                    if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                        subConnectors.AddRange(GetConnectors(reference.Owner));
                    }
                }

                //Мы ищем воздуховод сначала среди коннекторов фитинга, а потом, если не находим, среди коннекторов возможных фитингов вокруг
                //если там какая-то дикая конструкция из 4 соединенных фитингов - лучше вернуть нулл, а не искать дальше, таких мест вряд ли будет много
                foreach(Connector subConnector in subConnectors) {
                    foreach(Connector reference in subConnector.AllRefs) {
                        if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctCurves)) { return reference.Owner; }
                    }

                }
            }
            return null;
        }

        //получение толщины фитинга воздуховода
        public string GetDuctFittingThikness(Element element) {
            List<Connector> connectors = GetConnectors(element);
            Element duct = GetDuctFromFitting(connectors);
            if(duct is null) { return null; }

            return GetDuctThikness(duct);
        }
        //получение угла фитинга воздуховода
        private string GetDuctFittingAngle(Element element) {
            double angle = _unitConverter.DoubleToDegree(GetConnectors(element).First().Angle);
            if(angle <= 15.1) { return "15"; }
            if(angle <= 30.1) { return "30"; }
            if(angle <= 45.1) { return "45"; }
            if(angle <= 60.1) { return "60"; }
            if(angle <= 75.1) { return "75"; }
            if(angle <= 90.1) { return "90"; }
            return "0";
        }

        //получение имени фитинга воздуховода
        public string GetDuctFittingName(Element element) {
            string thikness = GetDuctFittingThikness(element);
            if(thikness is null) {
                return "!Не учитывать";
            }

            if(!_specConfiguration.IsSpecifyDuctFittings) { return "Металл для фасонных деталей воздуховодов с толщиной стенки " + thikness + " мм"; }

            string startName = "Не удалось определить тип фитинга";
            var instanse = element as FamilyInstance;
            var fitting = instanse.MEPModel as MechanicalFitting;


            if(fitting.PartType is PartType.Transition) { startName = "Переход между сечениями воздуховода "; }
            if(fitting.PartType is PartType.Tee) { startName = "Тройник "; }
            if(fitting.PartType is PartType.TapAdjustable) { startName = "Врезка в воздуховод "; }
            if(fitting.PartType is PartType.Cross) { startName = "Крестовина "; }
            if(fitting.PartType is PartType.Union) { return "!Не учитывать"; }

            if(fitting.PartType == PartType.Elbow) {
                Connector connector = GetConnectors(element).First();
                string angle = GetDuctFittingAngle(element);
                if(connector.Shape == ConnectorProfileType.Round) { startName = "Отвод " + angle + "° круглого сечения"; }
                if(connector.Shape == ConnectorProfileType.Rectangular) { startName = "Отвод " + angle + "° прямоугольного сечения"; }
            }

            string size = element.GetParamValue<string>(BuiltInParameter.RBS_CALCULATED_SIZE);
            //Ревит пишет размеры всех коннекторов. Для всего кроме тройника и перехода нам хватит первого размера
            if(!(fitting.PartType is PartType.Transition) || !(fitting.PartType is PartType.Tee)) { size = size.Split('-').First(); }



            return startName + " " + size + ", с толщиной стенки " + thikness + " мм";
        }

        //получение коннекторов элемента
        public List<Connector> GetConnectors(Element element) {
            var connectors = new List<Connector>();

            if(element is FamilyInstance) {
                var instance = element as FamilyInstance;
                if(instance.MEPModel.ConnectorManager is null) { return connectors; }
                ConnectorSetIterator set = instance.MEPModel.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }

            if(element.InAnyCategory(new List<BuiltInCategory>() { BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_PipeCurves })) {
                var curve = element as MEPCurve;
                if(curve.ConnectorManager is null) { return connectors; }
                ConnectorSetIterator set = curve.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }

            return connectors;
        }

        //получение площади фитинга воздуховода
        public double GetFittingArea(Element element) {

            double area = 0;

            foreach(Solid solid in element.GetSolids())
                foreach(Face face in solid.Faces)
                    area += face.Area;

            area = _unitConverter.DoubleToSquareMeters(area);

            if(area > 0) {
                double falseArea = 0;
                List<Connector> connectors = GetConnectors(element);
                foreach(Connector connector in connectors) {
                    if(connector.Shape == ConnectorProfileType.Rectangular) { falseArea += _unitConverter.DoubleToSquareMeters(connector.Height * connector.Width); }
                    if(connector.Shape == ConnectorProfileType.Round) { falseArea += _unitConverter.DoubleToSquareMeters(connector.Radius * connector.Radius * 3.14); }
                }
                //Вычетаем площадь пустоты на местах коннекторов
                area -= falseArea;
            }
            return area;
        }

    }
}
