using System;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models {
    internal class ViewHelper {
        /// <summary>
        /// Конструктор, применяемый при создании новых видов
        /// </summary>
        public ViewHelper(RevitRepository revitRepository, TreeReportNode report = null) {
            Repository = revitRepository;
            Report = report;
        }

        /// <summary>
        /// Конструктор, применяемый при анализе существующих видов
        /// </summary>
        public ViewHelper(ViewPlan viewPlan) {
            View = viewPlan;
            NameHelper = new ViewNameHelper(viewPlan);
        }

        public RevitRepository Repository { get; set; }
        public ViewPlan View { get; set; }
        public TreeReportNode Report { get; set; }
        public ViewNameHelper NameHelper { get; set; }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
        /// </summary>
        public ViewPlan GetView(string newViewName, Element visibilityScope = null, ViewFamilyType viewFamilyType = null, Level level = null, ViewPlan viewForDublicate = null) {

            if(newViewName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с видом! Передано некорректное имя для задания!");
                return null;
            }

            ViewPlan newViewPlan = Repository.FindViewByName(newViewName);
            // Если newViewPlan is null, значит вид с указанным именем не найден в проекте и его нужно создать
            if(newViewPlan is null) {
                Report?.AddNodeWithName($"Вид с именем \"{newViewName}\" не найден в проекте, приступаем к созданию!");

                if(level is null && viewForDublicate is null) {
                    Report?.AddNodeWithName($"❗               Произошла ошибка при создании вида! Не передано для задания уровень и область видимости!");
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
                Report?.AddNodeWithName($"Вид с именем \"{newViewName}\" успешно найден в проекте!");
                View = newViewPlan;
            }

            return newViewPlan;
        }



        /// <summary>
        /// Создает вид на указанном уровне, назначает тип вида и задает указанное имя
        /// </summary>
        public ViewPlan CreateView(string newViewName, ViewFamilyType viewFamilyType, Level level) {

            if(newViewName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании нового вида! Передано некорректное имя для задания!");
                return null;
            }
            if(viewFamilyType is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании нового вида! Не указан типоразмер вида!");
                return null;
            }
            if(level is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании нового вида! Не указан уровень, на котором нужно создать вид!");
                return null;
            }

            ViewPlan newViewPlan = null;
            try {
                newViewPlan = ViewPlan.Create(Repository.Document, viewFamilyType.Id, level.Id);
                Report?.AddNodeWithName($"Вид успешно создан!");
                Report?.AddNodeWithName($"Виду назначен тип {viewFamilyType.Name}!");
                newViewPlan.Name = newViewName;
                Report?.AddNodeWithName($"Задано имя: {newViewPlan.Name}");

            } catch(Exception) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании нового вида!");
            }
            View = newViewPlan;
            return newViewPlan;
        }


        /// <summary>
        /// Дублирует указанный вид и задает указанное имя
        /// </summary>
        public ViewPlan DublicateView(string newViewName, ViewPlan viewForDublicate) {

            if(newViewName.Length == 0) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при настройке вида! Передано некорректное имя для задания!");
                return null;
            }

            if(viewForDublicate is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при создании нового вида! Не указан вид, который нужно продублировать!");
                return null;
            }

            ViewPlan newViewPlan = null;
            try {
                ElementId newViewPlanId = viewForDublicate.Duplicate(ViewDuplicateOption.WithDetailing);
                newViewPlan = viewForDublicate.Document.GetElement(newViewPlanId) as ViewPlan;
                Report?.AddNodeWithName($"Вид успешно продублирован!");
                newViewPlan.Name = newViewName;
                Report?.AddNodeWithName($"Задано имя: {newViewName}");

            } catch(Exception) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при дублировании вида!");
            }
            View = newViewPlan;
            return newViewPlan;
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
        /// </summary>
        public void SetUpView(Element visibilityScope) {

            if(visibilityScope is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при работе с видом! Передана некорректная область видимости!");
                return;
            }

            if(View is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при настройке вида, не найде вид для работын!");
                return;
            }

            try {
                View.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(visibilityScope.Id);
                Report?.AddNodeWithName($"Задана область видимости: {visibilityScope.Name}");

                View.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(1);
                Report?.AddNodeWithName($"Задана образка аннотаций на виде");

                ViewCropRegionShapeManager cropManager = View.GetCropRegionShapeManager();
                double dim = UnitUtilsHelper.ConvertToInternalValue(3);
                cropManager.TopAnnotationCropOffset = dim;
                cropManager.BottomAnnotationCropOffset = dim;
                cropManager.LeftAnnotationCropOffset = dim;
                cropManager.RightAnnotationCropOffset = dim;
                Report?.AddNodeWithName($"Задано минимальное смещение обрезки аннотаций");

            } catch(Exception) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при настройке вида!");
            }
        }



        /// <summary>
        /// Размещает вид на листе
        /// </summary>
        public Viewport PlaceViewportOnSheet(ViewSheet viewSheet, ElementType viewportType) {

            // Если переданный лист или вид is null или вид.экран вида нельзя добавить на лист, то возвращаем null
            if(viewSheet is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при размещении вида! Лист для размещения не найден!");
                return null;
            }
            if(View is null) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при размещении вида! Вид для размещения не найден!");
                return null;
            }
            if(!Viewport.CanAddViewToSheet(Repository.Document, viewSheet.Id, View.Id)) {
                Report?.AddNodeWithName($"❗ Произошла ошибка при размещении вида! Нельзя разместить вид на листе!");
                return null;
            }

            // Размещаем план на листе в начальной точке, чтобы оценить габариты
            Viewport viewPort = Viewport.Create(Repository.Document, viewSheet.Id, View.Id, new XYZ(0, 0, 0));
            if(viewPort is null) {
                Report?.AddNodeWithName($"❗ Не удалось создать вид на листе!");
                return null;
            }
            Report?.AddNodeWithName($"Видовой экран успешно создан на листе!");

            if(viewportType != null) {
                viewPort.ChangeTypeId(viewportType.Id);
                Report?.AddNodeWithName($"Видовому экрану задан тип {viewportType.Name}!");
            }

            XYZ viewportCenter = viewPort.GetBoxCenter();
            Outline viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            // Ищем рамку листа
            FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) {
                Report?.AddNodeWithName($"❗ Не удалось найти рамку листа, она нужна для правильного расположения вида на листе!");
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
                titleBlockMinY + titleBlockHeight / 2,
                0);

            viewPort.SetBoxCenter(correctPosition);
            Report?.AddNodeWithName($"Вид успешно спозиционирован на листе!");

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(viewportHalfWidth * 0.9, viewportHalfHeight * 2, 0);
            Report?.AddNodeWithName($"Оглавление вида успешно спозиционировано на листе!");
#endif
            return viewPort;
        }
    }
}
