using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class SheetModel : IEntity, IEquatable<SheetModel> {
        private ViewSheet _viewSheet;

        /// <summary>
        /// Создает модель существующего листа
        /// </summary>
        /// <param name="viewSheet">Лист</param>
        /// <param name="viewports">Видовые экраны на листе</param>
        /// <param name="schedules">Спецификации на листе</param>
        /// <param name="annotations">Аннотации на листе</param>
        /// <param name="entitySaver">Сервис для сохранения листа в модели Revit</param>
        /// <param name="titleBlockSymbol">Типоразмер основной надписи листа, если она есть</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public SheetModel(
            ViewSheet viewSheet,
            ICollection<Viewport> viewports,
            ICollection<ScheduleSheetInstance> schedules,
            ICollection<AnnotationSymbol> annotations,
            ExistsEntitySaver entitySaver,
            FamilySymbol titleBlockSymbol = default) {

            _viewSheet = viewSheet ?? throw new ArgumentNullException(nameof(viewSheet));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));

            ViewPorts = viewports?.Select(v => new ViewPortModel(this, v, entitySaver)).ToList()
                ?? throw new ArgumentNullException(nameof(viewports));
            Schedules = schedules?.Select(s => new ScheduleModel(this, s, entitySaver)).ToList()
                ?? throw new ArgumentNullException(nameof(schedules));
            Annotations = annotations?.Select(a => new AnnotationModel(this, a, entitySaver)).ToList()
                ?? throw new ArgumentNullException(nameof(annotations));
            TitleBlockSymbol = titleBlockSymbol;

            AlbumBlueprint = viewSheet.GetParamValueOrDefault(
                SharedParamsConfig.Instance.AlbumBlueprints, string.Empty);
            SheetNumber = viewSheet.GetParamValueOrDefault(
                BuiltInParameter.SHEET_NUMBER, string.Empty);
            SheetCustomNumber = viewSheet.GetParamValueOrDefault(
                SharedParamsConfig.Instance.StampSheetNumber, string.Empty);
            Name = viewSheet.GetParamValueOrDefault(
                BuiltInParameter.SHEET_NAME, string.Empty);
            InitialAlbumBlueprint = AlbumBlueprint;
            InitialSheetNumber = SheetNumber;
            InitialSheetCustomNumber = SheetCustomNumber;
            InitialName = Name;
            InitialTitleBlockSymbol = TitleBlockSymbol;
            Exists = true;
        }

        /// <summary>
        /// Создает модель нового листа
        /// </summary>
        /// <param name="titleBlockSymbol">Типоразмер основной надписи листа</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public SheetModel(FamilySymbol titleBlockSymbol, NewEntitySaver entitySaver) {
            TitleBlockSymbol = titleBlockSymbol ?? throw new ArgumentNullException(nameof(titleBlockSymbol));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            Annotations = [];
            Schedules = [];
            ViewPorts = [];

            AlbumBlueprint = string.Empty;
            SheetNumber = string.Empty;
            Name = string.Empty;
            InitialAlbumBlueprint = AlbumBlueprint;
            InitialSheetNumber = SheetNumber;
            InitialSheetCustomNumber = SheetCustomNumber;
            InitialName = Name;
            Exists = false;
        }


        public bool Exists { get; }

        public IEntitySaver Saver { get; }

        public string InitialAlbumBlueprint { get; }

        public string AlbumBlueprint { get; set; }

        public string InitialSheetNumber { get; }

        /// <summary>
        /// Системный номер листа
        /// </summary>
        public string SheetNumber { get; set; }

        public string InitialSheetCustomNumber { get; }

        /// <summary>
        /// Ш.Номер листа
        /// </summary>
        public string SheetCustomNumber { get; set; }

        public string InitialName { get; }

        public string Name { get; set; }

        public FamilySymbol InitialTitleBlockSymbol { get; }

        public FamilySymbol TitleBlockSymbol { get; set; }

        public ICollection<ViewPortModel> ViewPorts { get; }

        public ICollection<ScheduleModel> Schedules { get; }

        public ICollection<AnnotationModel> Annotations { get; }


        public bool TryGetExistId(out ElementId id) {
            if(Exists && _viewSheet is null) {
                throw new InvalidOperationException();
            }
            id = Exists ? _viewSheet.Id : null;
            return Exists;
        }

        public bool TryGetViewSheet(out ViewSheet viewSheet) {
            if(Exists && _viewSheet is null) {
                throw new InvalidOperationException();
            }
            viewSheet = _viewSheet;
            return viewSheet is not null;
        }

        /// <summary>
        /// Назначает ссылку на новый лист в модели ревит, если данный лист еще не существовал
        /// </summary>
        /// <param name="viewSheet">Лист в модели ревит, представляющий текущий SheetModel</param>
        /// <returns>True, если ссылка назначена, иначе False</returns>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public bool TrySetNewViewSheet(ViewSheet viewSheet) {
            if(Exists) {
                return false;
            }
            _viewSheet = viewSheet ?? throw new ArgumentNullException(nameof(viewSheet));
            return true;
        }

        public bool Equals(SheetModel other) {
            return other is not null
                && (ReferenceEquals(this, other)
                || _viewSheet?.Id == other._viewSheet?.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as SheetModel);
        }

        public override int GetHashCode() {
            return 1344307554 + EqualityComparer<ElementId>.Default.GetHashCode(_viewSheet?.Id);
        }

        public void SetContentLocations() {
            var origin = new XYZ();
            var annotationsOrigin = origin + new XYZ(0, 7, 0);
            XYZ annotationsIncrementer = new(0.5, 0, 0);
            foreach(var annotation in Annotations) {
                if(!annotation.Exists && annotation.Location is null) {
                    annotation.Location = annotationsOrigin;
                    annotationsOrigin += annotationsIncrementer;
                }
            }
            var schedulesOrigin = origin + new XYZ(0, 6, 0);
            XYZ schedulesIncrementer = new(5, 0, 0);
            foreach(var schedule in Schedules) {
                if(!schedule.Exists && schedule.Location is null) {
                    schedule.Location = schedulesOrigin;
                    schedulesOrigin += schedulesIncrementer;
                }
            }
            var viewsOrigin = origin + new XYZ(0, 5, 0);
            XYZ viewsIncrementer = new(5, 0, 0);
            foreach(var viewPort in ViewPorts) {
                if(!viewPort.Exists && viewPort.Location is null) {
                    viewPort.Location = viewsOrigin;
                    viewsOrigin += viewsIncrementer;
                }
            }
        }
    }
}
