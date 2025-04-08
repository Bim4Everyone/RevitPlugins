using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCreateViewSheet.Models {
    internal class ViewPortModel : IEntity, IEquatable<ViewPortModel> {
        private readonly Viewport _viewport;
        private readonly View _view;
        private ElementType _viewPortType;

        /// <summary>
        /// Создает модель размещенного на листе видового экрана
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="viewport">Видовой экран, размещенный на листе</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ViewPortModel(SheetModel sheet, Viewport viewport) {
            Sheet = sheet
                ?? throw new ArgumentNullException(nameof(sheet));
            _viewport = viewport
                ?? throw new ArgumentNullException(nameof(viewport));
            _view = _viewport.Document.GetElement(_viewport.ViewId) as View
                ?? throw new ArgumentNullException(nameof(viewport.ViewId));
            Location = viewport.GetBoxCenter();
            _viewPortType = viewport.GetElementType();
            Name = viewport.Document.GetElement(viewport.ViewId).Name;
            State = EntityState.Unchanged;
        }

        /// <summary>
        /// Создает модель нового видового экрана
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="view">Вид для размещения</param>
        /// <param name="viewPortType">Типоразмер видового экрана</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ViewPortModel(SheetModel sheet, View view, ElementType viewPortType) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _viewPortType = viewPortType ?? throw new ArgumentNullException(nameof(viewPortType));
            Name = view.Name;
            State = EntityState.Added;
        }


        public EntityState State { get; private set; }

        public XYZ Location { get; set; }

        public string Name { get; }

        public SheetModel Sheet { get; }

        public View View => _view;

        public ElementType ViewPortType {
            get => _viewPortType;
            set {
                _viewPortType = value;
                if(State != EntityState.Added) {
                    State = EntityState.Modified;
                }
            }
        }


        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _viewport is not null) {
                repository.DeleteElement(_viewport.Id);

            } else if(State == EntityState.Modified && _viewport is not null) {
                if(ViewPortType is null) {
                    throw new InvalidOperationException(
                        $"Перед сохранением измененного видового экрана необходимо назначить {nameof(ViewPortType)}");
                }
                repository.UpdateViewPort(_viewport, ViewPortType.Id);
                State = EntityState.Unchanged;

            } else if(State == EntityState.Added && _view is not null) {
                if(Location is null) {
                    throw new InvalidOperationException(
                        $"Перед сохранением нового видового экрана необходимо назначить {nameof(Location)}");
                }
                if(ViewPortType is null) {
                    throw new InvalidOperationException(
                        $"Перед сохранением нового видового экрана необходимо назначить {nameof(ViewPortType)}");
                }
                repository.CreateViewPort(Sheet.GetViewSheet().Id, _view.Id, ViewPortType.Id, Location);
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
