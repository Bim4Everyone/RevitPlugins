using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class EntitiesTracker {
        private readonly ObservableCollection<SheetModel> _aliveSheets;
        private readonly ObservableCollection<ViewPortModel> _aliveViewPorts;
        private readonly ObservableCollection<ScheduleModel> _aliveSchedules;
        private readonly ObservableCollection<AnnotationModel> _aliveAnnotations;
        private readonly HashSet<ElementId> _removedEntities;

        public EntitiesTracker() {
            _aliveSheets = [];
            _aliveViewPorts = [];
            _aliveSchedules = [];
            _aliveAnnotations = [];
            _removedEntities = [];

            AliveSheets = new ReadOnlyObservableCollection<SheetModel>(_aliveSheets);
            AliveViewPorts = new ReadOnlyObservableCollection<ViewPortModel>(_aliveViewPorts);
            AliveSchedules = new ReadOnlyObservableCollection<ScheduleModel>(_aliveSchedules);
            AliveAnnotations = new ReadOnlyObservableCollection<AnnotationModel>(_aliveAnnotations);
        }


        public ReadOnlyObservableCollection<SheetModel> AliveSheets { get; }

        public ReadOnlyObservableCollection<ViewPortModel> AliveViewPorts { get; }

        public ReadOnlyObservableCollection<ScheduleModel> AliveSchedules { get; }

        public ReadOnlyObservableCollection<AnnotationModel> AliveAnnotations { get; }


        public IReadOnlyCollection<ElementId> GetRemovedEntities() {
            return [.. _removedEntities];
        }

        public void AddToRemovedEntities(IEntity entity) {
            if(entity is SheetModel sheet) {
                if(sheet.TryGetExistId(out ElementId sheetId)) {
                    _removedEntities.Add(sheetId);
                }
                _aliveSheets.Remove(sheet);
                foreach(var viewPort in sheet.ViewPorts) {
                    if(viewPort.TryGetExistId(out ElementId viewPortId)) {
                        _removedEntities.Add(viewPortId);
                    }
                    _aliveViewPorts.Remove(viewPort);
                }
                foreach(var schedule in sheet.Schedules) {
                    if(schedule.TryGetExistId(out ElementId scheduleId)) {
                        _removedEntities.Add(scheduleId);
                    }
                    _aliveSchedules.Remove(schedule);
                }
                foreach(var annotation in sheet.Annotations) {
                    if(annotation.TryGetExistId(out ElementId annotationId)) {
                        _removedEntities.Add(annotationId);
                    }
                    _aliveAnnotations.Remove(annotation);
                }
            } else if(entity is ViewPortModel viewPort) {
                if(viewPort.TryGetExistId(out ElementId viewPortId)) {
                    _removedEntities.Add(viewPortId);
                }
                _aliveViewPorts.Remove(viewPort);

            } else if(entity is ScheduleModel schedule) {
                if(schedule.TryGetExistId(out ElementId scheduleId)) {
                    _removedEntities.Add(scheduleId);
                }
                _aliveSchedules.Remove(schedule);

            } else if(entity is AnnotationModel annotation) {
                if(annotation.TryGetExistId(out ElementId annotationId)) {
                    _removedEntities.Add(annotationId);
                }
                _aliveAnnotations.Remove(annotation);
            }
        }

        public bool AddAliveSheet(SheetModel sheet) {
            if(!sheet.Exists || sheet.Exists
                && sheet.TryGetExistId(out ElementId viewSheetId)
                && !_removedEntities.Contains(viewSheetId)) {

                _aliveSheets.Add(sheet);
                return true;
            }
            return false;
        }

        public bool AddAliveViewPort(ViewPortModel viewPort) {
            if(!viewPort.Exists || viewPort.Exists
                && viewPort.TryGetExistId(out ElementId viewPortId)
                && !_removedEntities.Contains(viewPortId)) {

                _aliveViewPorts.Add(viewPort);
                return true;
            }
            return false;
        }

        public bool AddAliveSchedule(ScheduleModel schedule) {
            if(!schedule.Exists || schedule.Exists
                && schedule.TryGetExistId(out ElementId scheduleId)
                && !_removedEntities.Contains(scheduleId)) {

                _aliveSchedules.Add(schedule);
                return true;
            }
            return false;
        }

        public bool AddAliveAnnotation(AnnotationModel annotation) {
            if(!annotation.Exists || annotation.Exists
                && annotation.TryGetExistId(out ElementId annotationId)
                && !_removedEntities.Contains(annotationId)) {

                _aliveAnnotations.Add(annotation);
                return true;
            }
            return false;
        }

        public int GetTrackedEntitiesCount() {
            return _removedEntities.Count
                + _aliveSheets.Count
                + _aliveViewPorts.Count
                + _aliveSchedules.Count
                + _aliveAnnotations.Count;
        }
    }
}
