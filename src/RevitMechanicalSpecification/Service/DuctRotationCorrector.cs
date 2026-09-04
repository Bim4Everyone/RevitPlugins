using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Service {
    internal class DuctRotationCorrector {
        private const double GeometryTolerance = 0.0001;
        private const double ConnectorOriginTolerance = 0.001;

        private static readonly ElementId _widthParameterId =
            new ElementId(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
        private static readonly ElementId _heightParameterId =
            new ElementId(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
        private static readonly ElementId _offsetParameterId =
            new ElementId(BuiltInParameter.RBS_OFFSET_PARAM);

        private readonly Document _document;
        private readonly IElementEditorTracker _elementEditorTracker;
        private readonly IMessageBoxService _messageBoxService;
        private bool _hasFailedDuctRecreations;
        private bool _hasDuctsWithoutSystem;
        private ElementId _firstFailedDuctId;

        public DuctRotationCorrector(
            Document document,
            IElementEditorTracker elementEditorTracker,
            IMessageBoxService messageBoxService) {
            _document = document;
            _elementEditorTracker = elementEditorTracker;
            _messageBoxService = messageBoxService;
        }

        public void Execute(ElementSplitResult splitResult) {
            if(splitResult == null) {
                throw new ArgumentNullException(nameof(splitResult));
            }

            _hasFailedDuctRecreations = false;
            _hasDuctsWithoutSystem = false;
            _firstFailedDuctId = null;

            // Шаг 1. Берем одиночные элементы, среди которых могут находиться воздуховоды.
            List<SpecificationElement> specificationElements = splitResult.SingleElements;

            // Шаг 2. Сохраняем связи со старыми ID до удаления воздуховодов и изоляции.
            ILookup<ElementId, SpecificationElement> elementsById = specificationElements
                .Where(item => item.Element != null)
                .ToLookup(item => item.Element.Id);

            ILookup<ElementId, SpecificationElement> elementsByInsulationHostId = specificationElements
                .Where(item => item.InsulationSpHost?.Element != null)
                .ToLookup(item => item.InsulationSpHost.Element.Id);

            // Шаг 3. Выбираем уникальные воздуховоды из одиночных элементов спецификации.
            List<SpecificationElement> ducts = specificationElements
                .Where(item => item.Element?.Category != null
                               && item.Element.Category.IsId(BuiltInCategory.OST_DuctCurves))
                .GroupBy(item => item.Element.Id)
                .Select(group => group.First())
                .ToList();

            foreach(SpecificationElement specificationElement in ducts) {
                ElementId ductId = specificationElement.Element.Id;
                try {
                    // Шаг 4. Проверяем горизонтальность и поворот, затем при необходимости пересоздаем воздуховод.
                    var duct = (Duct) specificationElement.Element;
                    DuctReplacement replacement = ReplaceIfRequired(duct);
                    if(replacement == null) {
                        continue;
                    }

                    // Шаг 5. Заменяем ссылки на старый воздуховод и на хост изоляции новым элементом.
                    foreach(SpecificationElement item in elementsById[replacement.OldDuctId]) {
                        UpdateSpecificationElement(item, replacement.NewDuct);
                    }

                    foreach(SpecificationElement item in elementsByInsulationHostId[replacement.OldDuctId]) {
                        UpdateSpecificationElement(item.InsulationSpHost, replacement.NewDuct);
                    }

                    // Шаг 6. Заменяем ссылки на пересозданную изоляцию.
                    foreach(ElementReplacement insulation in replacement.Insulations) {
                        foreach(SpecificationElement item in elementsById[insulation.OldElementId]) {
                            UpdateSpecificationElement(item, insulation.NewElement);
                        }
                    }
                } catch(Exception) {
                    RegisterFailedDuct(ductId);
                }
            }

        }

        public void ShowReport() {
            if(!_hasFailedDuctRecreations && !_hasDuctsWithoutSystem) {
                return;
            }

            var reports = new List<string>();
            if(_hasFailedDuctRecreations) {
                reports.Add("В модели существуют повернутые воздуховоды, которые не удалось пересоздать.");
            }

            if(_hasDuctsWithoutSystem) {
                reports.Add("В модели существуют повернутые воздуховоды без системы.");
            }

            if(_firstFailedDuctId != null) {
                reports.Add($"ID воздуховода с ошибкой: {_firstFailedDuctId.GetIdValue()}.");
            }

            _hasFailedDuctRecreations = false;
            _hasDuctsWithoutSystem = false;
            _firstFailedDuctId = null;

            _messageBoxService.Show(
                string.Join(Environment.NewLine, reports),
                "Обновление спецификации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private DuctReplacement ReplaceIfRequired(Duct duct) {
            if(!(duct.Location is LocationCurve locationCurve)
               || !(locationCurve.Curve is Line line)
               || !IsHorizontal(line)) {
                return null;
            }

            List<Connector> endConnectors = GetEndConnectors(duct).ToList();
            Connector orientationConnector = endConnectors.FirstOrDefault();
            if(orientationConnector == null
               || orientationConnector.Shape != ConnectorProfileType.Rectangular
               || !IsRotatedByQuarterTurn(orientationConnector)) {
                return null;
            }

            DuctData ductData = CaptureDuctData(duct, line, endConnectors);
            if(ductData == null) {
                return null;
            }

            if(!CanEditRelatedElements(ductData)) {
                return null;
            }

            DuctReplacement replacement = RecreateDuct(ductData);
            if(replacement == null) {
                RegisterFailedDuct(duct.Id);
            }

            return replacement;
        }

        // Та же проверка горизонтальности, которая используется в RevitOpeningPlacement.
        private static bool IsHorizontal(Line line) {
            return Math.Abs(line.GetEndPoint(0).Z - line.GetEndPoint(1).Z) < GeometryTolerance;
        }

        private static bool IsRotatedByQuarterTurn(Connector connector) {
            // BasisX направлен вдоль ширины профиля и становится вертикальным после поворота на 90 или 270 градусов.
            XYZ widthDirection = connector.CoordinateSystem.BasisX.Normalize();
            return Math.Abs(Math.Abs(widthDirection.Z) - 1) < GeometryTolerance;
        }

        private DuctData CaptureDuctData(
            Duct duct,
            Line line,
            IReadOnlyCollection<Connector> endConnectors) {
            Parameter widthParameter = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            Parameter heightParameter = duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
            Parameter offsetParameter = duct.GetParam(BuiltInParameter.RBS_OFFSET_PARAM);
            if(widthParameter == null || heightParameter == null || offsetParameter == null) {
                RegisterFailedDuct(duct.Id);
                return null;
            }

            var ductData = new DuctData {
                Duct = duct,
                OldDuctId = duct.Id,
                DuctTypeId = duct.GetTypeId(),
                SystemTypeId = GetSystemTypeId(duct),
                LevelId = GetLevelId(duct),
                StartPoint = line.GetEndPoint(0),
                EndPoint = line.GetEndPoint(1),
                Width = widthParameter.AsDouble(),
                Height = heightParameter.AsDouble(),
                Offset = offsetParameter.AsDouble(),
                IsPinned = duct.Pinned,
                Parameters = CaptureParameters(duct),
                Ends = endConnectors.Select(CaptureDuctEnd).ToList(),
                Insulations = CaptureInsulations(duct).ToList()
            };

            if(IsInvalidId(ductData.SystemTypeId) || IsInvalidId(ductData.LevelId)) {
                if(IsInvalidId(ductData.SystemTypeId)) {
                    _hasDuctsWithoutSystem = true;
                }

                if(IsInvalidId(ductData.LevelId)) {
                    RegisterFailedDuct(duct.Id);
                }

                return null;
            }

            return ductData;
        }

        private void RegisterFailedDuct(ElementId ductId) {
            _hasFailedDuctRecreations = true;
            if(_firstFailedDuctId == null) {
                _firstFailedDuctId = ductId;
            }
        }

        private static ElementId GetSystemTypeId(Duct duct) {
            ElementId systemTypeId = duct
                .get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
                ?.AsElementId();

            if(IsInvalidId(systemTypeId) && duct.MEPSystem != null) {
                systemTypeId = duct.MEPSystem.GetTypeId();
            }

            return systemTypeId;
        }

        private static ElementId GetLevelId(Duct duct) {
            return duct.ReferenceLevel?.Id
                   ?? duct.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM)?.AsElementId();
        }

        private DuctEnd CaptureDuctEnd(Connector ductConnector) {
            var ductEnd = new DuctEnd {
                DuctConnector = ductConnector,
                Origin = ductConnector.Origin
            };

            foreach(Connector reference in ductConnector.AllRefs) {
                if(!IsPhysicalExternalConnection(ductConnector, reference)) {
                    continue;
                }

                ductEnd.Connections.Add(new ConnectorReference {
                    Connector = reference,
                    ConnectorId = reference.Id,
                    Owner = reference.Owner,
                    OwnerId = reference.Owner.Id,
                    Origin = reference.Origin
                });
            }

            return ductEnd;
        }

        /// <summary>
        /// Проверяет, что ссылка из <c>AllRefs</c> является реальным физическим соединением
        /// воздуховода с внешним элементом. Логические и системные ссылки не нужно сохранять,
        /// так как после пересоздания воздуховода их нельзя восстанавливать через <c>ConnectTo</c>.
        /// </summary>
        /// <param name="ductConnector">Коннектор проверяемого воздуховода.</param>
        /// <param name="reference">Связанный коннектор из набора <c>AllRefs</c>.</param>
        /// <returns>
        /// <see langword="true"/>, если коннекторы физически соединены, относятся к вентиляции
        /// и связанный коннектор принадлежит другому элементу; иначе <see langword="false"/>.
        /// </returns>
        private static bool IsPhysicalExternalConnection(Connector ductConnector, Connector reference) {
            return reference?.Owner != null
                   && !reference.Owner.Id.Equals(ductConnector.Owner.Id)
                   && !(reference.Owner is MEPSystem)
                   && reference.ConnectorType != ConnectorType.Logical
                   && reference.Domain == Domain.DomainHvac
                   && ductConnector.IsConnectedTo(reference);
        }

        private IEnumerable<InsulationData> CaptureInsulations(Duct duct) {
            IEnumerable<ElementId> insulationIds =
                InsulationLiningBase.GetInsulationIds(_document, duct.Id);

            foreach(ElementId id in insulationIds) {
                var insulation = _document.GetElement(id) as DuctInsulation;
                if(insulation == null) {
                    continue;
                }

                yield return new InsulationData {
                    Element = insulation,
                    OldElementId = insulation.Id,
                    TypeId = insulation.GetTypeId(),
                    Thickness = insulation.Thickness,
                    Parameters = CaptureParameters(insulation)
                };
            }
        }

        private bool CanEditRelatedElements(DuctData ductData) {
            IEnumerable<Element> connectedElements = ductData.Ends
                .SelectMany(item => item.Connections)
                .Select(item => item.Owner);
            IEnumerable<Element> insulations = ductData.Insulations.Select(item => item.Element);

            return connectedElements
                .Concat(insulations)
                .GroupBy(item => item.Id)
                .Select(group => group.First())
                .All(item => _elementEditorTracker.IsEditAvailable(item));
        }

        private DuctReplacement RecreateDuct(DuctData ductData) {
            using(var subTransaction = new SubTransaction(_document)) {
                if(subTransaction.Start() != TransactionStatus.Started) {
                    return null;
                }

                try {
                    if(ductData.IsPinned) {
                        ductData.Duct.Pinned = false;
                    }

                    DisconnectDuct(ductData);
                    _document.Delete(ductData.OldDuctId);
                    _document.Regenerate();

                    Duct newDuct = Duct.Create(
                        _document,
                        ductData.SystemTypeId,
                        ductData.DuctTypeId,
                        ductData.LevelId,
                        ductData.StartPoint,
                        ductData.EndPoint);

                    CopyParameters(ductData.Parameters, newDuct);
                    SetDuctGeometry(newDuct, ductData.Height, ductData.Width, ductData.Offset);
                    _document.Regenerate();

                    ReconnectDuct(newDuct, ductData.Ends);
                    List<ElementReplacement> insulations = RecreateInsulations(newDuct, ductData.Insulations);

                    newDuct.Pinned = ductData.IsPinned;
                    _document.Regenerate();

                    if(subTransaction.Commit() != TransactionStatus.Committed) {
                        return null;
                    }

                    return new DuctReplacement(ductData.OldDuctId, newDuct, insulations);
                } catch(Exception) {
                    if(subTransaction.GetStatus() == TransactionStatus.Started) {
                        subTransaction.RollBack();
                    }

                    return null;
                }
            }
        }

        private static void DisconnectDuct(DuctData ductData) {
            foreach(DuctEnd ductEnd in ductData.Ends) {
                foreach(ConnectorReference connection in ductEnd.Connections) {
                    if(ductEnd.DuctConnector.IsConnectedTo(connection.Connector)) {
                        ductEnd.DuctConnector.DisconnectFrom(connection.Connector);
                    }
                }
            }
        }

        private static void SetDuctGeometry(Duct duct, double width, double height, double offset) {
            Parameter widthParameter = duct.GetParam(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            Parameter heightParameter = duct.GetParam(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
            Parameter offsetParameter = duct.GetParam(BuiltInParameter.RBS_OFFSET_PARAM);

            widthParameter.Set(width);
            heightParameter.Set(height);
            offsetParameter.Set(offset);
        }

        private void ReconnectDuct(Duct duct, IEnumerable<DuctEnd> ductEnds) {
            foreach(DuctEnd ductEnd in ductEnds) {
                foreach(ConnectorReference connection in ductEnd.Connections) {
                    Connector ductConnector = FindDuctEndConnector(duct, ductEnd.Origin);
                    Connector externalConnector = FindExternalConnector(connection);
                    ductConnector.ConnectTo(externalConnector);
                    _document.Regenerate();
                }
            }
        }

        private static Connector FindDuctEndConnector(Duct duct, XYZ origin) {
            Connector connector = GetEndConnectors(duct)
                .OrderBy(item => item.Origin.DistanceTo(origin))
                .FirstOrDefault();

            if(connector == null || connector.Origin.DistanceTo(origin) > ConnectorOriginTolerance) {
                throw new InvalidOperationException($"Cannot find a connector on recreated duct {duct.Id}.");
            }

            return connector;
        }

        private Connector FindExternalConnector(ConnectorReference reference) {
            Element owner = _document.GetElement(reference.OwnerId);
            ConnectorManager connectorManager = GetConnectorManager(owner);
            Connector connector = connectorManager?.Lookup(reference.ConnectorId);

            if(connector != null) {
                return connector;
            }

            if(connectorManager != null) {
                connector = connectorManager.Connectors
                    .Cast<Connector>()
                    .Where(item => item.ConnectorType != ConnectorType.Logical
                                   && item.Domain == Domain.DomainHvac)
                    .OrderBy(item => item.Origin.DistanceTo(reference.Origin))
                    .FirstOrDefault();
            }

            if(connector == null || connector.Origin.DistanceTo(reference.Origin) > ConnectorOriginTolerance) {
                throw new InvalidOperationException(
                    $"Cannot restore a connection to element {reference.OwnerId}.");
            }

            return connector;
        }

        private static ConnectorManager GetConnectorManager(Element element) {
            if(element is MEPCurve mepCurve) {
                return mepCurve.ConnectorManager;
            }

            if(element is FamilyInstance familyInstance) {
                return familyInstance.MEPModel?.ConnectorManager;
            }

            if(element is FabricationPart fabricationPart) {
                return fabricationPart.ConnectorManager;
            }

            return null;
        }

        private List<ElementReplacement> RecreateInsulations(
            Duct duct,
            IEnumerable<InsulationData> insulations) {
            var replacements = new List<ElementReplacement>();

            foreach(InsulationData insulation in insulations) {
                DuctInsulation newInsulation = DuctInsulation.Create(
                    _document,
                    duct.Id,
                    insulation.TypeId,
                    insulation.Thickness);

                CopyParameters(insulation.Parameters, newInsulation);
                replacements.Add(new ElementReplacement(insulation.OldElementId, newInsulation));
            }

            return replacements;
        }

        private static IEnumerable<Connector> GetEndConnectors(Duct duct) {
            return duct.ConnectorManager.Connectors
                .Cast<Connector>()
                .Where(item => item.ConnectorType == ConnectorType.End
                               && item.Domain == Domain.DomainHvac);
        }

        private static List<ElementParameterValue> CaptureParameters(Element element) {
            var values = new List<ElementParameterValue>();

            foreach(Parameter parameter in element.Parameters) {
                if(!parameter.HasValue || IsAppliedSeparately(parameter.Id)) {
                    continue;
                }

                ElementParameterValue value = ElementParameterValue.Create(parameter);
                if(value != null) {
                    values.Add(value);
                }
            }

            return values;
        }

        private static void CopyParameters(
            IEnumerable<ElementParameterValue> values,
            Element targetElement) {
            Dictionary<ElementId, Parameter> targetParameters = targetElement.Parameters
                .Cast<Parameter>()
                .GroupBy(item => item.Id)
                .ToDictionary(group => group.Key, group => group.First());

            foreach(ElementParameterValue value in values) {
                if(!targetParameters.TryGetValue(value.ParameterId, out Parameter targetParameter)
                   || targetParameter.IsReadOnly) {
                    continue;
                }

                value.TryApply(targetParameter);
            }
        }

        private static bool IsAppliedSeparately(ElementId parameterId) {
            return parameterId.Equals(_widthParameterId)
                   || parameterId.Equals(_heightParameterId)
                   || parameterId.Equals(_offsetParameterId);
        }

        private static bool IsInvalidId(ElementId elementId) {
            return elementId == null || elementId.Equals(ElementId.InvalidElementId);
        }

        private void UpdateSpecificationElement(
            SpecificationElement specificationElement,
            Element replacement) {
            specificationElement.Element = replacement;
            specificationElement.ElementType = _document.GetElement(replacement.GetTypeId());
            specificationElement.BuiltInCategory = replacement.Category.GetBuiltInCategory();
        }
    }
}
