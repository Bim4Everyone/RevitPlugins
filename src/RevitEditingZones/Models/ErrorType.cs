using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitEditingZones.ViewModels;

namespace RevitEditingZones.Models {
    internal class ErrorType : IEquatable<ErrorType>, IComparable<ErrorType>, IComparable {
        public static readonly ErrorType NotLinkedZones = new ErrorType(0) {
            Name = "Зоны не связаны с уровнем", Description = "Зоны не связаны с уровнем"
        };

        public static readonly ErrorType ZoneNotMatchViewPlan = new ErrorType(1) {
            Name = "Этаж зоны не соответствует этажу уровня", Description = "Этаж зоны не соответствует этажу уровня"
        };

        public static readonly ErrorType ZoneMatchWithSameLevels = new ErrorType(2) {
            Name = "К нескольким зонам привязан один уровень", Description = "К нескольким зонам привязан один уровень"
        };

        public static readonly ErrorType ZoneNotMatchNames = new ErrorType(3) {
            Name = "Имена зон не соответствуют именам уровней",
            Description = "Имена зон не соответствуют именам уровней"
        };
        
        public static readonly ErrorType Default = new ErrorType(int.MaxValue) {
            Name = "Ошибки отсутствуют",
            Description = "Ошибки отсутствуют"
        };

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

    internal static class ZonePlanExtensions {
        public static bool IsNotLinkedZones(this ZonePlanViewModel zonePlan) {
            return zonePlan.Level == null;
        }

        public static bool IsZoneNotMatchViewPlan(this ZonePlanViewModel zonePlan) {
            return !zonePlan.Level.LevelName.StartsWith(zonePlan.AreaPlanName + "_");
        }

        public static bool IsZoneMatchWithSameLevels(this ZonePlanViewModel zonePlan,
            IEnumerable<ZonePlanViewModel> zonePlans) {
            return zonePlans.Any(item => item.Level?.Level.Id == zonePlan.Level?.Level.Id);
        }

        public static bool IsZoneNotMatchNames(this ZonePlanViewModel zonePlan) {
            return !zonePlan.AreaName.Equals(zonePlan.Level?.LevelName);
        }
    }
}