using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

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

        public List<ViewSchedule> AllScheduleViews => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSchedule))
                .WhereElementIsNotElementType()
                .OfType<ViewSchedule>()
                .ToList();


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

        public List<ViewSection> AllViewTemplates => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .OfType<ViewSection>()
                .Where(v => v.IsTemplate == true)
                .OrderBy(a => a.Name)
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


                    // Запрашиваем параметр фильтрации типовых пилонов. Если он не равен заданному, то отсеиваем этот пилон
                    Parameter typicalPylonParameter = elem.LookupParameter(mainViewModel.ProjectSettings.TYPICAL_PYLON_FILTER_PARAMETER);
                    if(typicalPylonParameter == null) {
                        mainViewModel.ErrorText = "Параметр фильтрации типовых пилонов не найден";
                        return;
                    }

                    if(typicalPylonParameter.AsString() is null || typicalPylonParameter.AsString() != mainViewModel.ProjectSettings.TYPICAL_PYLON_FILTER_VALUE) { continue; }


                    // Запрашиваем Раздел проекта
                    Parameter projectSectionParameter = elem.LookupParameter(mainViewModel.ProjectSettings.PROJECT_SECTION);
                    if(projectSectionParameter == null) {
                        mainViewModel.ErrorText = "Параметр раздела не найден у элементов Стен или Несущих колонн";
                        return;
                    }
                    string projectSection = projectSectionParameter.AsString();
                    if(projectSection is null) { continue; }


                    // Запрашиваем Марку пилона
                    Parameter hostMarkParameter = elem.LookupParameter(mainViewModel.ProjectSettings.MARK);
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
                        FindSheetInPj(mainViewModel, pylonSheetInfo);
                        //AnalizeSectionViews(mainViewModel, pylonSheetInfo);
                        //AnalizeScheduleViews(mainViewModel, pylonSheetInfo);

                        HostsInfo.Add(pylonSheetInfo);
                    } else {
                        testPylonSheetInfo.HostElems.Add(elem);
                        mainViewModel.ErrorText = "Найдены пилоны с одинаковой маркой";
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


        public void FindSheetInPj(MainViewModel mainViewModel, PylonSheetInfo pylonSheetInfo) {
            
            ViewSheet sheet = AllSheets
                .Where(item => item.Name.Equals(mainViewModel.ProjectSettings.SHEET_PREFIX + pylonSheetInfo.PylonKeyName + mainViewModel.ProjectSettings.SHEET_SUFFIX))
                .FirstOrDefault();

            if(sheet != null) {
                pylonSheetInfo.SheetInProject = true;
                pylonSheetInfo.PylonViewSheet = sheet;
            }

            return;
        }



        /// <summary>
        /// Ищет в проекте вид по имени, указанному в PylonView
        /// </summary>
        public void FindViewSectionInPj(PylonView pylonView) {

            foreach(ViewSection view in AllSectionViews) {
                
                if(view.Name == pylonView.ViewName) {
                    pylonView.ViewElement = view;

                    break;
                }
            }
        }

        /// <summary>
        /// Ищет в проекте спеки по имени, указанному в PylonView
        /// </summary>
        public void FindViewScheduleInPj(PylonView pylonView) {

            foreach(ViewSchedule view in AllScheduleViews) {

                if(view.Name == pylonView.ViewName) {
                    pylonView.ViewElement = view;

                    break;
                }
            }
        }
    }
}