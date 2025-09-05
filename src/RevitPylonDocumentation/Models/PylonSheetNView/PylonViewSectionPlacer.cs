using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewSectionPlacer {
    // Имя типа видового экрана основных видов
    private readonly string _viewportTypeWithTitle = "Заголовок на листе";
    // Имя типа видового экрана перпендикулярных и поперечных видов
    private readonly string _viewportTypeWithNumber = "Сечение_Номер вида";

    // Смещение по горизонтали в дюймах слева, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по вертикали в дюймах сверху, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    // Смещение по вертикали в дюймах снизу, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameBottomOffset = UnitUtilsHelper.ConvertToInternalValue(15);

    // Отступ между видовыми экранами в дюймах, для корректного взаимного размещения 
    private readonly double _viewportOffset = UnitUtilsHelper.ConvertToInternalValue(10);


    internal PylonViewSectionPlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    // Метод для размещения основного вида пилона.
    // Позиционирование - левый верхний угол листа
    internal bool PlaceGeneralViewport() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.GeneralView.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralView.ViewportTypeName = _viewportTypeWithTitle;
            // Задаем номер видового экрана. В случае основного вида никуда не выводится на листе,
            // номер не должен совпадать с номером основного вида армирования
            SheetInfo.GeneralView.ViewportNumber = "100";
            SheetInfo.GeneralView.ViewportName =
                ViewModel.ViewSectionSettings.GeneralViewPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralViewSuffix;
        }

        // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralView, false)) {
            return false;
        }

        // Если пользователь выбрал создание основного или бокового вида каркаса, то нужна большая рамка А1
        var sels = ViewModel.SelectionSettings;
        if(sels.NeedWorkWithGeneralRebarView || sels.NeedWorkWithGeneralPerpendicularRebarView) {
            SheetInfo.SetTitleBlockSize(Repository.Document, 1, 1);
        } else if(SheetInfo.GeneralView.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
            // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
            SheetInfo.SetTitleBlockSize(Repository.Document, 2, 1);
        }

        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset;

        // Рассчитываем и задаем корректную точку вставки основного вида опалубки пилона, если есть другие виды
        PylonView refPylonView = null;
        if(SheetInfo.GeneralViewPerpendicularRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewPerpendicularRebar;
        } else if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                                                       + SheetInfo.GeneralView.ViewportHalfWidth;
        }

        var newCenter = new XYZ(
                newCenterX,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - _titleBlockFrameTopOffset,
                0);

        (SheetInfo.GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralView.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceGeneralRebarViewport() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.GeneralViewRebar.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralViewRebar.ViewportTypeName = _viewportTypeWithTitle;
            // Задаем номер видового экрана. В случае основного вида никуда не выводится на листе,
            // номер не должен совпадать с номером основного вида опалубки
            SheetInfo.GeneralViewRebar.ViewportNumber = "101";
            SheetInfo.GeneralViewRebar.ViewportName =
                ViewModel.ViewSectionSettings.GeneralRebarViewPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralRebarViewSuffix;
        }

        // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewRebar, true)) {
            return false;
        }

        // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
        if(SheetInfo.GeneralViewRebar.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
            SheetInfo.SetTitleBlockSize(Repository.Document, 2, 1);
        }

        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewRebar.ViewportHalfWidth
                                                       + _titleBlockFrameLeftOffset;

        //// Рассчитываем и задаем корректную точку вставки основного вида армирования пилона, если есть другие виды
        var newCenter = new XYZ(
                newCenterX,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewRebar.ViewportHalfHeight - _titleBlockFrameTopOffset,
                0);

        (SheetInfo.GeneralViewRebar.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralViewRebar.ViewportCenter = newCenter;
        return true;
    }

    // Метод для размещения основного вида пилона.
    // Позиционирование - левый верхний угол листа
    internal bool PlaceGeneralPerpendicularViewport() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.GeneralViewPerpendicular.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralViewPerpendicular.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс перпендикулярного вида опалубки на листе
            SheetInfo.GeneralViewPerpendicular.ViewportNumber = "4";
            SheetInfo.GeneralViewPerpendicular.ViewportName =
                ViewModel.ViewSectionSettings.GeneralViewPerpendicularPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralViewPerpendicularSuffix;
        }

        // Передаем основной перпендикулярный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewPerpendicular, true)) {
            return false;
        }
        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset;

        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида армирования пилона,
        // если размещен основной вид армирования или другие
        PylonView refPylonView = null;
        if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        } else if(SheetInfo.GeneralViewPerpendicularRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewPerpendicularRebar;
        } else if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                                                       + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth;
        }
        var newCenter = new XYZ(newCenterX,
                                SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewPerpendicular.ViewportHalfHeight
                                                           - _titleBlockFrameTopOffset,
                                0);

        (SheetInfo.GeneralViewPerpendicular.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralViewPerpendicular.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceGeneralPerpendicularRebarViewport() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.GeneralViewPerpendicularRebar.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralViewPerpendicularRebar.ViewportTypeName = _viewportTypeWithNumber;

            // Индекс перпендикулярного вида армирования на листе
            SheetInfo.GeneralViewPerpendicularRebar.ViewportNumber = "г";
            SheetInfo.GeneralViewPerpendicularRebar.ViewportName =
                ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularSuffix;
        }

        // Передаем основной перпендикулярный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewPerpendicularRebar, true)) {
            return false;
        }

        // Смещение в случае, если ни один другой видовой экран не будет найден на листе
        double defaultOffsetX = -2.5;
        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида армирования пилона
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewPerpendicularRebar.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset 
                                                       + defaultOffsetX;

        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида армирования пилона
        PylonView refPylonView = null;
        if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        }
        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                                                       + SheetInfo.GeneralViewPerpendicularRebar.ViewportHalfWidth;
        }
        var newCenter = new XYZ(newCenterX,
                                SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewPerpendicularRebar.ViewportHalfHeight 
                                                           - _titleBlockFrameTopOffset,
                                0);

        (SheetInfo.GeneralViewPerpendicularRebar.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralViewPerpendicularRebar.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceTransverseFirstViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewFirst.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewFirst.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс первого по высоте поперечного вида опалубки на листе
            SheetInfo.TransverseViewFirst.ViewportNumber = "1";
            SheetInfo.TransverseViewFirst.ViewportName = "";
        }

        // Передаем первый поперечный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewFirst, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-50);

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона,
        // если есть основной вид
        PylonView refPylonView = null;
        if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        } else if(SheetInfo.TransverseViewSecond.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewSecond;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = SheetInfo.TransverseViewFirst.ViewportHalfHeight + _titleBlockFrameBottomOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewFirst.ViewportCenter = newCenter;
        return true;
    }

    internal bool PlaceTransverseSecondViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewSecond.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewSecond.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс второго по высоте поперечного вида опалубки на листе
            SheetInfo.TransverseViewSecond.ViewportNumber = "2";
            SheetInfo.TransverseViewSecond.ViewportName = "";
        }

        // Передаем второй поперечный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewSecond, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-25);

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона,
        // если есть основной вид
        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = refPylonView.ViewportCenter.Y + refPylonView.ViewportHalfHeight
                                                       + SheetInfo.TransverseViewSecond.ViewportHalfHeight
                                                       + _viewportOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewSecond.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewSecond.ViewportCenter = newCenter;
        return true;
    }

    internal bool PlaceTransverseThirdViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewThird.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewThird.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс третьего по высоте поперечного вида опалубки на листе
            SheetInfo.TransverseViewThird.ViewportNumber = "3";
            SheetInfo.TransverseViewThird.ViewportName = "";
        }

        // Передаем третий поперечный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewThird, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(0);

        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.TransverseViewSecond.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewSecond;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                                                       + SheetInfo.TransverseViewThird.ViewportHalfWidth
                                                       + _viewportOffset;
            newCenterY = SheetInfo.TransverseViewThird.ViewportHalfHeight + _titleBlockFrameBottomOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewThird.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewThird.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceTransverseRebarFirstViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewFirstRebar.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewFirstRebar.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс первого по высоте поперечного вида армирования на листе
            SheetInfo.TransverseViewFirstRebar.ViewportNumber = "a";
            SheetInfo.TransverseViewFirstRebar.ViewportName = "";
        }

        // Передаем поперечный вид армирования пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewFirstRebar, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида армирования пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewFirstRebar.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-50);

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида армирования пилона,
        // если есть основной вид каркаса
        PylonView refPylonView = null;
        if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        } else if(SheetInfo.TransverseViewSecondRebar.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewSecondRebar;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = SheetInfo.TransverseViewFirstRebar.ViewportHalfHeight + _titleBlockFrameBottomOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewFirstRebar.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewFirstRebar.ViewportCenter = newCenter;
        return true;
    }

    internal bool PlaceTransverseRebarSecondViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewSecondRebar.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewSecondRebar.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс второго по высоте поперечного вида армирования на листе
            SheetInfo.TransverseViewSecondRebar.ViewportNumber = "б";
            SheetInfo.TransverseViewSecondRebar.ViewportName = "";
        }

        // Передаем поперечный вид армирования пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewSecondRebar, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида армирования пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewSecondRebar.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-25);

        // Рассчитываем и задаем корректную точку вставки второго поперечного вида армирования пилона,
        // если есть первый поперечный вид каркаса или аналоги
        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirstRebar.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirstRebar;
        } else if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = refPylonView.ViewportCenter.Y + refPylonView.ViewportHalfHeight
                                                       + SheetInfo.TransverseViewSecondRebar.ViewportHalfHeight
                                                       + _viewportOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewSecondRebar.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewSecondRebar.ViewportCenter = newCenter;
        return true;
    }

    internal bool PlaceTransverseRebarThirdViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseViewThirdRebar.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseViewThirdRebar.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс второго по высоте поперечного вида армирования на листе
            SheetInfo.TransverseViewThirdRebar.ViewportNumber = "в";
            SheetInfo.TransverseViewThirdRebar.ViewportName = "";
        }

        // Передаем поперечный вид армирования пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewThirdRebar, true)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида армирования пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseViewThirdRebar.ViewportHalfWidth 
                                                       + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(0);

        // Рассчитываем и задаем корректную точку вставки третьего поперечного вида армирования пилона,
        // если есть первый поперечный вид каркаса или аналоги
        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirstRebar.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirstRebar;
        } else if(SheetInfo.TransverseViewSecondRebar.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewSecondRebar;
        } else if(SheetInfo.GeneralViewRebar.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewRebar;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth 
                                                       + SheetInfo.TransverseViewThirdRebar.ViewportHalfWidth
                                                       + _viewportOffset;
            newCenterY = SheetInfo.TransverseViewThirdRebar.ViewportHalfHeight + _titleBlockFrameBottomOffset;
        }
        var newCenter = new XYZ(newCenterX, newCenterY, 0);

        (SheetInfo.TransverseViewThirdRebar.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewThirdRebar.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlacePylonViewport(ViewSheet viewSheet, PylonView pylonView, bool hideUnnecessaryCategories) {
        var doc = Repository.Document;
        // Проверяем можем ли разместить на листе видовой экран вида
        if(!Viewport.CanAddViewToSheet(doc, viewSheet.Id, pylonView.ViewElement.Id)) {
            return false;
        }

        pylonView.ViewElement.get_Parameter(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC).Set(100);
        // Размещаем сечение пилона на листе
        Viewport viewPort = null;
        try {
            viewPort = Viewport.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));
        } catch(Exception) {
            return false;
        }

        pylonView.ViewportElement = viewPort;
        viewPort.LookupParameter("Номер вида").Set(pylonView.ViewportNumber);
        viewPort.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(pylonView.ViewportName);

        // Задание правильного типа видового экрана
        var typesOfViewPort = viewPort.GetValidTypes();
        foreach(var typeId in typesOfViewPort) {
            if(doc.GetElement(typeId) is not ElementType type) {
                continue;
            }
            if(type.Name == pylonView.ViewportTypeName) {
                viewPort.ChangeTypeId(type.Id);
                break;
            }
        }
        SheetInfo.GetInfoAboutViewport(pylonView, viewPort, hideUnnecessaryCategories);

        // Задание правильного положения метки видового экрана
#if REVIT_2021_OR_LESS
                    
        //report += "Вы работаете в Revit 2020 или 2021, поэтому имя вида необходимо будет спозиционировать на листе самостоятельно.";
                    //report += string.Format("Вы работаете в Revit 2020 или 2021, поэтому метку имени вида \"{0}\" необходимо будет спозиционировать на листе самостоятельно" 
                    //+ Environment.NewLine, ViewElement.Name);
#else
        viewPort.LabelOffset = new XYZ(pylonView.ViewportHalfWidth, 2 * pylonView.ViewportHalfHeight, 0);
#endif
        return true;
    }
}
