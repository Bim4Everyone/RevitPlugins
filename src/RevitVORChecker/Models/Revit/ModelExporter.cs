using System.IO;

using Autodesk.Revit.DB;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Выгрузка из модели данных, нужных для проверки ВОР. Модель не изменяется.
/// </summary>
internal static class ModelExporter {
    private const double FeetToMillimeters = 304.8;
    private const double SquareFeetToSquareMeters = 0.09290304;
    private const double CubicFeetToCubicMeters = 0.028316846592;

    private static readonly BuiltInCategory[] _elementCategories = {
        BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs, BuiltInCategory.OST_StructuralFraming,
        BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_Stairs,
        BuiltInCategory.OST_StructuralFoundation, BuiltInCategory.OST_StructConnections,
        BuiltInCategory.OST_GenericModel, BuiltInCategory.OST_Columns, BuiltInCategory.OST_StairsRuns,
        BuiltInCategory.OST_StairsLandings
    };

    private static readonly BuiltInParameter[] _levelParameters = {
        BuiltInParameter.FAMILY_LEVEL_PARAM, BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
        BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM,
        BuiltInParameter.SCHEDULE_LEVEL_PARAM
    };

    /// <summary>
    /// Префиксы имён спецификаций, содержимое которых выгружается.
    /// </summary>
    private static readonly string[] _schedulePrefixes = {
        "ВОР", "!ВОР", "Параметры ВОР", "КЖ_Изоляция", "Параметры_ФОП_ТИП"
    };

    private const int MaxScheduleRows = 3000;

    public static ModelData Export(Document document) {
        WorksetTable worksets = document.IsWorkshared ? document.GetWorksetTable() : null;
        string GetWorkset(Element element) {
            return worksets?.GetWorkset(element.WorksetId)?.Name;
        }

        return new ModelData {
            File = ExportFile(document),
            ProjectInfo = ExportProjectInfo(document),
            Units = ExportUnits(document),
            Worksets = document.IsWorkshared
                ? new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset).Select(workset => workset.Name).ToList()
                : new List<string>(),
            Levels = new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(level => level.Elevation)
                .Select(level => new LevelData {
                    Id = RevitIds.GetValue(level.Id),
                    Name = level.Name,
                    Elevation = level.Elevation * FeetToMillimeters,
                    Workset = GetWorkset(level)
                })
                .ToList(),
            Links = ExportLinks(document, GetWorkset),
            Imports = new FilteredElementCollector(document)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .Select(import => new ImportData {
                    Id = RevitIds.GetValue(import.Id),
                    Name = import.Category != null ? import.Category.Name : "",
                    IsLinked = import.IsLinked,
                    IsViewSpecific = import.ViewSpecific,
                    Workset = GetWorkset(import)
                })
                .ToList(),
            Views3D = new FilteredElementCollector(document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(view => !view.IsTemplate)
                .Select(view => new View3DData {
                    Id = RevitIds.GetValue(view.Id),
                    Name = view.Name,
                    IsCropBoxActive = view.CropBoxActive,
                    IsSectionBoxActive = view.IsSectionBoxActive
                })
                .ToList(),
            Materials = ExportMaterials(document),
            DirectShapeCount = new FilteredElementCollector(document).OfClass(typeof(DirectShape)).GetElementCount(),
            Elements = ExportElements(document, GetWorkset),
            Warnings = document.GetWarnings()
                .Select(warning => new WarningData {
                    Text = warning.GetDescriptionText(),
                    Severity = warning.GetSeverity().ToString(),
                    ElementIds = warning.GetFailingElements()
                        .Concat(warning.GetAdditionalElements())
                        .Select(RevitIds.GetValue)
                        .ToList()
                })
                .ToList(),
            Schedules = ExportSchedules(document)
        };
    }

    /// <summary>
    /// Значение параметра как текст (как в спецификации), null — нет значения.
    /// </summary>
    public static string GetParameterText(Parameter parameter) {
        if(parameter == null) {
            return null;
        }

        if(parameter.StorageType == StorageType.String) {
            return parameter.AsString();
        }

        if(parameter.StorageType == StorageType.ElementId) {
            return parameter.AsValueString();
        }

        if(!parameter.HasValue) {
            return null;
        }

        string valueText = parameter.AsValueString();
        if(valueText != null) {
            return valueText;
        }

        return parameter.StorageType switch {
            StorageType.Integer => parameter.AsInteger().ToString(),
            StorageType.Double => NumberText(parameter.AsDouble()),
            _ => null
        };
    }

    private static string NumberText(double value) {
        return double.IsNaN(value) || double.IsInfinity(value) ? null : value.ToString("R", PythonFormat.Invariant);
    }

    private static double? FiniteOrNull(double value) {
        return double.IsNaN(value) || double.IsInfinity(value) ? null : value;
    }

