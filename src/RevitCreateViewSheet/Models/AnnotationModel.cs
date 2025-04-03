using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreateViewSheet.Models {
    internal class AnnotationModel : IEntity, IEquatable<AnnotationModel> {
        private readonly AnnotationSymbol _annotationSymbol;
        private readonly AnnotationSymbolType _annotationType;

        /// <summary>
        /// Конструирует модель существующей аннотации на листе
        /// </summary>
        /// <param name="sheetModel">Модель листа</param>
        /// <param name="annotationSymbol">Экземпляр аннотации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public AnnotationModel(SheetModel sheetModel, AnnotationSymbol annotationSymbol) {
            Sheet = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            _annotationSymbol = annotationSymbol ?? throw new ArgumentNullException(nameof(annotationSymbol));
            Location = (_annotationSymbol.Location as LocationPoint)?.Point;
            FamilyName = annotationSymbol.AnnotationSymbolType.FamilyName;
            SymbolName = annotationSymbol.AnnotationSymbolType.Name;
            State = EntityState.Unchanged;
        }

        /// <summary>
        /// Конструирует модель новой аннотации на листе
        /// </summary>
        /// <param name="sheetModel">Модель листа</param>
        /// <param name="annotationSymbolType">Типоразмер аннотации</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public AnnotationModel(SheetModel sheetModel, AnnotationSymbolType annotationSymbolType) {
            Sheet = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            _annotationType = annotationSymbolType ?? throw new ArgumentNullException(nameof(annotationSymbolType));
            FamilyName = _annotationType.FamilyName;
            SymbolName = _annotationType.Name;
            State = EntityState.Added;
        }

        public EntityState State { get; private set; }

        public XYZ Location { get; set; }

        public SheetModel Sheet { get; }

        public string FamilyName { get; }

        public string SymbolName { get; }


        public void MarkAsDeleted() {
            State = EntityState.Deleted;
        }

        public void SaveChanges(RevitRepository repository) {
            if(State == EntityState.Deleted && _annotationSymbol is not null) {
                repository.DeleteElement(_annotationSymbol.Id);
            } else if(State == EntityState.Added && _annotationType is not null) {
                if(Location is null) {
                    throw new InvalidOperationException($"Перед сохранением необходимо назначить {nameof(Location)}");
                }
                repository.CreateAnnotation(Sheet.GetViewSheet(), _annotationType, Location);
                State = EntityState.Unchanged;
            }
        }

        public bool Equals(AnnotationModel other) {
            return other is not null
                && Sheet.Equals(other.Sheet)
                && (_annotationSymbol?.Id == other._annotationSymbol?.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AnnotationModel);
        }

        public override int GetHashCode() {
            int hashCode = -627852025;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(_annotationSymbol?.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<SheetModel>.Default.GetHashCode(Sheet);
            return hashCode;
        }
    }
}
