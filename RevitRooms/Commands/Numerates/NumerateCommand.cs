using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using DevExpress.XtraSpellChecker;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;

using RevitRooms.Comparators;
using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal abstract class NumerateCommand {
        private readonly RevitRepository _revitRepository;

        protected readonly IComparer<string> _logicStringComparer = new LogicalStringComparer();
        protected readonly IComparer<Element> _elementComparer = new dosymep.Revit.Comparators.ElementComparer();

        public NumerateCommand(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public int Start { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        
        protected RevitParam RevitParam { get; set; }
        protected string TransactionName { get; set; }

        public void Numerate(SpatialElementViewModel[] spatialElements, IProgress<int> progress = default, CancellationToken cancellationToken = default) {
            SpatialElementViewModel[] orderedElements = OrderElements(spatialElements);
            using(var transaction = _revitRepository.StartTransaction(TransactionName)) {
                int flatCount = Start;

                int count = 0;
                foreach(var element in orderedElements) {
                    progress?.Report(++count);
                    cancellationToken.ThrowIfCancellationRequested();

                    var numMode = CountFlat(element);
                    if(numMode == NumMode.Reset) {
                        flatCount = Start;
                    } else if(numMode == NumMode.Increment) {
                        ++flatCount;
                    }

                    if(element.IsNumberFix) {
                        continue;
                    }

                    element.Element.SetParamValue(RevitParam, Prefix + flatCount + Suffix);
                }

                transaction.Commit();
            }
        }

        protected abstract NumMode CountFlat(SpatialElementViewModel spatialElement);

        protected abstract SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements);

        protected double GetDistance(SpatialElement element) {
            var point = (element.Location as LocationPoint)?.Point;
            return point == null
                ? 0
                : Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        }
    }

    internal enum NumMode {
        Reset,
        Increment,
        NotChange,
    }
}