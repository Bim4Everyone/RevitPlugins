using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitEditingZones.Models {
    internal class ErrorType : IEquatable<ErrorType>, IComparable<ErrorType>, IComparable {
        public ErrorType(int id) {
            Id = id;
        }

        public int Id { get; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        #region IEquatable<ErrorType>

        public bool Equals(ErrorType other) {
            if(ReferenceEquals(null, other)) {
                return false;
            }

            if(ReferenceEquals(this, other)) {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) {
                return false;
            }

            if(ReferenceEquals(this, obj)) {
                return true;
            }

            if(obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ErrorType) obj);
        }

        public override int GetHashCode() {
            return Id;
        }

        public static bool operator ==(ErrorType left, ErrorType right) {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorType left, ErrorType right) {
            return !Equals(left, right);
        }

        #endregion

        #region IComparable<ErrorType>

        public int CompareTo(ErrorType other) {
            if(ReferenceEquals(this, other)) {
                return 0;
            }

            if(ReferenceEquals(null, other)) {
                return 1;
            }

            return Id.CompareTo(other.Id);
        }

        public int CompareTo(object obj) {
            return CompareTo(obj as ErrorType);
        }

        #endregion

        public override string ToString() {
            return Name;
        }
    }
}