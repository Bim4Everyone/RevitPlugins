namespace RevitDeclarations.Models {
    internal class ParameterToolTip {
        // Настройки выгрузки
        public string ParamDocumentToolTip => 
            "Проект на основе которого выводится список параметров для выбора";
        public string LastConfigToolTip => 
            "Подгрузка параметров, которые использовались\n" +
            "при последнем запуске скрипта в этом проекте";
        public string CompanyConfigToolTip => "Подгрузка параметров А101";

        // Параметры фильтрации и группировки помещений
        public string FilterRoomsParamToolTip => 
            "Параметр для фильтрации помещений для выгрузки.";
        public string FilterRoomsValuesToolTip =>
            "Значения для фильтрации групп помещений по параметру выше.\n\n" +
            "Скрипт обрабатывает только те помещения, которые СОДЕРЖАТ одно из введенных значений.\n" +
            "Регистр не учитывается.\n\n" +
            "Для создания нового значения фильтрации введите текст и нажмите \"Добавить\".\n" +
            "Для удаления значения нажмите на него.";
        public string GroupBySectionParamApartToolTip =>
            "Параметр, по которому помещения группируются по секциям.\n\n" +
            "Если помещения имеют одинаковое значение этого параметра,\n" +
            "то считается, что они относятся к одной секции.";
        public string GroupBySectionParamCommerToolTip =>
            "Параметр, по которому группируются нежилые помещения в основные группы.\n\n" +
            "Внутри этих групп происходит дополнительная группировка по следующему параметру.";
        public string GroupBySectionParamPublicToolTip =>
            "Параметр, по которому помещения группируются по секциям.\n\n" +
            "Если помещения имеют одинаковое значение этого параметра,\n" +
            "то считается, что они относятся к одной секции.";
        public string GroupByGroupApartParamToolTip =>
            "Параметр, по которому помещения группируются по квартирам.\n\n" +
            "Если помещения на одном этаже имеют одинаковое значение этого параметра\n" +
            "и находятся в одной секции, то считается, что они относятся к одной квартире.";
        public string GroupByGroupCommerParamToolTip =>
            "Параметр, по которому помещения группируются внутри основных групп.\n\n" +
            "Помещения, сгруппированные по этому параметру объединяются в одну строку в декларации.";
        public string MultiStoreyApartParamToolTip =>
            "Параметр, по которому определяются двухуровневые квартиры.\n\n" +
            "Все помещения, имеющие одинаковое значения этого параметра,\n" +
            "будут считаться одной квартирой, даже если они находятся на разных уровнях.\n" +
            "Для одноуровневых квартир параметр можно не заполнять.";
        public string MultiStoreyCommerParamToolTip =>
            "Параметр, по которому определяются двухуровневые группы.\n\n" +
            "Все помещения, имеющие одинаковое значения этого параметра,\n" +
            "будут считаться одной группой, даже если они находятся на разных уровнях.\n" +
            "Для одноуровневых групп параметр можно не заполнять.";

        // Общие параметры
        public string DepartmentParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Назначение\"";
        public string LevelParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Этаж расположения\"";
        public string SectionParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер подъезда\"";
        public string BuildingParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер корпуса\"";        
        public string BuildingNumberParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер здания\"";        
        public string ConstrWorkNumberParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер ОКС\"";
        public string ApartNumParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер на площадке\"";
        public string RoomsHeightParamToolTip => 
            "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Высота потолков\"";
        public string ProjectNameToolTip => 
            "Значение, которое выводится в столбец декларации \"ИД Объекта\"";

        // Параметры квартир
        public string ApartAreaParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Общая площадь без пониж. коэффициента\"";
        public string ApartAreaCoefParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Общая площадь с пониж. коэффициентом\"";
        public string ApartAreaLivingParamToolTip => 
            "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Жилая площадь\"";
        public string ApartAreaNonSumParamToolTip => 
            "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Площадь квартиры без летних помещений\"";
        public string RoomsAmountParamToolTip => 
            "Целочисленный параметр, значение которого выводится\n" +
            "в столбец декларации \"Количество комнат\"";
        public string ApartFullNumParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Сквозной номер квартиры\"";

        // Параметры нежилых помещений
        public string ConditionalNumberParamToolTip =>
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Условный номер\"\n\n." +
            "В значение можно добавить префикс, для этого надо поставить галочку\n" +
            "и выбрать параметр для префикса.";
        public string ParkingSpaceClassParamToolTip =>
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Класс машино-места\"";
        public string CommercialAreaParamToolTip =>
            "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Общая площадь\".\n\n" +
            "Необходимо выбрать два параметра.\n" +
            "Один для случая, когда группа состоит из нескольких помещений,\n" +
            "другой для случая, когда группа состоит из одного помещения.";
        public string CommercialNameClassParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Наименование помещения\".\n\n" +
            "Необходимо выбрать два параметра.\n" +
            "Один для случая, когда группа состоит из нескольких помещений,\n" +
            "другой для случая, когда группа состоит из одного помещения.";

        // Параметры МОПов
        public string PublicAreaNumberParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"№ п/п\".\n\n" +
            "В значение можно добавить префикс, для этого надо поставить галочку\n" +
            "и выбрать параметр для префикса.";
        public string PublicAreaNameParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Вид помещения\"";
        public string PublicAreaPositionParamToolTip =>
            "Текстовые параметры, значения которых выводится\n" +
            "в столбец декларации \"Описание места расположения помещения\".\n\n" +
            "Значение выводится в формате \"Корпус <Корпус>, Секция <Секция>, <Этаж> этаж\".";
        public string PublicAreaDepartmentParamToolTip => 
            "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Назначение помещения\"";
        public string PublicAreaAreaParamToolTip =>
            "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Площадь\".\n\n" +
            "Необходимо выбрать два параметра. Один для случая,\n" +
            "когда группа состоит из нескольких помещений,\n" +
            "другой для случая, когда группа состоит из одного помещения.";

        // Параметры отдельных помещений
        public string RoomNameParamToolTip =>
            "Текстовый параметр, значение которого выводится\n" +
            " в столбец с наименованием помещения.\n\n" +
            "Значение заполняется в формате \"<Имя>_<Номер>\".";
        public string RoomNumberParamToolTip =>
            "Текстовый параметр, значение которого выводится\n" +
            " в столбец с номером помещения.\n\n" +
            "Значение заполняется в формате \"<Номер>\".";
        public string RoomAreaParamToolTip => 
            "Числовой параметр, значение которого выводится\n" +
            "в столбец с площадью помещения без коэффициента.";
        public string RoomAreaCoefParamToolTip => 
            "Числовой параметр, значение которого выводится\n" +
            "в столбец с площадью помещения с коэффициентом.";
    }
}
