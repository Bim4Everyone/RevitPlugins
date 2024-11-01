using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class CommercialSettings : DeclarationSettings {
        private CommercialParamsVM _parametersVM;

        public override ParametersViewModel ParametersVM {
            get { return _parametersVM; }
            set {
                _parametersVM = (CommercialParamsVM) value;
            }
        }

        public Parameter BuildingNumberParam => _parametersVM.SelectedBuildingNumberParam;
        public Parameter ConstrWorksNumberParam => _parametersVM.SelectedConstrWorksNumberParam;
        public Parameter RoomsHeightParam => _parametersVM.SelectedRoomsHeightParam;

        public Parameter GroupNameParam => _parametersVM.SelectedGroupNameParam;
        public bool AddPostfixToNumber => _parametersVM.AddPostfixToNumber;

        public override Parameter RoomAreaCoefParam => ParametersVM.SelectedRoomAreaParam;
    }
}
