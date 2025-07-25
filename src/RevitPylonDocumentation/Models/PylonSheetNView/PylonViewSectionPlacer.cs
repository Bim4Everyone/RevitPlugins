using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewSectionPlacer {
    // Имя типа видового экрана основных видов
    private readonly string _viewportTypeWithTitle = "Заголовок на листе";
    // Имя типа видового экрана перпендикулярных и поперечных видов
    private readonly string _viewportTypeWithNumber = "Сечение_Номер вида";

    // Смещение по горизонтали в дюймах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameLeftOffset = UnitUtilsHelper.ConvertToInternalValue(20);

    // Смещение по вертикали в дюймах, для размещаемых компонентов листа требуемое, чтобы они попали на лист
    private readonly double _titleBlockFrameTopOffset = UnitUtilsHelper.ConvertToInternalValue(15);


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
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralView)) {
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

        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth + _titleBlockFrameLeftOffset;

        // Рассчитываем и задаем корректную точку вставки основного вида пилон, если есть еще и перпендикулярный
        if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - _titleBlockFrameLeftOffset;
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
        if(SheetInfo.GeneralRebarView.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralRebarView.ViewportTypeName = _viewportTypeWithTitle;
            // Задаем номер видового экрана. В случае основного вида никуда не выводится на листе,
            // номер не должен совпадать с номером основного вида опалубки
            SheetInfo.GeneralRebarView.ViewportNumber = "101";
            SheetInfo.GeneralRebarView.ViewportName =
                ViewModel.ViewSectionSettings.GeneralRebarViewPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralRebarViewSuffix;
        }

        // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralRebarView)) {
            return false;
        }

        // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
        if(SheetInfo.GeneralRebarView.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
            SheetInfo.SetTitleBlockSize(Repository.Document, 2, 1);
        }

        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralRebarView.ViewportHalfWidth + _titleBlockFrameLeftOffset;

        // Рассчитываем и задаем корректную точку вставки основного вида армирования пилона, если есть другие виды
        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewPerpendicular;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                + SheetInfo.GeneralRebarView.ViewportHalfWidth;
        }
        var newCenter = new XYZ(
                newCenterX,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralRebarView.ViewportHalfHeight - _titleBlockFrameTopOffset,
                0);

        (SheetInfo.GeneralRebarView.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralRebarView.ViewportCenter = newCenter;
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
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewPerpendicular)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + _titleBlockFrameLeftOffset;

        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона, если размещен основной вид
        if(SheetInfo.GeneralView.ViewportElement != null) {
            newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - _titleBlockFrameLeftOffset;
        }

        var newCenter = new XYZ(
                newCenterX,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewPerpendicular.ViewportHalfHeight - _titleBlockFrameTopOffset,
                0);

        (SheetInfo.GeneralViewPerpendicular.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralViewPerpendicular.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceGeneralPerpendicularRebarViewport() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.GeneralRebarViewPerpendicular.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.GeneralRebarViewPerpendicular.ViewportTypeName = _viewportTypeWithNumber;

            // Индекс перпендикулярного вида армирования на листе
            SheetInfo.GeneralRebarViewPerpendicular.ViewportNumber = SheetInfo.TransverseRebarViewSecond.ViewportElement is null ? "б" : "в";
            SheetInfo.GeneralRebarViewPerpendicular.ViewportName =
                ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularPrefix
                + SheetInfo.PylonKeyName
                + ViewModel.ViewSectionSettings.GeneralRebarViewPerpendicularSuffix;
        }

        // Передаем основной перпендикулярный вид пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralRebarViewPerpendicular)) {
            return false;
        }

        // Смещение в случае, если ни один другой видовой экран не будет найден на листе
        double defaultOffsetX = -2.5;
        // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralRebarViewPerpendicular.ViewportHalfWidth + _titleBlockFrameLeftOffset + defaultOffsetX;

        // Рассчитываем и задаем корректную точку вставки основного вида армирования пилона, если есть основной вид каркаса
        PylonView refPylonView = null;
        if(SheetInfo.GeneralRebarView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralRebarView;
        } else if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralViewPerpendicular;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X + refPylonView.ViewportHalfWidth
                + SheetInfo.GeneralRebarView.ViewportHalfWidth;
        }
        var newCenter = new XYZ(
                newCenterX,
                SheetInfo.TitleBlockHeight - SheetInfo.GeneralRebarViewPerpendicular.ViewportHalfHeight - _titleBlockFrameTopOffset,
                0);

        (SheetInfo.GeneralRebarViewPerpendicular.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.GeneralRebarViewPerpendicular.ViewportCenter = newCenter;
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
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewFirst)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
        double generalViewX = 0;
        double generalViewPerpendicularX = 0;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-75);

        // Если видовой экран основного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralView.ViewportElement != null) {
            generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
        }

        // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
        }

        double newCenterX;
        // Определяем координату Х первого поперечного вида пилона
        if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewX > generalViewPerpendicularX
                ? generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth
                : generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;

        } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
            newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
        } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
        } else {
            // Когда обоих видовых экранов нет на листе
            newCenterX = SheetInfo.TitleBlockWidth / 2;
        }

        if(SheetInfo.TransverseViewSecond.ViewportElement != null) {
            newCenterY = SheetInfo.TransverseViewSecond.ViewportCenter.Y - SheetInfo.TransverseViewSecond.ViewportHalfHeight
                - SheetInfo.TransverseViewFirst.ViewportHalfHeight;
        }
        var newCenter = new XYZ(
                newCenterX,
                newCenterY,
                0);

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
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewSecond)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
        double generalViewX = 0;
        double generalViewPerpendicularX = 0;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-50);

        // Если видовой экран основного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralView.ViewportElement != null) {
            generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
        }

        // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
        }

        double newCenterX;
        // Определяем координату Х первого поперечного вида пилона
        if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewX > generalViewPerpendicularX
                ? generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth
                : generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
        } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
            newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
        } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
        } else {
            // Когда обоих видовых экранов нет на листе
            newCenterX = SheetInfo.TitleBlockWidth / 2;
        }

        if(SheetInfo.TransverseViewThird.ViewportElement != null) {
            newCenterY = SheetInfo.TransverseViewThird.ViewportCenter.Y - SheetInfo.TransverseViewThird.ViewportHalfHeight
                - SheetInfo.TransverseViewSecond.ViewportHalfHeight;
        }
        var newCenter = new XYZ(
                newCenterX,
                newCenterY,
                0);

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
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewThird)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
        double generalViewX = 0;
        double generalViewPerpendicularX = 0;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-25);

        // Если видовой экран основного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralView.ViewportElement != null) {
            generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
        }

        // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
        if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
        }

        double newCenterX;
        // Определяем координату Х первого поперечного вида пилона
        if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewX > generalViewPerpendicularX
                ? generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth
                : generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
        } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
            newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
        } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
            newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
        } else {
            // Когда обоих видовых экранов нет на листе
            newCenterX = SheetInfo.TitleBlockWidth / 2;
        }

        PylonView refPylonView = null;
        if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.TransverseViewSecond.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewSecond;
        }
        if(refPylonView is null) {
            newCenterY = SheetInfo.TitleBlockHeight - SheetInfo.TransverseViewThird.ViewportHalfHeight - _titleBlockFrameTopOffset;
        }
        var newCenter = new XYZ(
                newCenterX,
                newCenterY,
                0);

        (SheetInfo.TransverseViewThird.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseViewThird.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceTransverseRebarFirstViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseRebarViewFirst.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseRebarViewFirst.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс первого по высоте поперечного вида армирования на листе
            SheetInfo.TransverseRebarViewFirst.ViewportNumber = "a";
            SheetInfo.TransverseRebarViewFirst.ViewportName = "";
        }

        // Передаем поперечный вид армирования пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseRebarViewFirst)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида армирования пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseRebarViewFirst.ViewportHalfWidth + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-50);

        // Рассчитываем и задаем корректную точку вставки основного вида армирования пилона, если есть основной вид каркаса
        PylonView refPylonView = null;

        if(SheetInfo.TransverseRebarViewSecond.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseRebarViewSecond;
        } else if(SheetInfo.GeneralRebarView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralRebarView;
        } else if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = refPylonView.ViewportCenter.Y - refPylonView.ViewportHalfHeight
                - SheetInfo.TransverseRebarViewFirst.ViewportHalfHeight;
        }
        var newCenter = new XYZ(
                newCenterX,
                newCenterY,
                0);

        (SheetInfo.TransverseRebarViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseRebarViewFirst.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlaceTransverseRebarSecondViewPort() {
        // Проверяем вдруг вид не создался
        if(SheetInfo.TransverseRebarViewSecond.ViewElement == null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.TransverseRebarViewSecond.ViewportTypeName = _viewportTypeWithNumber;
            // Индекс второго по высоте поперечного вида армирования на листе
            SheetInfo.TransverseRebarViewSecond.ViewportNumber = "б";
            SheetInfo.TransverseRebarViewSecond.ViewportName = "";
        }

        // Передаем поперечный вид армирования пилона в метод по созданию видов в (0.0.0)
        if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseRebarViewSecond)) {
            return false;
        }

        // Рассчитываем и задаем корректную точку вставки поперечного вида армирования пилона
        // Дефолтные значения координат для размещения видового экрана, используемое в случае, если 
        // референсные видовые экраны на листе не будут найдены
        double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.TransverseRebarViewSecond.ViewportHalfWidth + _titleBlockFrameLeftOffset;
        double newCenterY = UnitUtilsHelper.ConvertToInternalValue(-25);

        // Рассчитываем и задаем корректную точку вставки основного вида армирования пилона, если есть основной вид каркаса
        PylonView refPylonView = null;
        if(SheetInfo.GeneralRebarView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralRebarView;
        } else if(SheetInfo.TransverseViewFirst.ViewportElement != null) {
            refPylonView = SheetInfo.TransverseViewFirst;
        } else if(SheetInfo.GeneralView.ViewportElement != null) {
            refPylonView = SheetInfo.GeneralView;
        }

        if(refPylonView != null) {
            newCenterX = refPylonView.ViewportCenter.X;
            newCenterY = refPylonView.ViewportCenter.Y - refPylonView.ViewportHalfHeight
                - SheetInfo.TransverseRebarViewSecond.ViewportHalfHeight;
        }
        var newCenter = new XYZ(
                newCenterX,
                newCenterY,
                0);

        (SheetInfo.TransverseRebarViewSecond.ViewportElement as Viewport).SetBoxCenter(newCenter);
        SheetInfo.TransverseRebarViewSecond.ViewportCenter = newCenter;
        return true;
    }


    internal bool PlacePylonViewport(ViewSheet viewSheet, PylonView pylonView) {
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
        SheetInfo.GetInfoAboutViewport(pylonView, viewPort);

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
