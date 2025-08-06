using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

using Document = Autodesk.Revit.DB.Document;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models;
internal class RevitRepository {
    private readonly double _maxDistanceBetweenPylon = 10;

    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    /// <summary>
    /// Возвращает список всех листов, имеющихся  в проекте
    /// </summary>
    public List<ViewSheet> AllSheets => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSheet))
            .OfType<ViewSheet>()
            .ToList();

    /// <summary>
    /// Возвращает список всех сечений, имеющихся в проекте
    /// </summary>
    public List<ViewSection> AllSectionViews => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSection))
            .WhereElementIsNotElementType()
            .OfType<ViewSection>()
            .ToList();

    /// <summary>
    /// Возвращает список всех спецификаций, имеющихся в проекте
    /// </summary>
    public List<ViewSchedule> AllScheduleViews => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .WhereElementIsNotElementType()
            .OfType<ViewSchedule>()
            .Where(view => !view.IsTemplate)
            .ToList();

    /// <summary>
    /// Возвращает список всех типоразмеров рамок листа
    /// </summary>
    public List<FamilySymbol> TitleBlocksInProject => new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .WhereElementIsElementType()
            .OfType<FamilySymbol>()
            .ToList();

    /// <summary>
    /// Возвращает список типоразмеров видов в проекте
    /// </summary>
    public List<ViewFamilyType> ViewFamilyTypes => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .OfType<ViewFamilyType>()
            .Where(a => ViewFamily.Section == a.ViewFamily)
            .ToList();
    
    /// <summary>
    /// Возвращает список всех легенд, присутствующих в проекте
    /// </summary>
    public List<View> LegendsInProject => new FilteredElementCollector(Document)
            .OfClass(typeof(View))
            .OfType<View>()
            .Where(view => view.ViewType == ViewType.Legend)
            .ToList();
    
    /// <summary>
    /// Возвращает список всех шаблонов сечений в проекте
    /// </summary>
    public List<ViewSection> AllViewTemplates => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSection))
            .WhereElementIsNotElementType()
            .OfType<ViewSection>()
            .Where(v => v.IsTemplate == true)
            .OrderBy(a => a.Name)
            .ToList();

    /// <summary>
    /// Возвращает список типоразмеров высотных отметок
    /// </summary>
    public List<DimensionType> DimensionTypes => new FilteredElementCollector(Document)
            .OfClass(typeof(DimensionType))
            .OfType<DimensionType>()
            .OrderBy(a => a.Name)
            .ToList();

    /// <summary>
    /// Возвращает список типоразмеров высотных отметок
    /// </summary>
    public List<SpotDimensionType> SpotDimensionTypes => new FilteredElementCollector(Document)
            .OfClass(typeof(SpotDimensionType))
            .OfType<SpotDimensionType>()
            .OrderBy(a => a.Name)
            .ToList();

    /// <summary>
    /// Возвращает список осей, видимых на виде
    /// </summary>
    public List<Grid> GridsInView(View view) => new FilteredElementCollector(Document, view.Id)
        .OfCategory(BuiltInCategory.OST_Grids)
        .Cast<Grid>()
        .ToList();

    /// <summary>
    /// Хранит оболочки над листами пилонов - центральное хранилище информации по пилону для работы плагина
    /// </summary>
    public List<PylonSheetInfo> HostsInfo { get; set; } = [];

    public List<string> HostProjectSections { get; set; } = [];

    /// <summary>
    /// Получает информацию о всех пилонах, размещенных в проекте
    /// </summary>
    public void GetHostData(MainViewModel mainViewModel) {
        HostsInfo.Clear();
        HostProjectSections.Clear();

        foreach(var cat in new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns }) {
            var elems = new FilteredElementCollector(Document)
                .OfCategory(cat)
                .WhereElementIsNotElementType()
                .ToElements();

            AnalizePylons(mainViewModel, elems);
        }

        HostsInfo = [.. HostsInfo
            .OrderBy(i => i.PylonKeyName)];

        // Получаем список разделов в проекте (комплектов документации)
        HostProjectSections = [.. HostsInfo
            .Select(item => item.ProjectSection)
            .Distinct()
            .OrderBy(i => i)];
    }

    /// <summary>
    /// Получает информацию только о выбранном пилоне
    /// </summary>
    public void GetHostData(MainViewModel mainViewModel, IList<Element> elems) {
        HostsInfo.Clear();
        HostProjectSections.Clear();

        AnalizePylons(mainViewModel, elems);

        // Получаем список разделов в проекте (комплектов документации)
        HostProjectSections = [.. HostsInfo
            .Select(item => item.ProjectSection)
            .Distinct()
            .OrderBy(i => i)];
    }

    /// <summary>
    /// Анализирует найденные пилоны, извлекая информацию о них и заполняя список оболочек над листами пилонов
    /// </summary>
    private void AnalizePylons(MainViewModel mainViewModel, IList<Element> elems) {
        foreach(var elem in elems) {
            if(!elem.Name.Contains("Пилон") && !elem.Name.Contains("Колонна")) { continue; }

            // Запрашиваем параметр фильтрации типовых пилонов. Если он не равен заданному, то отсеиваем этот пилон
            var typicalPylonParameter = elem.LookupParameter(mainViewModel.ProjectSettings.TypicalPylonFilterParameter);
            if(typicalPylonParameter == null) {
                mainViewModel.ErrorText = "Параметр фильтрации типовых пилонов не найден";
                return;
            }

            if(typicalPylonParameter.AsString() is null
                || typicalPylonParameter.AsString() != mainViewModel.ProjectSettings.TypicalPylonFilterValue) { continue; }

            // Запрашиваем Раздел проекта
            var projectSectionParameter = elem.LookupParameter(mainViewModel.ProjectSettings.ProjectSection);
            if(projectSectionParameter == null) {
                mainViewModel.ErrorText = "Параметр раздела не найден у элементов Стен или Несущих колонн";
                return;
            }
            string projectSection = projectSectionParameter.AsString();
            if(projectSection is null) { continue; }


            // Запрашиваем Марку пилона
            var hostMarkParameter = elem.LookupParameter(mainViewModel.ProjectSettings.Mark);
            if(hostMarkParameter == null) {
                mainViewModel.ErrorText = "Параметр марки не найден у элементов Стен или Несущих колонн";
                return;
            }
            string hostMark = hostMarkParameter.AsString();
            if(hostMark is null) { continue; }

            var testPylonSheetInfo = HostsInfo
                .Where(item => item.PylonKeyName.Equals(hostMark) && item.ProjectSection.Equals(projectSection))
                .FirstOrDefault();

            if(testPylonSheetInfo is null) {
                var pylonSheetInfo = new PylonSheetInfo(mainViewModel, this, projectSection, hostMark);
                pylonSheetInfo.HostElems.Add(elem);
                FindSheetInPj(mainViewModel, pylonSheetInfo);

                HostsInfo.Add(pylonSheetInfo);
            } else {
                double elemZ = elem.get_BoundingBox(null).Min.Z;
                // Для корректного создания видов нужно, чтобы первым в списке стоял элемент с самой низкой отметкой
                if(testPylonSheetInfo.HostElems[0].get_BoundingBox(null).Min.Z > elemZ) {
                    testPylonSheetInfo.HostElems.Insert(0, elem);
                } else {
                    testPylonSheetInfo.HostElems.Add(elem);
                }

                // Проверяем, что опалубочные модели пилонов отстоят друг от друга на небольшом расстоянии
                // Если это не так, то значит ошибка в проекте
                // Пилоны с одинаковой маркой и значением параметра фильтрации типовых пилонов могут стоять только
                // рядом, например, это могут быть двухэтажные пилоны
                var pt1 = elem.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls
                    ? (elem.Location as LocationCurve).Curve.GetEndPoint(0)
                    : (elem.Location as LocationPoint).Point;

                var elemForCompareByDistance = testPylonSheetInfo.HostElems[0];
                var pt2 = elemForCompareByDistance.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls
                    ? (elemForCompareByDistance.Location as LocationCurve).Curve.GetEndPoint(0)
                    : (elemForCompareByDistance.Location as LocationPoint).Point;

                if(pt1.DistanceTo(pt2) > _maxDistanceBetweenPylon) {
                    mainViewModel.ErrorText = "Найдены пилоны с одинаковой маркой";
                }
            }
        }
    }



    /// <summary>
    /// Ищет лист в проекте по информации из оболочки листа пилона PylonSheetInfo
    /// </summary>
    public void FindSheetInPj(MainViewModel mainViewModel, PylonSheetInfo pylonSheetInfo) {
        var sheet = AllSheets
            .Where(item => item.Name.Equals(mainViewModel.ProjectSettings.SheetPrefix + pylonSheetInfo.PylonKeyName + mainViewModel.ProjectSettings.SheetSuffix))
            .FirstOrDefault();

        if(sheet != null) {
            pylonSheetInfo.SheetInProject = true;
            pylonSheetInfo.PylonViewSheet = sheet;
        }
    }

    /// <summary>
    /// Ищет в проекте вид по имени, указанному в оболочке вида пилона PylonView
    /// </summary>
    public void FindViewSectionInPj(PylonView pylonView) {
        foreach(var view in AllSectionViews) {
            if(view.Name == pylonView.ViewName) {
                pylonView.ViewElement = view;
                break;
            }
        }
    }

    /// <summary>
    /// Ищет в проекте спеки по имени, указанному в оболочке вида пилона PylonView
    /// </summary>
    public void FindViewScheduleInPj(PylonView pylonView) {
        foreach(var view in AllScheduleViews) {
            if(view.Name == pylonView.ViewName) {
                pylonView.ViewElement = view;
                break;
            }
        }
    }

    /// <summary>
    /// Ищет типоразмер по имени типа
    /// </summary>
    public FamilySymbol FindSymbol(BuiltInCategory builtInCategory, string typeName) {
        if(new FilteredElementCollector(Document)
            .OfCategory(builtInCategory)
            .WhereElementIsElementType()
            .FirstOrDefault(e => e.Name == typeName) is not FamilySymbol symbol) {
            return null;
        }

        // Убедимся, что типоразмер активен
        if(!symbol.IsActive) {
            symbol.Activate();
        }
        return symbol;
    }

    /// <summary>
    /// Ищет типоразмер по имени типа
    /// </summary>
    public FamilySymbol FindSymbol(BuiltInCategory builtInCategory, string familyName, string typeName) {
        if(new FilteredElementCollector(Document)
            .OfCategory(builtInCategory)
            .WhereElementIsElementType()
            .Cast<ElementType>()
            .FirstOrDefault(e => e.FamilyName == familyName && e.Name == typeName) is not FamilySymbol symbol) {
            return null;
        }

        // Убедимся, что типоразмер активен
        if(!symbol.IsActive) {
            symbol.Activate();
        }
        return symbol;
    }
}
