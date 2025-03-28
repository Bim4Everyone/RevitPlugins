using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitCreateViewSheet.Models {
    internal class SheetModel : IEntity, IEquatable<SheetModel> {
        private readonly List<AnnotationModel> _annotations;
        private readonly List<ScheduleModel> _schedules;
        private readonly List<ViewPortModel> _viewPorts;
        private ViewSheet _viewSheet;
        private string _albumBlueprint;
        private string _sheetNumber;
        private string _name;
        private FamilySymbol _titleBlockSymbol;

        public SheetModel(
            ViewSheet viewSheet,
            FamilySymbol titleBlockSymbol) {

            if(viewSheet is null) {
                throw new ArgumentNullException(nameof(viewSheet));
            }
            if(titleBlockSymbol is null) {
                throw new ArgumentNullException(nameof(titleBlockSymbol));
            }

            _viewSheet = viewSheet;
            _titleBlockSymbol = titleBlockSymbol;
            _viewPorts = GetViewPortModels(_viewSheet);
            _schedules = GetScheduleModels(_viewSheet);
            _annotations = GetAnnotationModels(_viewSheet);

            _albumBlueprint = viewSheet.GetParamValueOrDefault(
                SharedParamsConfig.Instance.AlbumBlueprints, string.Empty);
            _sheetNumber = viewSheet.GetParamValueOrDefault(
                SharedParamsConfig.Instance.StampSheetNumber, string.Empty);
            _name = viewSheet.GetParamValueOrDefault(
                BuiltInParameter.SHEET_NAME, string.Empty);
            State = EntityState.Unchanged;
        }

        public SheetModel(FamilySymbol titleBlockSymbol) {
            if(titleBlockSymbol is null) {
                throw new ArgumentNullException(nameof(titleBlockSymbol));
            }

            _titleBlockSymbol = titleBlockSymbol;
            _annotations = [];
            _schedules = [];
            _viewPorts = [];

            _albumBlueprint = string.Empty;
            _sheetNumber = string.Empty;
            _name = string.Empty;
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public string AlbumBlueprint {
            get => _albumBlueprint;
            set {
                if(value != _albumBlueprint) {
                    _albumBlueprint = value;
                    SetModifiedState();
                }
            }
        }

        public string SheetNumber {
            get => _sheetNumber;
            set {
                if(value != _sheetNumber) {
                    _sheetNumber = value;
                    SetModifiedState();
                }
            }
        }

        public string Name {
            get => _name;
            set {
                if(value != _name) {
                    _name = value;
                    SetModifiedState();
                }
            }
        }

        public FamilySymbol TitleBlockSymbol {
            get => _titleBlockSymbol;
            set {
                if(value != _titleBlockSymbol) {
                    _titleBlockSymbol = value;
                    SetModifiedState();
                }
            }
        }


        public ViewSheet GetViewSheet() {
            if(_viewSheet is not null) {
                return _viewSheet;
            } else {
                throw new InvalidOperationException("Лист еще не создан в модели Revit");
            }
        }

        public IReadOnlyCollection<ViewPortModel> GetViewPorts() {
            return new ReadOnlyCollection<ViewPortModel>(
                _viewPorts.Where(v => v.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddViewPort(ViewPortModel viewPort) {
            _viewPorts.Add(viewPort);
            SetModifiedState();
        }

        public void RemoveViewPort(ViewPortModel viewPort) {
            var existingViewPort = _viewPorts.FirstOrDefault(v => v.Equals(viewPort));
            if(existingViewPort is not null) {
                existingViewPort.MarkAsDeleted();
                SetModifiedState();
            }
        }

        public IReadOnlyCollection<ScheduleModel> GetSchedules() {
            return new ReadOnlyCollection<ScheduleModel>(_schedules
                .Where(s => s.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddSchedule(ScheduleModel schedule) {
            _schedules.Add(schedule);
            SetModifiedState();
        }

        public void RemoveSchedule(ScheduleModel schedule) {
            var existingSchedule = _schedules.FirstOrDefault(s => s.Equals(schedule));
            if(existingSchedule is not null) {
                existingSchedule.MarkAsDeleted();
                SetModifiedState();
            }
        }

        public IReadOnlyCollection<AnnotationModel> GetAnnotations() {
            return new ReadOnlyCollection<AnnotationModel>(_annotations
                .Where(a => a.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddAnnotation(AnnotationModel annotation) {
            _annotations.Add(annotation);
            SetModifiedState();
        }

        public void RemoveAnnotation(AnnotationModel annotation) {
            var existingAnnotation = _annotations.FirstOrDefault(a => a.Equals(annotation));
            if(existingAnnotation is not null) {
                existingAnnotation.MarkAsDeleted();
                SetModifiedState();
            }
        }

        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _viewSheet is not null) {
                repository.DeleteElement(_viewSheet.Id);

            } else if(State == EntityState.Modified && _viewSheet is not null) {
                Validate();
                repository.UpdateViewSheet(_viewSheet, this);
                SaveNestedItems(repository);
                State = EntityState.Unchanged;

            } else if(State == EntityState.Added
                || State == EntityState.Modified && _viewSheet is null) {
                Validate();
                _viewSheet = repository.CreateViewSheet(this);
                SaveNestedItems(repository);
                State = EntityState.Unchanged;
            }
        }

        public bool Equals(SheetModel other) {
            return other is not null
                && _viewSheet?.Id == other._viewSheet?.Id;
        }

        public override bool Equals(object obj) {
            return Equals(obj as SheetModel);
        }

        public override int GetHashCode() {
            return 1344307554 + EqualityComparer<ElementId>.Default.GetHashCode(_viewSheet?.Id);
        }

        private void SaveNestedItems(RevitRepository repository) {
            foreach(var annotation in _annotations) {
                annotation.SaveChanges(repository);
            }
            foreach(var schedule in _schedules) {
                schedule.SaveChanges(repository);
            }
            foreach(var viewPort in _viewPorts) {
                viewPort.SaveChanges(repository);
            }
        }

        private List<AnnotationModel> GetAnnotationModels(ViewSheet viewSheet) {
            return new FilteredElementCollector(viewSheet.Document)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(AnnotationSymbol))
                .ToElements()
                .OfType<AnnotationSymbol>()
                .Select(a => new AnnotationModel(this, a))
                .ToList();
        }

        private List<ScheduleModel> GetScheduleModels(ViewSheet viewSheet) {
            return new FilteredElementCollector(viewSheet.Document)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(ScheduleSheetInstance))
                .ToElements()
                .OfType<ScheduleSheetInstance>()
                .Select(s => new ScheduleModel(this, s))
                .ToList();
        }

        private List<ViewPortModel> GetViewPortModels(ViewSheet viewSheet) {
            return new FilteredElementCollector(viewSheet.Document)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(Viewport))
                .ToElements()
                .OfType<Viewport>()
                .Select(v => new ViewPortModel(this, v))
                .ToList();
        }

        private void SetModifiedState() {
            if(State != EntityState.Added) {
                State = EntityState.Modified;
            }
        }

        private void Validate() {
            if(string.IsNullOrWhiteSpace(AlbumBlueprint)) {
                throw new InvalidOperationException($"Сначала необходимо назначить {nameof(AlbumBlueprint)}");
            }
            if(string.IsNullOrWhiteSpace(SheetNumber)) {
                throw new InvalidOperationException($"Сначала необходимо назначить {nameof(SheetNumber)}");
            }
            if(string.IsNullOrWhiteSpace(Name)) {
                throw new InvalidOperationException($"Сначала необходимо назначить {nameof(Name)}");
            }
            if(TitleBlockSymbol is null) {
                throw new InvalidOperationException($"Сначала необходимо назначить {nameof(TitleBlockSymbol)}");
            }
        }
    }
}