    private static ModelFileData ExportFile(Document document) {
        long size = -1;
        try {
            if(File.Exists(document.PathName)) {
                size = new FileInfo(document.PathName).Length;
            }
        } catch(Exception) {
            size = -1;
        }

        string centralPath = null;
        try {
            if(document.IsWorkshared) {
                centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(document.GetWorksharingCentralModelPath());
            }
        } catch(Exception) {
            centralPath = null;
        }

        return new ModelFileData {
            Title = document.Title,
            Path = document.PathName,
            CentralPath = centralPath,
            SizeBytes = size,
            IsWorkshared = document.IsWorkshared,
            RevitVersion = document.Application.VersionName + " " + document.Application.SubVersionNumber,
            ExportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
    }

    private static Dictionary<string, string> ExportProjectInfo(Document document) {
        var projectInfo = new Dictionary<string, string>();
        foreach(Parameter parameter in document.ProjectInformation.Parameters) {
            string value = GetParameterText(parameter);
            if(!string.IsNullOrEmpty(value)) {
                projectInfo[parameter.Definition.Name] = value;
            }
        }

        return projectInfo;
    }

    private static Dictionary<string, string> ExportUnits(Document document) {
        Units units = document.GetUnits();
        string Describe(ForgeTypeId spec) {
            try {
                FormatOptions options = units.GetFormatOptions(spec);
                return options.GetUnitTypeId().TypeId + "|acc=" + NumberText(options.Accuracy);
            } catch(Exception) {
                return null;
            }
        }

        return new Dictionary<string, string> {
            ["length"] = Describe(SpecTypeId.Length),
            ["area"] = Describe(SpecTypeId.Area),
            ["volume"] = Describe(SpecTypeId.Volume),
            ["angle"] = Describe(SpecTypeId.Angle)
        };
    }

    private static List<LinkData> ExportLinks(Document document, Func<Element, string> getWorkset) {
        var links = new List<LinkData>();
        foreach(RevitLinkType linkType in new FilteredElementCollector(document).OfClass(typeof(RevitLinkType))) {
            var instances = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(instance => instance.GetTypeId() == linkType.Id)
                .ToList();
            links.Add(new LinkData {
                Name = linkType.Name,
                IsLoaded = RevitLinkType.IsLoaded(document, linkType.Id),
                Attachment = linkType.AttachmentType.ToString(),
                InstanceCount = instances.Count,
                InstanceWorksets = instances.Select(getWorkset).ToList(),
                TypeWorkset = getWorkset(linkType)
            });
        }

        return links;
    }

    private static List<MaterialData> ExportMaterials(Document document) {
        var materials = new List<MaterialData>();
        foreach(Material material in new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>()) {
            var data = new MaterialData {
                Id = RevitIds.GetValue(material.Id),
                Name = material.Name,
                MaterialClass = material.MaterialClass,
                Keynote = material.get_Parameter(BuiltInParameter.KEYNOTE_PARAM)?.AsString(),
                Description = material.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)?.AsString()
            };
            foreach(string parameterName in MaterialData.ExportedParameters) {
                data.SetParameter(parameterName, GetParameterText(material.LookupParameter(parameterName)));
            }

            materials.Add(data);
        }

        return materials;
    }

    private static List<ElementData> ExportElements(Document document, Func<Element, string> getWorkset) {
        var elements = new List<ElementData>();
        var collector = new FilteredElementCollector(document)
            .WherePasses(new ElementMulticategoryFilter(_elementCategories))
            .WhereElementIsNotElementType();
        foreach(Element element in collector) {
            if(element.Category == null) {
                continue;
            }

            elements.Add(ExportElement(document, element, getWorkset));
        }

        return elements;
    }

