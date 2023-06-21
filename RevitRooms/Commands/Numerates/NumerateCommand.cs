using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.XtraSpellChecker;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal abstract class NumerateCommand {
        private readonly RevitRepository _revitRepository;
        
        public NumerateCommand(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        
        public int Start { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        
        
        protected RevitParam RevitParam { get; set; }
        protected string TransactionName { get; set; }

        public void Numerate(SpatialElementViewModel[] spatialElements) {
            SpatialElementViewModel[] orderedElements = OrderElements(spatialElements);
            using(var transaction = _revitRepository.StartTransaction(TransactionName)) {
                int flatCount = Start;

                foreach(var element in orderedElements) {
                    flatCount = CountFlat(flatCount, element) ? ++flatCount : flatCount;
                    element.Element.SetParamValue(RevitParam, Prefix + flatCount + Suffix);
                }

                transaction.Commit();
            }
        }

        protected abstract bool CountFlat(int currentCount, SpatialElementViewModel spatialElement);
        protected abstract SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements);

        protected double GetDistance(SpatialElement element) {
            var point = (element.Location as LocationPoint)?.Point;
            return point == null
                ? 0 
                : Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        }
    }
}