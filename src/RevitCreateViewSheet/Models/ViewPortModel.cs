using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class ViewPortModel : IEntity, IEquatable<ViewPortModel> {
        private Viewport _viewport;

        /// <summary>
        /// Создает модель размещенного на листе видового экрана
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="viewport">Видовой экран, размещенный на листе</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ViewPortModel(SheetModel sheet, Viewport viewport, ExistsEntitySaver entitySaver) {
            Sheet = sheet
                ?? throw new ArgumentNullException(nameof(sheet));
            _viewport = viewport
                ?? throw new ArgumentNullException(nameof(viewport));
            View = _viewport.Document.GetElement(_viewport.ViewId) as View
                ?? throw new ArgumentNullException(nameof(viewport.ViewId));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            Location = viewport.GetBoxCenter();
            ViewPortType = viewport.GetElementType();
            Name = viewport.Document.GetElement(viewport.ViewId).Name;
            InitialViewPortType = ViewPortType;
            Exists = true;
        }

        /// <summary>
        /// Создает модель нового видового экрана
        /// </summary>
        /// <param name="sheet">Модель листа</param>
        /// <param name="view">Вид для размещения</param>
        /// <param name="viewPortType">Типоразмер видового экрана</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public ViewPortModel(SheetModel sheet, View view, ElementType viewPortType, NewEntitySaver entitySaver) {
            Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            View = view ?? throw new ArgumentNullException(nameof(view));
            ViewPortType = viewPortType ?? throw new ArgumentNullException(nameof(viewPortType));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            Name = view.Name;
            InitialViewPortType = ViewPortType;
            Exists = false;
        }


        public bool Exists { get; }

        public XYZ Location { get; set; }

        public string Name { get; }

        public SheetModel Sheet { get; }

        public View View { get; }

        public IEntitySaver Saver { get; }

        public ElementType InitialViewPortType { get; }

        public ElementType ViewPortType { get; set; }


        public bool TryGetExistId(out ElementId id) {
            if(Exists && _viewport is null) {
                throw new InvalidOperationException();
            }
            id = Exists ? _viewport.Id : null;
            return Exists;
        }

        public bool TryGetViewport(out Viewport viewport) {
            if(Exists && _viewport is null) {
                throw new InvalidOperationException();
            }
            viewport = _viewport;
            return viewport is not null;
        }

        /// <summary>
        /// Назначает ссылку на новый видовой экран в модели ревит, если данный видовой экран еще не существовал
        /// </summary>
        /// <param name="viewport">Видовой экран в модели ревит, представляющий текущий ViewPortModel</param>
        /// <returns>True, если ссылка назначена, иначе False</returns>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public bool TrySetNewViewSheet(Viewport viewport) {
            if(Exists) {
                return false;
            }
            _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
            return true;
        }

        public bool Equals(ViewPortModel other) {
            return other is not null
                && Sheet.Equals(other.Sheet)
                && View.Id == other.View.Id;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewPortModel);
        }

        public override int GetHashCode() {
            int hashCode = 1305361952;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(View.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            return hashCode;
        }
    }
}
