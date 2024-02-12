using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class NumberingOrderViewModel : ElementViewModel<Element> {
        private int _order;

        public NumberingOrderViewModel(Element element, RevitRepository revitRepository)
            : base(element, revitRepository) {
            Order = Element.GetParamValueOrDefault<int>(ProjectParamsConfig.Instance.NumberingOrder, 0);
        }

        public int Order {
            get => _order;
            set => this.RaiseAndSetIfChanged(ref _order, value);
        }

        public void UpdateOrder() {
            Element.SetParamValue(ProjectParamsConfig.Instance.NumberingOrder, Order);
        }
    }
}
