namespace RevitCopyInteriorSpecs.Services
{
    internal class DefaultParamNameService
    {
        private readonly string _groupTypeParamName = "ФОП_Тип квартиры";
        private readonly string _levelParamName = "Уровень";
        private readonly string _levelShortNameParamName = "ФОП_Этаж";
        private readonly string _phaseParamName = "Стадия";
        private readonly string _firstDispatcherGroupingLevelParamName = "_Стадия Проекта";
        private readonly string _secondDispatcherGroupingLevelParamName = "_Группа Видов";
        private readonly string _thirdDispatcherGroupingLevelParamName = "Назначение вида";

        public string GetGroupType() => _groupTypeParamName;
        public string GetLevel() => _levelParamName;
        public string GetLevelShortName() => _levelShortNameParamName;
        public string GetPhase() => _phaseParamName;
        public string GetFirstDispatcherGroupingLevel() => _firstDispatcherGroupingLevelParamName;
        public string GetSecondDispatcherGroupingLevel() => _secondDispatcherGroupingLevelParamName;
        public string GetThirdDispatcherGroupingLevel() => _thirdDispatcherGroupingLevelParamName;
    }
}
