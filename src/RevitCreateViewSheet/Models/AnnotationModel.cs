using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class AnnotationModel : IEntity, IEquatable<AnnotationModel> {
        private readonly AnnotationSymbol _annotationInstance;

        /// <summary>
        /// Конструирует модель существующей аннотации на листе
        /// </summary>
        /// <param name="sheetModel">Модель листа</param>
        /// <param name="annotationSymbol">Экземпляр аннотации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public AnnotationModel(
            SheetModel sheetModel,
            AnnotationSymbol annotationSymbol,
            ExistsEntitySaver entitySaver) {

            Sheet = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            _annotationInstance = annotationSymbol ?? throw new ArgumentNullException(nameof(annotationSymbol));
            AnnotationSymbolType = _annotationInstance.AnnotationSymbolType;
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            Location = (_annotationInstance.Location as LocationPoint)?.Point;
            FamilyName = AnnotationSymbolType.FamilyName;
            SymbolName = AnnotationSymbolType.Name;
            Exists = true;
        }

        /// <summary>
        /// Конструирует модель новой аннотации на листе
        /// </summary>
        /// <param name="sheetModel">Модель листа</param>
        /// <param name="annotationSymbolType">Типоразмер аннотации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public AnnotationModel(
            SheetModel sheetModel,
            AnnotationSymbolType annotationSymbolType,
            NewEntitySaver entitySaver) {

            Sheet = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            AnnotationSymbolType = annotationSymbolType ?? throw new ArgumentNullException(nameof(annotationSymbolType));
            Saver = entitySaver ?? throw new ArgumentNullException(nameof(entitySaver));
            FamilyName = AnnotationSymbolType.FamilyName;
            SymbolName = AnnotationSymbolType.Name;
            Exists = false;
        }

        public bool Exists { get; }

        public XYZ Location { get; set; }

        public SheetModel Sheet { get; }

        public string FamilyName { get; }

        public string SymbolName { get; }

        public IEntitySaver Saver { get; }

        public AnnotationSymbolType AnnotationSymbolType { get; }

        public bool TryGetExistId(out ElementId id) {
            if(Exists && _annotationInstance is null) {
                throw new InvalidOperationException();
            }
            id = Exists ? _annotationInstance.Id : null;
            return Exists;
        }

        public bool TryGetAnnotationInstance(out AnnotationSymbol instance) {
            if(Exists && _annotationInstance is null) {
                throw new InvalidOperationException();
            }
            instance = Exists ? _annotationInstance : null;
            return Exists;
        }

        public bool Equals(AnnotationModel other) {
            return other is not null
                && Sheet.Equals(other.Sheet)
                && (_annotationInstance?.Id == other._annotationInstance?.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AnnotationModel);
        }

        public override int GetHashCode() {
            int hashCode = -627852025;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(_annotationInstance?.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            return hashCode;
        }
    }
}