    private static ElementData ExportElement(Document document, Element element, Func<Element, string> getWorkset) {
        var elementType = document.GetElement(element.GetTypeId()) as ElementType;
        Parameter volume = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
        var familyInstance = element as FamilyInstance;
        Parameter phaseCreated = element.get_Parameter(BuiltInParameter.PHASE_CREATED);
        Parameter phaseDemolished = element.get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
        var data = new ElementData {
            Id = RevitIds.GetValue(element.Id),
            Category = element.Category.Name,
            BuiltInCategory = ((BuiltInCategory) RevitIds.GetValue(element.Category.Id)).ToString(),
            FamilyName = elementType?.FamilyName,
            TypeName = elementType != null ? elementType.Name : element.Name,
            Workset = getWorkset(element),
            LevelName = GetLevelName(document, element),
            IsInGroup = element.GroupId != ElementId.InvalidElementId,
            IsInPlace = familyInstance?.Symbol != null && familyInstance.Symbol.Family.IsInPlace,
            SuperComponentId = familyInstance?.SuperComponent != null
                ? RevitIds.GetValue(familyInstance.SuperComponent.Id)
                : null,
            PhaseCreated = phaseCreated?.AsValueString(),
            PhaseDemolished = phaseDemolished?.AsValueString(),
            Volume = volume != null && volume.HasValue ? FiniteOrNull(volume.AsDouble() * CubicFeetToCubicMeters) : null
        };

        BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
        if(boundingBox != null) {
            data.BoundingBox = new List<double?> {
                boundingBox.Min.X * FeetToMillimeters, boundingBox.Min.Y * FeetToMillimeters,
                boundingBox.Min.Z * FeetToMillimeters, boundingBox.Max.X * FeetToMillimeters,
                boundingBox.Max.Y * FeetToMillimeters, boundingBox.Max.Z * FeetToMillimeters
            };
        }

        foreach(string parameterName in ElementData.ExportedParameters) {
            Parameter parameter = element.LookupParameter(parameterName);
            string owner = "i";
            if(parameter == null && elementType != null) {
                parameter = elementType.LookupParameter(parameterName);
                owner = "t";
            }

            if(parameter != null) {
                data.Parameters[parameterName] = new List<string> { GetParameterText(parameter), owner };
            }
        }

        try {
            foreach(ElementId materialId in element.GetMaterialIds(false)) {
                data.Materials.Add(new List<double?> {
                    RevitIds.GetValue(materialId),
                    FiniteOrNull(element.GetMaterialVolume(materialId) * CubicFeetToCubicMeters),
                    FiniteOrNull(element.GetMaterialArea(materialId, false) * SquareFeetToSquareMeters)
                });
            }
        } catch(Exception) {
            // у части элементов материалы не читаются — элемент выгружается без материалов
        }

        try {
            data.PaintMaterialIds = element.GetMaterialIds(true).Select(RevitIds.GetValue).ToList();
        } catch(Exception) {
            data.PaintMaterialIds = new List<long>();
        }

        return data;
    }

    private static string GetLevelName(Document document, Element element) {
        if(element.LevelId != null && element.LevelId != ElementId.InvalidElementId) {
            Element level = document.GetElement(element.LevelId);
            if(level != null) {
                return level.Name;
            }
        }

        foreach(BuiltInParameter levelParameter in _levelParameters) {
            Parameter parameter = element.get_Parameter(levelParameter);
            if(parameter != null && parameter.AsElementId() != ElementId.InvalidElementId) {
                Element level = document.GetElement(parameter.AsElementId());
                if(level != null) {
                    return level.Name;
                }
            }
        }

        return null;
    }

    private static List<ScheduleData> ExportSchedules(Document document) {
        var schedules = new List<ScheduleData>();
        var viewSchedules = new FilteredElementCollector(document)
            .OfClass(typeof(ViewSchedule))
            .Cast<ViewSchedule>()
            .Where(schedule => !schedule.IsTitleblockRevisionSchedule
                               && !schedule.IsTemplate
                               && _schedulePrefixes.Any(prefix => schedule.Name.StartsWith(prefix)));
        foreach(ViewSchedule schedule in viewSchedules) {
            bool includesLinks = false;
            try {
                includesLinks = schedule.Definition.IncludeLinkedFiles;
            } catch(Exception) {
                includesLinks = false;
            }

            schedules.Add(new ScheduleData {
                Name = schedule.Name,
                IncludeLinks = includesLinks,
                Filters = ReadFilters(schedule.Definition),
                Rows = ReadBodyRows(schedule)
            });
        }

        return schedules;
    }

    private static List<string> ReadFilters(ScheduleDefinition definition) {
        var filters = new List<string>();
        try {
            foreach(ScheduleFilter filter in definition.GetFilters()) {
                ScheduleField field = definition.GetField(filter.FieldId);
                string value = filter.IsStringValue ? filter.GetStringValue()
                    : filter.IsDoubleValue ? NumberText(filter.GetDoubleValue())
                    : filter.IsElementIdValue ? RevitIds.GetValue(filter.GetElementIdValue()).ToString()
                    : filter.IsIntegerValue ? filter.GetIntegerValue().ToString()
                    : "";
                filters.Add(field.GetName() + " " + filter.FilterType + " " + value);
            }
        } catch(Exception) {
            // фильтры, которые не читаются API, пропускаются
        }

        return filters;
    }

    private static List<List<string>> ReadBodyRows(ViewSchedule schedule) {
        var rows = new List<List<string>>();
        try {
            TableSectionData body = schedule.GetTableData().GetSectionData(SectionType.Body);
            int rowCount = Math.Min(body.NumberOfRows, MaxScheduleRows);
            for(int row = 0; row < rowCount; row++) {
                var cells = new List<string>();
                for(int column = 0; column < body.NumberOfColumns; column++) {
                    cells.Add(schedule.GetCellText(SectionType.Body, row, column));
                }

                rows.Add(cells);
            }
        } catch(Exception) {
            // спецификация не читается — без строк
        }

        return rows;
    }
}
