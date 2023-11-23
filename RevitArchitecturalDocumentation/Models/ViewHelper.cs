using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Autodesk.Revit.DB;

using RevitArchitecturalDocumentation.ViewModels;

namespace RevitArchitecturalDocumentation.Models {
    internal class ViewHelper {
        public ViewHelper(StringBuilder report, RevitRepository revitRepository) {
            Report = report;
            Repository = revitRepository;
        }

        public StringBuilder Report { get; set; }
        public RevitRepository Repository { get; set; }
        public ViewPlan View { get; set; }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
        /// </summary>
        public ViewPlan GetView(string newViewName, Element visibilityScope = null, ViewFamilyType viewFamilyType = null, Level level = null, ViewPlan viewForDublicate = null) {

            if(newViewName.Length == 0) {
                Report.AppendLine($"❗               Произошла ошибка при работе с видом! Передано некорректное имя для задания!");
                return null;
            }

            ViewPlan newViewPlan = Repository.FindViewByName(newViewName);
            // Если newViewPlan is null, значит вид с указанным именем не найден в проекте и его нужно создать
            if(newViewPlan is null) {
                Report.AppendLine($"                Вид с именем {newViewName} не найден в проекте, приступаем к созданию!");

                if(level is null && viewForDublicate is null) {
                    Report.AppendLine($"❗               Произошла ошибка при создании вида! Не передано для задания уровень и область видимости!");
                    return null;
                }

                // В зависимости от того, что передали дублируем вид или создаем с нуля
                if(viewForDublicate != null) {
                    DublicateView(newViewName, viewForDublicate);
                    SetUpView(visibilityScope);
                } else {
                    CreateView(newViewName, viewFamilyType, level);
                    SetUpView(visibilityScope);
                }

            } else {
                Report.AppendLine($"                Вид с именем {newViewName} успешно найден в проекте!");
                View = newViewPlan;
            }
                        
            return newViewPlan;
        }



        /// <summary>
        /// Создает вид на указанном уровне, назначает тип вида и задает указанное имя
        /// </summary>
        public ViewPlan CreateView(string newViewName, ViewFamilyType viewFamilyType, Level level) {

            if(newViewName.Length == 0) {
                Report.AppendLine($"❗               Произошла ошибка при создании нового вида! Передано некорректное имя для задания!");
                return null;
            }
            if(viewFamilyType is null) {
                Report.AppendLine($"❗               Произошла ошибка при создании нового вида! Не указан типоразмер вида!");
                return null;
            }
            if(level is null) {
                Report.AppendLine($"❗               Произошла ошибка при создании нового вида! Не указан уровень, на котором нужно создать вид!");
                return null;
            }

            ViewPlan newViewPlan;
            try {
                newViewPlan = ViewPlan.Create(Repository.Document, viewFamilyType.Id, level.Id);
                Report.AppendLine($"                Вид успешно создан!");
                Report.AppendLine($"                Виду назначен тип {viewFamilyType.Name}!");
                newViewPlan.Name = newViewName;
                Report.AppendLine($"                Задано имя: {newViewPlan.Name}");
            } catch(Exception) {
                Report.AppendLine($"❗               Произошла ошибка при создании нового вида!");
                return null;
            }

            View = newViewPlan;
            return newViewPlan;
        }


