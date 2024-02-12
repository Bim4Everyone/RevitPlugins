using System;

namespace RevitSetLevelSection.Models
{
    internal class MainBimBuildPart : IEquatable<MainBimBuildPart> {
        public static readonly MainBimBuildPart ARPart = new MainBimBuildPart() {Id = 0, Name = "АР"};
        public static readonly MainBimBuildPart KRPart = new MainBimBuildPart() {Id = 1, Name = "КР"};
        public static readonly MainBimBuildPart VisPart = new MainBimBuildPart() {Id = 2, Name = "ВИС"};
        public static readonly MainBimBuildPart KOORDPart = new MainBimBuildPart() {Id = 3, Name = "КООРД"};

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        #region IEquatable<MainBimBuildPart>

        public bool Equals(MainBimBuildPart other) {
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

            if(obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((MainBimBuildPart) obj);
        }

        public override int GetHashCode() {
            return Id;
        }

        public static bool operator ==(MainBimBuildPart left, MainBimBuildPart right) {
            return Equals(left, right);
        }

        public static bool operator !=(MainBimBuildPart left, MainBimBuildPart right) {
            return !Equals(left, right);
        }

        #endregion
    }
}