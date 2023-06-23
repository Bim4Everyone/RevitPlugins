using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.ViewModels;

using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

using Document = Autodesk.Revit.DB.Document;
using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<ViewSheet> AllSheets => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();

        public List<ViewSection> AllSectionViews => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .OfType<ViewSection>()
                .ToList();

        public IList<Element> AllScheduleViews => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSchedule))
                .WhereElementIsNotElementType()
                .ToElements();


        public List<FamilySymbol> TitleBlocksInProject => new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .ToList();


        public List<ViewFamilyType> ViewFamilyTypes => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .OfType<ViewFamilyType>()
                .Where(a => ViewFamily.Section == a.ViewFamily)
                .ToList();        
        
        public List<View> LegendsInProject => new FilteredElementCollector(Document)
                .OfClass(typeof(View))
                .OfType<View>()
                .Where(view => view.ViewType == ViewType.Legend)
                .ToList();



        public List<PylonSheetInfo> HostsInfo { get; set; } = new List<PylonSheetInfo>();

        public List<string> HostProjectSections { get; set; } = new List<string>();



        public void GetHostData(MainViewModel mainViewModel) {

            HostsInfo.Clear();
            HostProjectSections.Clear();

            foreach(var cat in new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns }) {

                IList<Element> elems = new FilteredElementCollector(Document)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType()
                    .ToElements();


                foreach(Element elem in elems) {
                    if(!elem.Name.Contains("Пилон")) {
                        continue;
                    }


                    // Запрашиваем Раздел проекта
                    Parameter projectSectionParameter = elem.LookupParameter(mainViewModel.PROJECT_SECTION);
                    if(projectSectionParameter == null) {
                        mainViewModel.ErrorText = "Параметр раздела не найден у элементов Стен или Несущих колонн";
                        return;
                    }
                    string projectSection = projectSectionParameter.AsString();
                    if(projectSection is null) { continue; }


                    // Запрашиваем Марку пилона
                    Parameter hostMarkParameter = elem.LookupParameter(mainViewModel.MARK);
                    if(hostMarkParameter == null) {
                        mainViewModel.ErrorText = "Параметр марки не найден у элементов Стен или Несущих колонн";
                        return;
                    }
                    string hostMark = hostMarkParameter.AsString();
                    if(hostMark is null) { continue; }


                    PylonSheetInfo testPylonSheetInfo = HostsInfo
                        .Where(item => item.PylonKeyName.Equals(hostMark))
                        .FirstOrDefault();

                    if(testPylonSheetInfo is null) {
                        PylonSheetInfo pylonSheetInfo = new PylonSheetInfo(mainViewModel, this, hostMark);
                        pylonSheetInfo.ProjectSection = projectSection;
                        pylonSheetInfo.HostElems.Add(elem);
                        FindSheets(mainViewModel, pylonSheetInfo);
                        AnalizeViews(mainViewModel, pylonSheetInfo);

                        HostsInfo.Add(pylonSheetInfo);
                    } else {
                        testPylonSheetInfo.HostElems.Add(elem);
                    }
                }
            }

            HostsInfo = new List<PylonSheetInfo>(HostsInfo
                .OrderBy(i => i.PylonKeyName));

            // Получаем список разделов в проекте (комплектов документации)
            HostProjectSections = new List<string>(HostsInfo
                .Select(item => item.ProjectSection)
                .Distinct()
                .OrderBy(i => i));

            return;
        }


        public void FindSheets(MainViewModel mainViewModel, PylonSheetInfo pylonSheetInfo) {
            
            ViewSheet sheet = AllSheets
                .Where(item => item.Name.Equals(mainViewModel.SHEET_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.SHEET_SUFFIX))
                .FirstOrDefault();

            if(sheet != null) {
                pylonSheetInfo.SheetInProject = true;
                pylonSheetInfo.SheetInProjectEditableInGUI = false;
                pylonSheetInfo.PylonViewSheet = sheet;

                // Сразу ищем рамку листа
                FamilyInstance titleBlock = new FilteredElementCollector(Document, sheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstOrDefault() as FamilyInstance;

                if(titleBlock is null) { return; }

                pylonSheetInfo.TitleBlock = titleBlock;

                // Получаем габариты рамки листа
                pylonSheetInfo.GetTitleBlockSize();
            }

            return;
        }


        public void AnalizeViews(MainViewModel mainViewModel, PylonSheetInfo pylonSheetInfo) {

            foreach(ViewSection view in AllSectionViews) {

                if(view.Name == mainViewModel.GENERAL_VIEW_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.GENERAL_VIEW_SUFFIX) {
                    pylonSheetInfo.GeneralView.ViewElement = view;
                    pylonSheetInfo.GeneralView.InProject = true;
                    pylonSheetInfo.GeneralView.InProjectEditableInGUI = false;

                    string sheetName = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString();
                    
                    if(sheetName != null && pylonSheetInfo.SheetInProject && pylonSheetInfo.PylonViewSheet.Name.Equals(sheetName)) {
                        // Значит видовой экран вида есть на листе, но мы не знаем где
                        pylonSheetInfo.GeneralView.OnSheet = true;
                        pylonSheetInfo.GeneralView.OnSheetEditableInGUI = false;

                        // Ищем видовой экран вида на листе, собираем инфу по нему
                        GetInfoAboutViewport(pylonSheetInfo.PylonViewSheet, pylonSheetInfo.GeneralView);
                    }
                }

                if(view.Name == mainViewModel.GENERAL_VIEW_PERPENDICULAR_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.GENERAL_VIEW_PERPENDICULAR_SUFFIX) {
                    pylonSheetInfo.GeneralViewPerpendicular.ViewElement = view;
                    pylonSheetInfo.GeneralViewPerpendicular.InProject = true;
                    pylonSheetInfo.GeneralViewPerpendicular.InProjectEditableInGUI = false;

                    string sheetName = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString();

                    if(sheetName != null && pylonSheetInfo.SheetInProject && pylonSheetInfo.PylonViewSheet.Name.Equals(sheetName)) {
                        pylonSheetInfo.GeneralViewPerpendicular.OnSheet = true;
                        pylonSheetInfo.GeneralViewPerpendicular.OnSheetEditableInGUI = false;

                        // Ищем видовой экран вида на листе, собираем инфу по нему
                        GetInfoAboutViewport(pylonSheetInfo.PylonViewSheet, pylonSheetInfo.GeneralViewPerpendicular);
                    }
                }

                if(view.Name == mainViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX) {
                    pylonSheetInfo.TransverseViewFirst.ViewElement = view;
                    pylonSheetInfo.TransverseViewFirst.InProject = true;
                    pylonSheetInfo.TransverseViewFirst.InProjectEditableInGUI = false;

                    string sheetName = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString();

                    if(sheetName != null && pylonSheetInfo.SheetInProject && pylonSheetInfo.PylonViewSheet.Name.Equals(sheetName)) {
                        pylonSheetInfo.TransverseViewFirst.OnSheet = true;
                        pylonSheetInfo.TransverseViewFirst.OnSheetEditableInGUI = false;

                        // Ищем видовой экран вида на листе, собираем инфу по нему
                        GetInfoAboutViewport(pylonSheetInfo.PylonViewSheet, pylonSheetInfo.TransverseViewFirst);
                    }
                }

                if(view.Name == mainViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX) {
                    pylonSheetInfo.TransverseViewSecond.ViewElement = view;
                    pylonSheetInfo.TransverseViewSecond.InProject = true;
                    pylonSheetInfo.TransverseViewSecond.InProjectEditableInGUI = false;

                    string sheetName = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString();

                    if(sheetName != null && pylonSheetInfo.SheetInProject && pylonSheetInfo.PylonViewSheet.Name.Equals(sheetName)) {
                        pylonSheetInfo.TransverseViewSecond.OnSheet = true;
                        pylonSheetInfo.TransverseViewSecond.OnSheetEditableInGUI = false;

                        // Ищем видовой экран вида на листе, собираем инфу по нему
                        GetInfoAboutViewport(pylonSheetInfo.PylonViewSheet, pylonSheetInfo.TransverseViewSecond);
                    }
                }

                if(view.Name == mainViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX) {
                    pylonSheetInfo.TransverseViewThird.ViewElement = view;
                    pylonSheetInfo.TransverseViewThird.InProject = true;
                    pylonSheetInfo.TransverseViewThird.InProjectEditableInGUI = false;

                    string sheetName = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString();

                    if(sheetName != null && pylonSheetInfo.SheetInProject && pylonSheetInfo.PylonViewSheet.Name.Equals(sheetName)) {
                        pylonSheetInfo.TransverseViewThird.OnSheet = true;
                        pylonSheetInfo.TransverseViewThird.OnSheetEditableInGUI = false;

                        // Ищем видовой экран вида на листе, собираем инфу по нему
                        GetInfoAboutViewport(pylonSheetInfo.PylonViewSheet, pylonSheetInfo.TransverseViewThird);
                    }
                }



                //if(pylonSheetInfo.GeneralView.ViewElement != null
                //    && pylonSheetInfo.TransverseViewFirst.ViewElement != null
                //    && pylonSheetInfo.TransverseViewSecond.ViewElement != null
                //    && pylonSheetInfo.TransverseViewThird.ViewElement != null) {
                //    break;
                //}
            }

            return;
        }


        public void GetInfoAboutViewport(ViewSheet viewSheet, PylonView pylonView) {

            // Ищем видовой экран вида на листе
            Viewport viewport = new FilteredElementCollector(Document, viewSheet.Id)
                .OfClass(typeof(Viewport))
                .WhereElementIsNotElementType()
                .FirstOrDefault(item =>
                    item.OwnerViewId.Equals(pylonView.ViewElement.Id)) as Viewport;

            // Вообще мы уже выяснили, что видЭкран вида на листе есть, но на всякий
            if(viewport != null) {
                pylonView.ViewportElement = viewport;

                // Получение центра и габаритов видового экрана
                XYZ viewportCenter = viewport.GetBoxCenter();
                Outline viewportOutline = viewport.GetBoxOutline();
                double viewportHalfWidth = viewportOutline.MaximumPoint.X;
                double viewportHalfHeight = viewportOutline.MaximumPoint.Y;

                pylonView.ViewportHalfWidth = viewportHalfWidth;
                pylonView.ViewportHalfHeight = viewportHalfHeight;
            }
        }
    }
}