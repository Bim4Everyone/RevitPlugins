using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class PublicAreasSettings : DeclarationSettings {
        private PublicAreasParamsVM _parametersVM;

        public override ParametersViewModel ParametersVM {
            get { return _parametersVM; }
            set {
                _parametersVM = (PublicAreasParamsVM) value;
            }
        }

        public bool AddPostfixToNumber => _parametersVM.AddPostfixToNumber;
        public override Parameter RoomAreaCoefParam => ParametersVM.SelectedRoomAreaParam;
    }
}
