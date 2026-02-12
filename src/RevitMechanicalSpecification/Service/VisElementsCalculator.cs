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

        private readonly Document _document;

        public VisElementsCalculator(SpecConfiguration config, Document document) {
            _specConfiguration = config;
            _document = document;
        }

        /// <summary>
        /// Получение имени воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public string GetDuctName(Element element, Element elemType) {
            return ", с толщиной стенки " +
                    GetDuctThikness(element, elemType) +
                    " мм, " +
                    element.GetParamValue(BuiltInParameter.RBS_CALCULATED_SIZE);
        }

        /// <summary>
        /// Получение марки фитинга воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetDuctFittingMark(Element element) {
            List<Connector> connectors = GetConnectors(element);
            Element duct = GetDuctFromConnectorList(connectors);
            if(duct is null) {
                return "!Не учитывать";
            }
            return duct.GetTypeOrInstanceParamStringValue(duct.GetElementType(), _specConfiguration.OriginalParamNameMark);
        }

        /// <summary>
        /// Получение имени фитинга воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetDuctFittingName(Element element) {
            string thikness = GetDuctFittingThikness(element);
            if(thikness is null) {
                return "!Не учитывать";
            }

            if(!_specConfiguration.IsSpecifyDuctFittings) {
                return "Металл для фасонных деталей воздуховодов с толщиной стенки " + thikness + " мм";
            }

            string startName = "Не удалось определить тип фитинга";
            FamilyInstance instanse = element as FamilyInstance;
            MechanicalFitting fitting = instanse.MEPModel as MechanicalFitting;

            switch(fitting.PartType) {
                case PartType.Transition:
                    startName = "Переход между сечениями воздуховода";
                    break;
                case PartType.Tee:
                    startName = "Тройник";
                    break;
                case PartType.TapAdjustable:
                    startName = "Врезка в воздуховод";
                    break;
                case PartType.Cross:
                    startName = "Крестовина";
                    break;
                case PartType.Cap:
                    startName = "Заглушка";
                    break;
                case PartType.Union:
                    return "!Не учитывать";
                case PartType.Elbow:
                    Connector connector = GetConnectors(element).First();
                    string angle = GetDuctFittingAngle(element);
                    if(connector.Shape == ConnectorProfileType.Round) {
                        startName = "Отвод " + angle + "° круглого сечения";
                    }
                    if(connector.Shape == ConnectorProfileType.Rectangular) {
                        startName = "Отвод " + angle + "° прямоугольного сечения";
                    }
                    break;
                default:
                    return startName;
            }

            string size = element.GetParamValue<string>(BuiltInParameter.RBS_CALCULATED_SIZE);
            //Ревит пишет размеры всех коннекторов. Для всего кроме тройника и перехода нам хватит первого размера

            bool notTransition = !(fitting.PartType is PartType.Transition);
            bool notTee = !(fitting.PartType is PartType.Tee); 

            if(notTransition && notTee) {
                size = size.Split('-').First();
            }
            return startName + " " + size + ", с толщиной стенки " + thikness + " мм";
        }

        /// <summary>
        /// Получение площади фитинга воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public double GetFittingArea(Element element) {
            double area = 0;

            foreach(Solid solid in element.GetSolids())
                foreach(Face face in solid.Faces)
                    area += face.Area;

            area = UnitConverter.DoubleToSquareMeters(area);

            if(area > 0) {
                double falseArea = 0;
                List<Connector> connectors = GetConnectors(element);
                foreach(Connector connector in connectors) {
                    if(connector.Shape == ConnectorProfileType.Rectangular) {
                        falseArea += UnitConverter.DoubleToSquareMeters(connector.Height * connector.Width);
                    }
                    if(connector.Shape == ConnectorProfileType.Round) {
                        falseArea += UnitConverter.DoubleToSquareMeters(connector.Radius * connector.Radius * 3.14);
                    }
                }
                //Вычетаем площадь пустоты на местах коннекторов
                area -= falseArea;
            }
            return area;
        }

        /// <summary>
        /// Формируем диаметр трубы для наименования в зависимости от того что включено у нее в типе
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public string GetPipeSize(Element element, Element elemType) {
            bool dy = elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.Dy) == 1;
            bool dyWall = elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.DyWall) == 1;
            bool dExternalWall = elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.DExternalWall) == 1;

            double externalSize = UnitConverter.DoubleToMilimeters(
                element.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER)
                );
            
            double internalSize = UnitConverter.DoubleToMilimeters(
                element.GetParamValue<double>(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM)
                );

            string pipeThickness = UnitConverter.DoubleToString(((
                Math.Round(externalSize - internalSize, 2) / 2)));
            string pipeDiameter = UnitConverter.DoubleToString(
                    UnitConverter.DoubleToMilimeters(
                    element.GetParamValue<double>(
                    BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)));

            if(dy) {
                return " ⌀" + pipeDiameter;
            }
            if(dyWall) {
                return " ⌀" + pipeDiameter + "x" + pipeThickness;
            }
            if(dExternalWall) {
                return " ⌀" + UnitConverter.DoubleToString(externalSize) + "x" + pipeThickness;
            }

            return " НЕ ЗАДАН ТИП ДИАМЕТРА";
        }

        /// <summary>
        /// Проверяем по галочкам в проекте и типе нужно ли специфицировать фиттинг трубы
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsSpecifyPipeFittingName(Element element) {
            List<Connector> connectors = GetConnectors(element);

            // Если нет учета на весь проект - учет не ведется
            if(_specConfiguration.IsSpecifyPipeFittings is false) {
                return false;
            }
            
            // Если есть учет на весь проект - по умолчанию true
            bool specifiPipeFitting = true;
            if(_specConfiguration.IsSpecifyPipeFittings) {
                foreach(Connector connector in connectors) {
                    foreach(Connector reference in connector.AllRefs) {
                        // Проверяем каждую трубу подключенную к фитингу
                        if(reference.Owner.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                            // Если подключена хоть одна труба - требуется проверять учет по трубе, по умолчанию false
                            specifiPipeFitting = false;
                            Element elemType = reference.Owner.GetElementType();
                            specifiPipeFitting = elemType.GetSharedParamValueOrDefault<int>
                                (_specConfiguration.ParamNameIsSpecifyPipeFittingsFromPype) == 1;

                            if(specifiPipeFitting) {
                                return true;
                            }
                        }
                    }
                }
            }
            
            // Возвращаем результаты проверки после прохода по всем коннекторам. Если были встречены трубы и на них не включены расчеты - 
            // возвращается false. Если их не было или на трубах включен- true
            return specifiPipeFitting;
        }

        /// <summary>
        /// Базовая толщина воздуховодов по ГОСТ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private double GetBaseDuctThikness(Element element) {
            var curve = element as MEPCurve;
            Connector connector = GetConnectors(element).FirstOrDefault();
            if(connector.Shape == ConnectorProfileType.Rectangular || connector.Shape == ConnectorProfileType.Oval) {
                double sizeA = curve.Width;
                double sizeB = curve.Height;
                double size = UnitConverter.DoubleToMilimeters(Math.Max(sizeA, sizeB));
                // Толщины по СП 60.13330.2020 Отопление, вентиляция и кондиционирование воздуха. Приложение К
                if(size < 251) {
                    return 0.5;
                }
                if(size < 1001) {
                    return 0.7;
                }
                if(size < 2001) {
                    return 0.9;
                }
                if(size > 2001) {
                    return 1.4;
                }
            }
            if(connector.Shape == ConnectorProfileType.Round) {
                double size = UnitConverter.DoubleToMilimeters(curve.Diameter);
                if(size < 201) {
                    return 0.5;
                }
                if(size < 451) {
                    return 0.6;
                }
                if(size < 801) {
                    return 0.7;
                }
                if(size < 1251) {
                    return 1.0;
                }
                if(size < 1601) {
                    return 1.2;
                }
                if(size > 1601) {
                    return 1.4;
                }
            }
            return 0;
        }

        /// <summary>
        /// Толщина воздуховодов с учетом ограничителей в изоляции и в типе воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        private string GetDuctThikness(Element element, Element elemType) {
            var filter = new ElementCategoryFilter(BuiltInCategory.OST_DuctInsulations);

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
            if(thikness > upCriteria && upCriteria != 0) {
                return 
                    UnitConverter.DoubleToString(Math.Max(maxInsulThikness, maxDuctThikness));
            }
            if(thikness < minCriteria && minCriteria != 0) {
                return
                    UnitConverter.DoubleToString(Math.Max(minInsulThikness, minDuctThikness));
            }

            return UnitConverter.DoubleToString(thikness);
        }

        /// <summary>
        /// Поиск воздуховода из коннекторов фитинга
        /// </summary>
        /// <param name="connectors"></param>
        /// <returns></returns>
        private Element GetDuctFromConnectorList(List<Connector> connectors) {
            foreach(Connector connector in connectors) {
                Element duct = GetDuctFromConnector(connector);
                if(duct != null) {
                    return duct;
                }
            }

            return null;
        }

        private Element GetDuctFromConnector(Connector connector) {
            var subConnectors = new List<Connector>();

            foreach(Connector reference in connector.AllRefs) {
                if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                    return reference.Owner;
                }
                if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                    subConnectors.AddRange(GetConnectors(reference.Owner));
                }
            }

            //Мы ищем воздуховод сначала среди коннекторов фитинга, а потом, если не находим, среди коннекторов возможных фитингов вокруг
            //если там какая-то дикая конструкция из 4 соединенных фитингов - лучше вернуть нулл, а не искать дальше, таких мест вряд ли будет много
            foreach(Connector subConnector in subConnectors) {
                foreach(Connector reference in subConnector.AllRefs) {
                    if(reference.Owner.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                        return reference.Owner;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// получение толщины фитинга воздуховода. Возвращает максимальное значение из подключенных воздуховодов
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetDuctFittingThikness(Element element) {
            List<Connector> connectors = GetConnectors(element);
            double maxValue = double.MinValue;
            bool hasValue = false;

            foreach(Connector connector in connectors) {
                Element duct = GetDuctFromConnector(connector);
                if(duct == null)
                    continue;

                string valueStr = GetDuctThikness(duct, duct.GetElementType());

                if(double.TryParse(valueStr, out double value)) {
                    if(value > maxValue) {
                        maxValue = value;
                        hasValue = true;
                    }
                }
            }

            if(!hasValue)
                return null;

            return maxValue.ToString();
        }

        /// <summary>
        /// Получение угла фитинга воздуховода
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetDuctFittingAngle(Element element) {
            double angle = UnitConverter.DoubleToDegree(GetConnectors(element).First().Angle);
            if(angle <= 15.1) {
                return "15";
            }
            if(angle <= 30.1) {
                return "30";
            }
            if(angle <= 45.1) {
                return "45";
            }
            if(angle <= 60.1) {
                return "60";
            }
            if(angle <= 75.1) {
                return "75";
            }
            if(angle <= 90.1) {
                return "90";
            }
            return "0";
        }

        /// <summary>
        /// Получение коннекторов элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private List<Connector> GetConnectors(Element element) {
            List<Connector> connectors = new List<Connector>();

            if(element is FamilyInstance instance && instance.MEPModel.ConnectorManager != null) {
                AddConnectors(connectors, instance.MEPModel.ConnectorManager.Connectors);
            }
            if(element.InAnyCategory
                (new List<BuiltInCategory> { BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_PipeCurves })
                       && (element as MEPCurve)?.ConnectorManager != null) {
                AddConnectors(connectors, (element as MEPCurve).ConnectorManager.Connectors);
            }
            return connectors;
        }

        /// <summary>
        /// Наполняет лист коннекторов из сета коннекторов
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="connectorSet"></param>
        private void AddConnectors(List<Connector> connectors, ConnectorSet connectorSet) {
            ConnectorSetIterator set = connectorSet.ForwardIterator();
            while(set.MoveNext()) {
                connectors.Add(set.Current as Connector);
            }
        }
    }
}
