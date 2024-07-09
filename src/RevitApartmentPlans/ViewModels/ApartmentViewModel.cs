using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class ApartmentViewModel : BaseViewModel, IEquatable<ApartmentViewModel> {
        private readonly Apartment _apartment;

        public ApartmentViewModel(Apartment apartment) {
            _apartment = apartment ?? throw new System.ArgumentNullException(nameof(apartment));
        }


        public string Name => _apartment.Name;

        public string LevelName => _apartment.LevelName;

        private bool _isSelected;
        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }


        public bool Equals(ApartmentViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return _apartment.Name == other._apartment.Name
                && _apartment.LevelName == other._apartment.LevelName;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ApartmentViewModel);
        }

        public override int GetHashCode() {
            int hashCode = 1221721940;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LevelName);
            return hashCode;
        }

        public Apartment GetApartment() {
            return _apartment;
        }
    }
}
