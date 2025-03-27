using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitCreateViewSheet.Models {
    internal class SheetModel : IModel, IEquatable<SheetModel> {
        private readonly List<AnnotationModel> _annotations;
        private readonly List<ScheduleModel> _schedules;
        private readonly List<ViewPortModel> _viewPorts;
        private ViewSheet _viewSheet;
        private string _drawingSet;
        private string _sheetNumber;
        private string _name;
        private ElementId _titleBlockSymbolId;

        public SheetModel(ViewSheet viewSheet) {
            _viewSheet = viewSheet ?? throw new ArgumentNullException(nameof(viewSheet));
            _annotations = GetAnnotationModels(viewSheet);
            _schedules = GetSchedules(viewSheet);
            _viewPorts = GetViewPorts(viewSheet);

            _drawingSet = viewSheet.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints, string.Empty);
            _sheetNumber = viewSheet.GetParamValueOrDefault(SharedParamsConfig.Instance.StampSheetNumber, string.Empty);
            _name = viewSheet.GetParamValueOrDefault(BuiltInParameter.SHEET_NAME, string.Empty);
            _titleBlockSymbolId = GetTitleBlockSymbolId(viewSheet);
            State = EntityState.Unchanged;
        }

        public SheetModel() {
            _annotations = new List<AnnotationModel>();
            _schedules = new List<ScheduleModel>();
            _viewPorts = new List<ViewPortModel>();

            _drawingSet = string.Empty;
            _sheetNumber = string.Empty;
            _name = string.Empty;
            _titleBlockSymbolId = ElementId.InvalidElementId;
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public string AlbumBlueprints {
            get => _drawingSet;
            set {
                if(value != _drawingSet) {
                    _drawingSet = value;
                    State = EntityState.Modified;
                }
            }
        }

        public string SheetNumber {
            get => _sheetNumber;
            set {
                if(value != _sheetNumber) {
                    _sheetNumber = value;
                    State = EntityState.Modified;
                }
            }
        }

        public string Name {
            get => _name;
            set {
                if(value != _name) {
                    _name = value;
                    State = EntityState.Modified;
                }
            }
        }

        public ElementId TitleBlockSymbolId {
            get => _titleBlockSymbolId;
            set {
                if(value != _titleBlockSymbolId) {
                    _titleBlockSymbolId = value;
                    State = EntityState.Modified;
                }
            }
        }


        public ViewSheet GetViewSheet() {
            if(_viewSheet is not null) {
                return _viewSheet;
            } else {
                throw new InvalidOperationException("Необходимо сначала сохранить созданный лист");
            }
        }

        public IReadOnlyCollection<ViewPortModel> GetViewPorts() {
            return new ReadOnlyCollection<ViewPortModel>(
                _viewPorts.Where(v => v.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddViewPort(ViewPortModel viewPort) {
            _viewPorts.Add(viewPort);
            State = EntityState.Modified;
        }

        public void RemoveViewPort(ViewPortModel viewPort) {
            _viewPorts.FirstOrDefault(v => v.Equals(viewPort))?.MarkAsDeleted();
            State = EntityState.Modified;
        }

        public IReadOnlyCollection<ScheduleModel> GetSchedules() {
            return new ReadOnlyCollection<ScheduleModel>(_schedules
                .Where(s => s.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddSchedule(ScheduleModel schedule) {
            _schedules.Add(schedule);
            State = EntityState.Modified;
        }

        public void RemoveSchedule(ScheduleModel schedule) {
            _schedules.FirstOrDefault(s => s.Equals(schedule))?.MarkAsDeleted();
            State = EntityState.Modified;
        }

        public IReadOnlyCollection<AnnotationModel> GetAnnotations() {
            return new ReadOnlyCollection<AnnotationModel>(_annotations
                .Where(a => a.State != EntityState.Deleted)
                .ToArray());
        }

        public void AddAnnotation(AnnotationModel annotation) {
            _annotations.Add(annotation);
            State = EntityState.Modified;
        }

        public void RemoveAnnotation(AnnotationModel annotation) {
            _annotations.FirstOrDefault(a => a.Equals(annotation))?.MarkAsDeleted();
            State = EntityState.Modified;
        }

        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _viewSheet is not null) {
                repository.RemoveElement(_viewSheet.Id);

            } else if(State == EntityState.Modified && _viewSheet is not null) {
                repository.UpdateViewSheet(_viewSheet, this);
                SaveNestedItems(repository);
                State = EntityState.Unchanged;

            } else if(State == EntityState.Added
                || State == EntityState.Modified && _viewSheet is null) {
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
            var doc = viewSheet.Document;
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(AnnotationSymbol))
                .ToElements()
                .OfType<AnnotationSymbol>()
                .Select(a => new AnnotationModel(this, a))
                .ToList();
        }

        private List<ScheduleModel> GetSchedules(ViewSheet viewSheet) {
            var doc = viewSheet.Document;
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(ScheduleSheetInstance))
                .ToElements()
                .OfType<ScheduleSheetInstance>()
                .Select(s => new ScheduleModel(this, s))
                .ToList();
        }

        private List<ViewPortModel> GetViewPorts(ViewSheet viewSheet) {
            var doc = viewSheet.Document;
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfClass(typeof(Viewport))
                .ToElements()
                .OfType<Viewport>()
                .Select(v => new ViewPortModel(this, v))
                .ToList();
        }

        private ElementId GetTitleBlockSymbolId(ViewSheet viewSheet) {
            var doc = viewSheet.Document;
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .FirstElementId();
        }
    }
}
