using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.ViewModels;

using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

using Document = Autodesk.Revit.DB.Document;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace RevitPylonDocumentation.Models {
    public class PylonSheetInfo {
        internal PylonSheetInfo(MainViewModel mvm, RevitRepository repository, string pylonKeyName) {
            ViewModel = mvm;
            Repository = repository;
            PylonKeyName = pylonKeyName;

            GeneralView = new PylonView(ViewModel, repository, this);
            GeneralViewPerpendicular = new PylonView(ViewModel, repository, this);
            TransverseViewFirst = new PylonView(ViewModel, repository, this);
            TransverseViewSecond = new PylonView(ViewModel, repository, this);
            TransverseViewThird = new PylonView(ViewModel, repository, this);

            RebarSchedule = new PylonView(ViewModel, repository, this);
            MaterialSchedule = new PylonView(ViewModel, repository, this);
            SystemPartsSchedule = new PylonView(ViewModel, repository, this);
            IFCPartsSchedule = new PylonView(ViewModel, repository, this);

            LegendView = new PylonView(ViewModel, repository, this);
        }

        internal  MainViewModel ViewModel { get; set; }
        internal  RevitRepository Repository { get; set; }

        public bool IsCheck { get; set; } = false;
        public bool SheetInProject { get; set; } = false;
        //public bool SheetInProjectEditableInGUI { get; set; } = true;


        // Марка пилона 
        public string PylonKeyName { get; set; }
        public string ProjectSection { get; set; }

        public List<Element> HostElems { get; set; } = new List<Element>();
        public ViewSheet PylonViewSheet { get; set; }


        // Рамка листа
        public FamilyInstance TitleBlock { get; set; }
        public double TitleBlockHeight { get; set; }
        public double TitleBlockWidth { get; set; }


        // Видовые экраны разразов
        public PylonView GeneralView { get; set; }
        public PylonView GeneralViewPerpendicular { get; set; }
        public PylonView TransverseViewFirst { get; set; }
        public PylonView TransverseViewSecond { get; set; }
        public PylonView TransverseViewThird { get; set; }



        // Видовые экраны спецификаций
        public PylonView RebarSchedule { get; set; }
        public PylonView MaterialSchedule { get; set; }
        public PylonView SystemPartsSchedule { get; set; }
        public PylonView IFCPartsSchedule { get; set; }


        // Легенда
        public PylonView LegendView { get; set; }



        /// <summary>
        /// Создание листа, задание имени, поиск рамки и задание ей нужных габаритов
        /// </summary>
        public bool CreateSheet() {
            if(PylonViewSheet != null ) {
                return false; 
            }

            PylonViewSheet = ViewSheet.Create(Repository.Document, ViewModel.SelectedTitleBlocks.Id);
            PylonViewSheet.Name = ViewModel.SHEET_PREFIX + PylonKeyName + ViewModel.SHEET_SUFFIX;

            Parameter viewSheetGroupingParameter = PylonViewSheet.LookupParameter(ViewModel.DISPATCHER_GROUPING_FIRST);
            if(viewSheetGroupingParameter == null) {
            } else {
                viewSheetGroupingParameter.Set(ViewModel.SelectedProjectSection);
            }

            FindTitleBlock();
            SetTitleBlockSize(Repository.Document);

            return true;
        }

        /// <summary>
        /// Поиск рамки на листе
        /// </summary>
        public bool FindTitleBlock() {

            // Ищем рамку листа
            FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, PylonViewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) { return false; }
            TitleBlock = titleBlock;

            return true;
        }



        /// <summary>
        /// Получает габариты рамки листа и записывает в параметры TitleBlockWidth и TitleBlockHeight
        /// </summary>
        internal void GetTitleBlockSize() {
            if(TitleBlock is null) {
                return;
            }

            // Получение габаритов рамки листа
            BoundingBoxXYZ boundingBoxXYZ = TitleBlock.get_BoundingBox(PylonViewSheet);
            TitleBlockWidth = -1 * boundingBoxXYZ.Min.X;
            TitleBlockHeight = boundingBoxXYZ.Max.Y;
        }


        /// <summary>
        /// Задает размеры рамки листа (по дефолту ставит А3) и запоминает
        /// </summary>
        internal void SetTitleBlockSize(Document doc, int sheetSize = 3, int sheetCoefficient = 1) {
            if(this.TitleBlock is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не найдена рамка на листе \"{0}\"", PylonViewSheet.Name);
                #endregion
                return;
            }
            // Пытаемся задать габарит листа
            Parameter paramA = this.TitleBlock.LookupParameter(ViewModel.SHEET_SIZE);
            Parameter paramX = this.TitleBlock.LookupParameter(ViewModel.SHEET_COEFFICIENT);
            if(paramA is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! У рамки на листе не найден параметр \"{0}\", отвечающий за формат листа \"{1}\"",
                    ViewModel.SHEET_SIZE, PylonViewSheet.Name);
                #endregion
            }
            if(paramX is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! У рамки на листе не найден параметр \"{0}\", отвечающий за множитель формата листа \"{1}\"", 
                    ViewModel.SHEET_COEFFICIENT, PylonViewSheet.Name);
                #endregion
            }

            if(paramA != null && paramX != null) {
                paramA.Set(sheetSize);
                paramX.Set(sheetCoefficient);
                doc.Regenerate();

                string format = string.Empty;
                if(sheetCoefficient > 1) {
                    format = string.Format("А{0}х{1}", sheetSize, sheetCoefficient);
                } else {
                    format = "А" + sheetSize;
                }
                #region Отчет
                ViewModel.Report = string.Format("\tЛисту задан формат: {0}", format);
                #endregion
            }

            // Получение итоговых габаритов рамки листа
            GetTitleBlockSize();
        }

        /// <summary>
        /// Получает и сохраняет имена видов в соответствии с префиксами/суффиксами от пользователя
        /// </summary>
        public void WriteViewNames() {
            
            GeneralView.ViewName = ViewModel.GENERAL_VIEW_PREFIX + PylonKeyName + ViewModel.GENERAL_VIEW_SUFFIX;
            GeneralViewPerpendicular.ViewName = ViewModel.GENERAL_VIEW_PERPENDICULAR_PREFIX + PylonKeyName + ViewModel.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            TransverseViewFirst.ViewName = ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX;
            TransverseViewSecond.ViewName = ViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX;
            TransverseViewThird.ViewName = ViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX;

            RebarSchedule.ViewName = ViewModel.REBAR_SCHEDULE_PREFIX + PylonKeyName + ViewModel.REBAR_SCHEDULE_SUFFIX;
            MaterialSchedule.ViewName = ViewModel.MATERIAL_SCHEDULE_PREFIX + PylonKeyName + ViewModel.MATERIAL_SCHEDULE_SUFFIX;
            SystemPartsSchedule.ViewName = ViewModel.SYSTEM_PARTS_SCHEDULE_PREFIX + PylonKeyName + ViewModel.SYSTEM_PARTS_SCHEDULE_SUFFIX;
            IFCPartsSchedule.ViewName = ViewModel.IFC_PARTS_SCHEDULE_PREFIX + PylonKeyName + ViewModel.IFC_PARTS_SCHEDULE_SUFFIX;
        }



        /// <summary>
        /// Ищет и запоминает виды и видовые экраны через видовые экраны, размещенные на листе
        /// </summary>
        public void FindViewsNViewportsOnSheet() {

            foreach(ElementId id in PylonViewSheet.GetAllViewports()) {

                Viewport viewport = Repository.Document.GetElement(id) as Viewport;
                ViewSection viewSection = Repository.Document.GetElement(viewport.ViewId) as ViewSection;
                
                // Видовые экраны не разрезов нас не интересуют
                if(viewSection is null) { continue; }

                // Если вид не находили до этого в этом цикле и его имя равно нужному, то сохраняем
                // GeneralView
                if(GeneralView.ViewElement is null && viewSection.Name.Equals(GeneralView.ViewName)) {
                    GeneralView.ViewElement = viewSection;
                    GeneralView.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(GeneralView, viewport);
                    continue;
                }

                // GeneralViewPerpendicular
                if(GeneralViewPerpendicular.ViewElement is null && viewSection.Name.Equals(GeneralViewPerpendicular.ViewName)) {
                    GeneralViewPerpendicular.ViewElement = viewSection;
                    GeneralViewPerpendicular.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(GeneralViewPerpendicular, viewport);
                    continue;
                }

                // TransverseViewFirst
                if(TransverseViewFirst.ViewElement is null && viewSection.Name.Equals(TransverseViewFirst.ViewName)) {
                    TransverseViewFirst.ViewElement = viewSection;
                    TransverseViewFirst.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewFirst, viewport);
                    continue;
                }

                // TransverseViewSecond
                if(TransverseViewSecond.ViewElement is null && viewSection.Name.Equals(TransverseViewSecond.ViewName)) {
                    TransverseViewSecond.ViewElement = viewSection;
                    TransverseViewSecond.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewSecond, viewport);
                    continue;
                }

                // TransverseViewThird
                if(TransverseViewThird.ViewElement is null && viewSection.Name.Equals(TransverseViewThird.ViewName)) {
                    TransverseViewThird.ViewElement = viewSection;
                    TransverseViewThird.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewThird, viewport);
                    continue;
                }
            }
        }




        /// <summary>
        /// Получение и сохранение информации о центре и габаритах видового экрана
        /// </summary>
        public void GetInfoAboutViewport(PylonView pylonView, Viewport viewport) {

            XYZ viewportCenter = viewport.GetBoxCenter();
            Outline viewportOutline = viewport.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            pylonView.ViewportCenter = viewportCenter;
            pylonView.ViewportHalfWidth = viewportHalfWidth;
            pylonView.ViewportHalfHeight = viewportHalfHeight;
        }


        // Метод для размещения основного вида пилона.
        // Позиционирование - левый верхний угол листа
        //internal void PlaceGeneralViewport() {
        //    #region Отчет
        //    ViewModel.Report = "Основной вид";
        //    #endregion
        //    // Ищем основное сечение пилона пока не найдем
        //    foreach(var item in ViewModel._revitRepository.AllSectionViews) {
        //        ViewSection view = item as ViewSection;
        //        if(view == null) {
        //            continue;
        //        }

        //        if(view.Name == ViewModel.GENERAL_VIEW_PREFIX + PylonKeyName + ViewModel.GENERAL_VIEW_SUFFIX) {
        //            // Заполнеяем данные для задания
        //            GeneralView.ViewElement = view;
        //            GeneralView.ViewportTypeName = "Заголовок на листе";
        //            GeneralView.ViewportNumber = "100";
        //            GeneralView.ViewportName = "Пилон " + PylonKeyName;
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tОсновной вид пилона \"{0}\" для листа \"{1}\" успешно найден",
        //                ViewModel.GENERAL_VIEW_PREFIX + PylonKeyName + ViewModel.GENERAL_VIEW_SUFFIX, PylonViewSheet.Name);
        //            #endregion
        //            break;
        //        }
        //    }
        //    // Проверка вдруг вид не найден
        //    if(GeneralView.ViewElement is null) {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПроизошла ошибка! Не найден основной вид пилона \"{0}\" для листа \"{1}\"", 
        //            ViewModel.GENERAL_VIEW_PREFIX + PylonKeyName + ViewModel.GENERAL_VIEW_SUFFIX, PylonViewSheet.Name);
        //        #endregion
        //        return;
        //    }

        //    // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
        //    string answer = GeneralView.PlacePylonViewport(ViewModel._revitRepository.Document, this);
        //    if(answer.Length > 0) {
        //        ViewModel.Report = answer;
        //    }

        //    if(GeneralView.ViewportElement is null) {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tРабота с размещением основного вида пилона на листе \"{0}\" прервана", PylonViewSheet.Name);
        //        #endregion
        //        return;
        //    } else {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tОсновной вид пилона \"{0}\" успешно размещен на листе \"{1}\"", GeneralView.ViewElement.Name, PylonViewSheet.Name);
        //        #endregion
        //    }

        //    // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
        //    if(GeneralView.ViewportHalfHeight * 2 > TitleBlockHeight) {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tОсновной вид \"{0}\" не вмещается в рамку на листе, поэтому изменим ее габарит", PylonViewSheet.Name);
        //        #endregion

        //        SetTitleBlockSize(ViewModel._revitRepository.Document, 2, 1);
        //    }

        //    // Рассчитываем и задаем корректную точку вставки основного вида пилона
        //    XYZ newCenter = new XYZ(
        //        - TitleBlockWidth + GeneralView.ViewportHalfWidth + 0.065,
        //        TitleBlockHeight - GeneralView.ViewportHalfHeight - 0.016,
        //        0);
        //    (GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);

        //    GeneralView.ViewportCenter = newCenter;
        //    #region Отчет
        //    ViewModel.Report = string.Format("\tОсновной вид пилона \"{0}\" спозиционирован", PylonViewSheet.Name);
        //    ViewModel.Report = "Основной вид - работа завершена";
        //    #endregion

        //    return;
        //}


        // Метод для размещения поперечных видов пилона.
        // Позиционирование - снизу вверх  от нижней границы листа правее от основного вида




        //internal void PlaceTransverseViewPorts() {
        //    double coordinateX = 0;
        //    double coordinateY = 0;

        //    #region Отчет
        //    ViewModel.Report = "Поперечные виды";
        //    #endregion

        //    #region Поиск поперечных сечений пилона 1-1, 2-2, 3-3 
        //    foreach(var item in ViewModel._revitRepository.AllSectionViews) {
        //        ViewSection view = item as ViewSection;
        //        if(view == null) {
        //            continue;
        //        }

        //        if(view.Name == ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX) {
        //            TransverseViewFirst.ViewElement = view;
        //            TransverseViewFirst.ViewportTypeName = "Сечение_Номер вида";
        //            TransverseViewFirst.ViewportNumber = "1";
        //            TransverseViewFirst.ViewportName = "";
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид пилона \"{0}\" для листа \"{1}\" успешно найден",
        //                ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX, PylonViewSheet.Name);
        //            #endregion

        //            // Останавливаем поиск, если ранее нашли сечение 2-2 и 3-3
        //            if(TransverseViewSecond.ViewElement != null && TransverseViewThird.ViewElement != null) {
        //                break;
        //            }
        //        }

        //        if(view.Name == ViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX) {
        //            TransverseViewSecond.ViewElement = view;
        //            TransverseViewSecond.ViewportTypeName = "Сечение_Номер вида";
        //            TransverseViewSecond.ViewportNumber = "2";
        //            TransverseViewSecond.ViewportName = "";
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид пилона \"{0}\" для листа \"{1}\" успешно найден",
        //                ViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX, PylonViewSheet.Name);
        //            #endregion

        //            // Останавливаем поиск, если ранее нашли сечение 1-1 и 3-3
        //            if(TransverseViewFirst.ViewElement != null && TransverseViewThird.ViewElement != null) {
        //                break;
        //            }
        //        }

        //        if(view.Name == ViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX) {
        //            TransverseViewThird.ViewElement = view;
        //            TransverseViewThird.ViewportTypeName = "Сечение_Номер вида";
        //            TransverseViewThird.ViewportNumber = "3";
        //            TransverseViewThird.ViewportName = "";
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид пилона \"{0}\" для листа \"{1}\" успешно найден",
        //                ViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX, PylonViewSheet.Name);
        //            #endregion

        //            // Останавливаем поиск, если ранее нашли сечение 1-1 и 2-2
        //            if(TransverseViewFirst.ViewElement != null && TransverseViewSecond.ViewElement != null) {
        //                break;
        //            }
        //        }
        //    }
        //    #endregion

        //    double titleBlockOffset = UnitUtilsHelper.ConvertToInternalValue(20);


        //    // Размещение поперечного сечения 1-1 (потому что он ниже)
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 1-1";
        //    #endregion
        //    if(TransverseViewFirst.ViewElement != null) {
        //        // Передаем первое поперечное сечение пилона
        //        string answer = TransverseViewFirst.PlacePylonViewport(ViewModel._revitRepository.Document, this);
        //        if(answer.Length > 0) {
        //            ViewModel.Report = answer;
        //        }

        //        if(TransverseViewFirst.ViewportElement is null) {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tРабота с размещением поперечного вида пилона 1-1 на листе \"{0}\" прервана", PylonViewSheet.Name);
        //            #endregion
        //            return;
        //        } else {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид 1-1 пилона \"{0}\" успешно размещен на листе \"{1}\"", TransverseViewFirst.ViewElement.Name, PylonViewSheet.Name);
        //            #endregion
        //        }

        //        // Рассчитываем и задаем корректную точку вставки первого попереченого вида пилона
        //        XYZ newCenterFirst = new XYZ(
        //            - TitleBlockWidth + titleBlockOffset + GeneralView.ViewportHalfWidth * 2 + TransverseViewFirst.ViewportHalfWidth,
        //            0.015 + TransverseViewFirst.ViewportHalfHeight,
        //            0);
        //        (TransverseViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenterFirst);

        //        TransverseViewFirst.ViewportCenter = newCenterFirst;
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПоперечный вид 1-1 пилона \"{0}\" спозиционирован", PylonViewSheet.Name);
        //        #endregion
        //    } else {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПроизошла ошибка! Не найден поперечный вид пилона \"{0}\" для листа \"{1}\"",
        //            ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX, PylonViewSheet.Name);
        //        #endregion
        //    }
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 1-1 - работа завершена";
        //    #endregion


        //    // Размещение поперечного сечения 2-2
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 2-2";
        //    #endregion
        //    if(TransverseViewSecond.ViewElement != null) {
        //        string answer = TransverseViewSecond.PlacePylonViewport(ViewModel._revitRepository.Document, this);
        //        if(answer.Length > 0) {
        //            ViewModel.Report = answer;
        //        }

        //        if(TransverseViewSecond.ViewportElement is null) {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tРабота с размещением поперечного вида пилона 2-2 на листе \"{0}\" прервана", PylonViewSheet.Name);
        //            #endregion
        //            return;
        //        } else {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид 2-2 пилона \"{0}\" успешно размещен на листе \"{1}\"", TransverseViewSecond.ViewElement.Name, PylonViewSheet.Name);
        //            #endregion
        //        }


        //        // Рассчитываем и задаем корректную точку вставки второго поперечного вида пилона
        //        coordinateX = 0;
        //        coordinateY = 0;
        //        if(TransverseViewFirst.ViewElement == null) {
        //            coordinateX = - TitleBlockWidth + titleBlockOffset + GeneralView.ViewportHalfWidth * 2 + TransverseViewSecond.ViewportHalfWidth;
        //            coordinateY = 0.015 + TransverseViewSecond.ViewportHalfHeight;
        //        } else {
        //            coordinateX = TransverseViewFirst.ViewportCenter.X;
        //            coordinateY = TransverseViewFirst.ViewportCenter.Y + TransverseViewFirst.ViewportHalfHeight
        //                + 0.005 + TransverseViewSecond.ViewportHalfHeight;
        //        }

        //        XYZ newCenterSecond = new XYZ(
        //            coordinateX,
        //            coordinateY,
        //            0);
        //        (TransverseViewSecond.ViewportElement as Viewport).SetBoxCenter(newCenterSecond);

        //        TransverseViewSecond.ViewportCenter = newCenterSecond;

        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПоперечный вид 2-2 пилона \"{0}\" спозиционирован", PylonViewSheet.Name);
        //        #endregion
        //    } else {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПроизошла ошибка! Не найден поперечный вид пилона \"{0}\" для листа \"{1}\"",
        //            ViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX, PylonViewSheet.Name);
        //        #endregion
        //    }
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 2-2 - работа завершена";
        //    #endregion


        //    // Размещение поперечного сечения 3-3
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 3-3";
        //    #endregion
        //    if(TransverseViewThird.ViewElement != null) {
        //        string answer = TransverseViewThird.PlacePylonViewport(ViewModel._revitRepository.Document, this);
        //        if(answer.Length > 0) {
        //            ViewModel.Report = answer;
        //        }

        //        if(TransverseViewThird.ViewportElement is null) {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tРабота с размещением поперечного вида пилона 3-3 на листе \"{0}\" прервана", PylonViewSheet.Name);
        //            #endregion
        //            return;
        //        } else {
        //            #region Отчет
        //            ViewModel.Report = string.Format("\tПоперечный вид 3-3 пилона \"{0}\" успешно размещен на листе \"{1}\"", TransverseViewThird.ViewElement.Name, PylonViewSheet.Name);
        //            #endregion
        //        }


        //        // Рассчитываем и задаем корректную точку вставки третьего поперечного вида пилона
        //        coordinateX = 0;
        //        coordinateY = 0;

        //        if(TransverseViewSecond.ViewElement != null) {
        //            coordinateX = TransverseViewSecond.ViewportCenter.X;
        //            coordinateY = TransverseViewSecond.ViewportCenter.Y + TransverseViewSecond.ViewportHalfHeight
        //                + 0.005 + TransverseViewSecond.ViewportHalfHeight;
        //        } else if(TransverseViewSecond.ViewElement == null && TransverseViewFirst.ViewElement != null) {
        //            coordinateX = TransverseViewFirst.ViewportCenter.X;
        //            coordinateY = TransverseViewFirst.ViewportCenter.Y + TransverseViewFirst.ViewportHalfHeight
        //                + 0.005 + TransverseViewFirst.ViewportHalfHeight;
        //        } else {
        //            coordinateX = -TitleBlockWidth + titleBlockOffset + GeneralView.ViewportHalfWidth * 2 + TransverseViewThird.ViewportHalfWidth;
        //            coordinateY = 0.015 + TransverseViewThird.ViewportHalfHeight;
        //        }

        //        XYZ newCenterThird = new XYZ(
        //            coordinateX,
        //            coordinateY,
        //            0);
        //        (TransverseViewThird.ViewportElement as Viewport).SetBoxCenter(newCenterThird);

        //        TransverseViewThird.ViewportCenter = newCenterThird;

        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПоперечный вид 3-3 пилона \"{0}\" спозиционирован", PylonViewSheet.Name);
        //        #endregion
        //    } else {
        //        #region Отчет
        //        ViewModel.Report = string.Format("\tПроизошла ошибка! Не найден поперечный вид пилона \"{0}\" для листа \"{1}\"",
        //            ViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + PylonKeyName + ViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX, PylonViewSheet.Name);
        //        #endregion
        //    }
        //    #region Отчет
        //    ViewModel.Report = "      Поперечный вид 3-3 - работа завершена";
        //    #endregion

        //    #region Отчет
        //    ViewModel.Report = "Поперечные виды - работа завершена";
        //    #endregion
        //    return;
        //}






        // Метод для размещения спецификации армирования
        // Позиционирование - правый верхний угол



        internal void PlaceRebarSchedule() {
            #region Отчет
            ViewModel.Report = "Спецификация арматуры";
            #endregion
            // Ищем спеку арматуры пилона
            foreach(var item in ViewModel._revitRepository.AllScheduleViews) {
                ViewSchedule schedule = item as ViewSchedule;
                if(schedule == null) {
                    continue;
                }

                if(schedule.Name == ViewModel.REBAR_SCHEDULE_PREFIX + PylonKeyName + ViewModel.REBAR_SCHEDULE_SUFFIX) {
                    // Заполняем данные для задания
                    RebarSchedule.ViewElement = schedule;
                    #region Отчет
                    ViewModel.Report = string.Format("\tСпецификация арматуры пилона \"{0}\" для листа \"{1}\" успешно найдена",
                        ViewModel.REBAR_SCHEDULE_PREFIX + PylonKeyName + ViewModel.REBAR_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                    #endregion
                    break;
                }
            }
            // Проверка вдруг спека не найдена
            if(RebarSchedule.ViewElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не найдена спецификация арматуры пилона \"{0}\" для листа \"{1}\"",
                    ViewModel.REBAR_SCHEDULE_PREFIX + PylonKeyName + ViewModel.REBAR_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                #endregion
                return;
            }


            // Создаем в дефолтной точке спеку арматуры пилона
            string answer = RebarSchedule.PlaceScheduleViewport(ViewModel._revitRepository.Document, this);
            if(answer.Length > 0) {
                ViewModel.Report = answer;
            }

            if(RebarSchedule.ViewportElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tРабота с размещением спецификации арматуры на листе \"{0}\" прервана", PylonViewSheet.Name);
                #endregion
                return;
            } else {
                #region Отчет
                ViewModel.Report = string.Format("\tСпецификация арматуры пилона \"{0}\" успешно размещена на листе \"{1}\"", RebarSchedule.ViewElement.Name, PylonViewSheet.Name);
                #endregion
            }




            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                - RebarSchedule.ViewportHalfWidth * 2 - 0.0095,
                TitleBlockHeight - 0.032,
                0);
            (RebarSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            RebarSchedule.ViewportCenter = newCenter;
            #region Отчет
            ViewModel.Report = string.Format("\tСпецификация арматуры пилона \"{0}\" спозиционирована", PylonViewSheet.Name);
            ViewModel.Report = "Спецификация арматуры - работа завершена";
            #endregion

            return;
        }


        // Метод для размещения спецификации материалов
        // Позиционирование - правый верхний угол ниже спецификации армирования
        internal void PlaceMaterialSchedule() {
            #region Отчет
            ViewModel.Report = "Спецификация материалов";
            #endregion
            // Ищем спеку материалов пилона
            foreach(var item in ViewModel._revitRepository.AllScheduleViews) {
                ViewSchedule schedule = item as ViewSchedule;
                if(schedule == null) {
                    continue;
                }

                if(schedule.Name == ViewModel.MATERIAL_SCHEDULE_PREFIX + PylonKeyName + ViewModel.MATERIAL_SCHEDULE_SUFFIX) {
                    // Заполняем данные для задания
                    MaterialSchedule.ViewElement = schedule;
                    #region Отчет
                    ViewModel.Report = string.Format("\tСпецификация материалов пилона \"{0}\" для листа \"{1}\" успешно найдена",
                        ViewModel.MATERIAL_SCHEDULE_PREFIX + PylonKeyName + ViewModel.MATERIAL_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                    #endregion
                    break;
                }
            }
            // Проверка вдруг спека не найдена
            if(MaterialSchedule.ViewElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не найдена спецификация материалов пилона \"{0}\" для листа \"{1}\"", 
                    ViewModel.MATERIAL_SCHEDULE_PREFIX + PylonKeyName + ViewModel.MATERIAL_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                #endregion
                return;
            }

            // Создаем в дефолтной точке спеку материалов пилона
            string answer = MaterialSchedule.PlaceScheduleViewport(ViewModel._revitRepository.Document, this);
            if(answer.Length > 0) {
                ViewModel.Report = answer;
            }

            if(MaterialSchedule.ViewportElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tРабота с размещением спецификации материалов на листе \"{0}\" прервана", PylonViewSheet.Name);
                #endregion
                return;
            } else {
                #region Отчет
                ViewModel.Report = string.Format("\tСпецификация материалов пилона \"{0}\" успешно размещена на листе \"{1}\"", MaterialSchedule.ViewElement.Name, PylonViewSheet.Name);
                #endregion
            }



            // Рассчитываем и задаем корректную точку вставки спецификации материалов пилона
            XYZ newCenter = new XYZ(
                - MaterialSchedule.ViewportHalfWidth * 2 - 0.0095,
                TitleBlockHeight - RebarSchedule.ViewportHalfHeight * 2 - 0.02505,
                0);
            (MaterialSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            MaterialSchedule.ViewportCenter = newCenter;

            #region Отчет
            ViewModel.Report = string.Format("\tСпецификация материалов пилона \"{0}\" спозиционирована", PylonViewSheet.Name);
            ViewModel.Report = "Спецификация материалов - работа завершена";
            #endregion

            return;
        }


        //Метод для размещения ведомости деталей
        // Позиционирование - середина слева над легендой примечания
        internal void PlacePartsSchedule() {
            #region Отчет
            ViewModel.Report = "Ведомость деталей";
            #endregion
            // Ищем спеку деталей пилона
            foreach(var item in ViewModel._revitRepository.AllScheduleViews) {
                ViewSchedule schedule = item as ViewSchedule;
                if(schedule == null) {
                    continue;
                }

                if(schedule.Name == ViewModel.IFC_PARTS_SCHEDULE_PREFIX + PylonKeyName + ViewModel.IFC_PARTS_SCHEDULE_SUFFIX) {
                    // Заполняем данные для задания
                    IFCPartsSchedule.ViewElement = schedule;
                    #region Отчет
                    ViewModel.Report = string.Format("\tВедомость деталей пилона \"{0}\" для листа \"{1}\" успешно найдена",
                        ViewModel.IFC_PARTS_SCHEDULE_PREFIX + PylonKeyName + ViewModel.IFC_PARTS_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                    #endregion
                    break;
                }
            }
            // Проверка вдруг спека не найдена
            if(IFCPartsSchedule.ViewElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не найдена ведомость деталей пилона \"{0}\" для листа \"{1}\"", 
                    ViewModel.IFC_PARTS_SCHEDULE_PREFIX + PylonKeyName + ViewModel.IFC_PARTS_SCHEDULE_SUFFIX, PylonViewSheet.Name);
                #endregion
                return;
            }

            // Создаем в дефолтной точке спеку деталей пилона
            string answer = IFCPartsSchedule.PlaceScheduleViewport(ViewModel._revitRepository.Document, this);
            if(answer.Length > 0) {
                ViewModel.Report = answer;
            }

            if(IFCPartsSchedule.ViewportElement is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tРабота с размещением ведомости деталей на листе \"{0}\" прервана", PylonViewSheet.Name);
                #endregion
                return;
            } else {
                #region Отчет
                ViewModel.Report = string.Format("\tВедомость деталей пилона \"{0}\" успешно размещена на листе \"{1}\"", IFCPartsSchedule.ViewElement.Name, PylonViewSheet.Name);
                #endregion
            }



            // Рассчитываем и задаем корректную точку вставки спецификации деталей пилона
            XYZ newCenter = new XYZ(
                -UnitUtilsHelper.ConvertToInternalValue(10) - IFCPartsSchedule.ViewportHalfWidth * 2,
                UnitUtilsHelper.ConvertToInternalValue(100) + IFCPartsSchedule.ViewportHalfHeight * 2,
                0);

            (IFCPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            IFCPartsSchedule.ViewportCenter = newCenter;

            #region Отчет
            ViewModel.Report = string.Format("\tВедомость деталей пилона \"{0}\" спозиционирована", PylonViewSheet.Name);
            ViewModel.Report = "Ведомость деталей - работа завершена";
            #endregion

            return;
        }


        // Метод для размещения легенды примечания
        // Позиционирование - над штампом
        internal void PlaceLegend(RevitRepository revitRepository) {
            #region Отчет
            ViewModel.Report = "Примечания";
            #endregion
            // Проверка вдруг вид не найден
            if(MainViewModel.SelectedLegend is null) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не выбрана легенда примечания для листа \"{0}\"", PylonViewSheet.Name);
                #endregion
                return;
            }
            // Проверяем можем ли разместить на листе легенду
            if(!Viewport.CanAddViewToSheet(revitRepository.Document, PylonViewSheet.Id, MainViewModel.SelectedLegend.Id)) {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Нельзя разместить легенду примечания \"{0}\" на листе \"{1}\"", 
                    MainViewModel.SelectedLegend.Name, PylonViewSheet.Name);
                #endregion
                return;
            }


            // Размещаем легенду на листе
            Viewport viewPort = null;
            try {
                // Размещаем спецификацию арматуры пилона на листе
                viewPort = Viewport.Create(revitRepository.Document, PylonViewSheet.Id, MainViewModel.SelectedLegend.Id, LegendView.ViewportCenter);
            } catch {
                #region Отчет
                ViewModel.Report = string.Format("\tПроизошла ошибка! Не удалось разместить легенду примечания у {0}", PylonKeyName);
                #endregion
                return;
            }

            LegendView.ViewportElement = viewPort;


            // Задание правильного типа видового экрана
            ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
            foreach(ElementId typeId in typesOfViewPort) {
                ElementType type = revitRepository.Document.GetElement(typeId) as ElementType;
                if(type == null) {
                    continue;
                }

                if(type.Name == LegendView.ViewportTypeName) {
                    viewPort.ChangeTypeId(type.Id);
                    break;
                }
            }

            #region Отчет
            ViewModel.Report = string.Format("\tЛегенда примечаний пилона \"{0}\" спозиционирована", PylonViewSheet.Name);
            ViewModel.Report = "Примечания - работа завершена";
            #endregion

            return;
        }
    }
}
