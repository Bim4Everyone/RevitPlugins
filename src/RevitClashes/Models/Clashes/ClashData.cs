using System;


namespace RevitClashDetective.Models.Clashes {
    /// <summary>
    /// Данные о коллизии <see cref="ClashModel"/>
    /// </summary>
    internal class ClashData : IEquatable<ClashData> {
        /// <summary>
        /// Точность сравнения объемов в кубических футах ~0.028 мм3
        /// </summary>
        private const double _precision = 0.000001;


        /// <summary>
        /// Конструирует объект информации о коллизии с заданными параметрами
        /// </summary>
        /// <param name="mainElementVolume">Объем первого элемента коллизии в единицах ревита (кубических футах)</param>
        /// <param name="otherElementVolume">Объем второго элемента коллизии в единицах ревита (кубических футах)</param>
        /// <param name="clashVolume">Объем пересечения элементов коллизии в единицах ревита (кубических футах)</param>
        public ClashData(double mainElementVolume, double otherElementVolume, double clashVolume) {
            MainElementVolume = mainElementVolume;
            OtherElementVolume = otherElementVolume;
            ClashVolume = clashVolume;
        }

        /// <summary>
        /// Конструирует объект информации о коллизии с параметрами по умолчанию
        /// </summary>
        public ClashData() : this(0, 0, 0) {
        }


        /// <summary>
        /// Объем пересечения <see cref="ClashModel.MainElement"/> c <see cref="ClashModel.OtherElement"/> 
        /// в единицах ревита (кубических футах)
        /// </summary>
        public double ClashVolume { get; }

        /// <summary>
        /// Объем <see cref="ClashModel.MainElement"/> в единицах ревита (кубических футах)
        /// </summary>
        public double MainElementVolume { get; }

        /// <summary>
        /// Объем <see cref="ClashModel.OtherElement"/> в единицах ревита (кубических футах)
        /// </summary>
        public double OtherElementVolume { get; }


        public override bool Equals(object obj) {
            return Equals(obj as ClashData);
        }

        public override int GetHashCode() {
            int hashCode = -1962954641;
            hashCode = hashCode * -1521134295 + ClashVolume.GetHashCode();
            hashCode = hashCode * -1521134295 + MainElementVolume.GetHashCode();
            hashCode = hashCode * -1521134295 + OtherElementVolume.GetHashCode();
            return hashCode;
        }

        public bool Equals(ClashData other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return Math.Abs(MainElementVolume - other.MainElementVolume) < _precision
                && Math.Abs(OtherElementVolume - other.OtherElementVolume) < _precision
                && Math.Abs(ClashVolume - other.ClashVolume) < _precision;
        }
    }
}
