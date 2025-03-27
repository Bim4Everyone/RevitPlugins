using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCreateViewSheet.Models {
    internal class ViewPortModel : IModel, IEquatable<ViewPortModel> {
        private readonly Viewport _viewport;
        private readonly View _view;
        private XYZ _location;

        public ViewPortModel(SheetModel sheet, Viewport viewport) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
            _location = viewport.GetBoxCenter();
            ViewPortTypeId = _viewport.GetTypeId();
            State = EntityState.Unchanged;
        }

        public ViewPortModel(SheetModel sheet, View view) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public SheetModel Sheet { get; }

        public ElementId ViewPortTypeId { get; set; }


        public void SetLocation(XYZ point) {
            _location = point ?? throw new ArgumentNullException(nameof(point));
        }

        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _viewport is not null) {
                repository.RemoveElement(_viewport.Id);

            } else if(State == EntityState.Modified && _viewport is not null) {
                if(ViewPortTypeId.IsNull()) {
                    throw new InvalidOperationException($"Сначала необходимо назначить {ViewPortTypeId}");
                }
                repository.UpdateViewPort(_viewport, ViewPortTypeId);
                State = EntityState.Unchanged;

            } else if(State == EntityState.Added && _view is not null) {
                if(_location is null) {
                    throw new InvalidOperationException("Сначала необходимо назначить Location видового экрана");
                }
                if(ViewPortTypeId.IsNull()) {
                    throw new InvalidOperationException($"Сначала необходимо назначить {ViewPortTypeId}");
                }
                repository.CreateViewPort(Sheet.GetViewSheet().Id, _view.Id, ViewPortTypeId, _location);
                State = EntityState.Unchanged;
            }
        }

        public bool Equals(ViewPortModel other) {
            return other is not null
                && Sheet.Equals(other.Sheet)
                && _viewport?.Id == other._viewport?.Id;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewPortModel);
        }

        public override int GetHashCode() {
            int hashCode = 1305361952;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(_viewport?.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            return hashCode;
        }
    }
}