        /// <summary>
        /// Дублирует указанный вид и задает указанное имя
        /// </summary>
        public ViewPlan DublicateView(string newViewName, ViewPlan viewForDublicate) {

            if(newViewName.Length == 0) {
                Report.AppendLine($"❗               Произошла ошибка при настройке вида! Передано некорректное имя для задания!");
                return null;
            }

            if(viewForDublicate is null) {
                Report.AppendLine($"❗               Произошла ошибка при создании нового вида! Не указан вид, который нужно продублировать!");
                return null;
            }

            ViewPlan newViewPlan = null;
            try {
                ElementId newViewPlanId = viewForDublicate.Duplicate(ViewDuplicateOption.WithDetailing);
                newViewPlan = viewForDublicate.Document.GetElement(newViewPlanId) as ViewPlan;
                Report.AppendLine($"                Вид успешно продублирован!");
                newViewPlan.Name = newViewName;
                Report.AppendLine($"                Задано имя: {newViewName}");
            } catch(Exception) {
                Report.AppendLine($"❗               Произошла ошибка при дублировании вида!");
            }

            View = newViewPlan;
            return newViewPlan;
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
        /// </summary>
        public void SetUpView(Element visibilityScope) {

            if(visibilityScope is null) {
                Report.AppendLine($"❗               Произошла ошибка при работе с видом! Передана некорректная область видимости!");
                return;
            }

            if(View is null) {
                Report.AppendLine($"❗               Произошла ошибка при настройке вида, вид для работы не найден!");
                return;
            }

            try {
                View.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(visibilityScope.Id);
                Report.AppendLine($"                Задана область видимости: {visibilityScope.Name}");

                View.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(1);
                Report.AppendLine($"                Задана образка аннотаций на виде");

                ViewCropRegionShapeManager cropManager = View.GetCropRegionShapeManager();
                double dim = UnitUtilsHelper.ConvertToInternalValue(3);
                cropManager.TopAnnotationCropOffset = dim;
                cropManager.BottomAnnotationCropOffset = dim;
                cropManager.LeftAnnotationCropOffset = dim;
                cropManager.RightAnnotationCropOffset = dim;
                Report.AppendLine($"                Задано минимальное смещение обрезки аннотаций");

            } catch(Exception) {
                Report.AppendLine($"❗               Произошла ошибка при настройке вида!");
            }
        }



        /// <summary>
        /// Размещает вид на листе
        /// </summary>
        public Viewport PlaceViewportOnSheet(ViewSheet viewSheet, ElementType viewportType) {

            // Если переданный лист или вид is null или вид.экран вида нельзя добавить на лист, то возвращаем null
            if(viewSheet is null) {
                Report.AppendLine($"❗               Произошла ошибка при размещении вида! Лист для размещения не найден!");
                return null;
            }
            if(View is null) {
                Report.AppendLine($"❗               Произошла ошибка при размещении вида! Вид для размещения не найден!");
                return null;
            }
            if(!Viewport.CanAddViewToSheet(Repository.Document, viewSheet.Id, View.Id)) {
                Report.AppendLine($"❗               Произошла ошибка при размещении вида! Нельзя разместить вид на листе!");
                return null;
            }

            // Размещаем план на листе в начальной точке, чтобы оценить габариты
            Viewport viewPort = Viewport.Create(Repository.Document, viewSheet.Id, View.Id, new XYZ(0, 0, 0));
            if(viewPort is null) {
                Report.AppendLine($"❗       Не удалось создать вид на листе!");
                return null;
            }
            Report.AppendLine($"        Видовой экран успешно создан на листе!");

            if(viewportType != null) {
                viewPort.ChangeTypeId(viewportType.Id);
                Report.AppendLine($"        Видовому экрану задан тип {viewportType.Name}!");
            }

            XYZ viewportCenter = viewPort.GetBoxCenter();
            Outline viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            //double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            // Ищем рамку листа
            FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) {
                Report.AppendLine($"❗       Не удалось найти рамку листа, она нужна для правильного расположения вида на листе!");
                return null;
            }

            Repository.Document.Regenerate();

            // Получение габаритов рамки листа
            BoundingBoxXYZ boundingBoxXYZ = titleBlock.get_BoundingBox(viewSheet);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMinY = boundingBoxXYZ.Min.Y;
            double titleBlockMinX = boundingBoxXYZ.Min.X;

            XYZ correctPosition = new XYZ(
                titleBlockMinX + viewportHalfWidth,
                titleBlockHeight / 2 + titleBlockMinY,
                0);

            viewPort.SetBoxCenter(correctPosition);
            Report.AppendLine($"        Вид успешно спозиционирован на листе!");

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(0.142591947719928, 0.318344950433976, 0);
            Report.AppendLine($"        Оглавление вида успешно спозиционировано на листе!");
#endif

            return viewPort;
        }
    }
}
