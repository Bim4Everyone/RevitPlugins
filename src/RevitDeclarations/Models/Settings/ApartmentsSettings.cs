using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class ApartmentsSettings : DeclarationSettings {
        private ApartmentsParamsVM _parametersVM;

        public override ParametersViewModel ParametersVM { 
            get { return _parametersVM; }
            set {
                _parametersVM = (ApartmentsParamsVM) value;
            } 
        }

        public Parameter ApartmentFullNumberParam => _parametersVM.SelectedApartFullNumParam;
        public Parameter BuildingNumberParam => _parametersVM.SelectedBuildingNumberParam;
        public Parameter ConstrWorksNumberParam => _parametersVM.SelectedConstrWorksNumberParam;
        
        public Parameter ApartmentAreaCoefParam => _parametersVM.SelectedApartAreaCoefParam;
        public Parameter ApartmentAreaLivingParam => _parametersVM.SelectedApartAreaLivingParam;
        public Parameter ApartmentAreaNonSumParam => _parametersVM.SelectedApartAreaNonSumParam;
        public Parameter RoomsAmountParam => _parametersVM.SelectedRoomsAmountParam;
        public Parameter RoomsHeightParam => _parametersVM.SelectedRoomsHeightParam;
        public override Parameter RoomAreaCoefParam => _parametersVM.SelectedRoomAreaCoefParam;
    }
}
