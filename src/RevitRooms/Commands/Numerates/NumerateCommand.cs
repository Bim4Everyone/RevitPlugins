using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Comparators;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates;
internal abstract class NumerateCommand(RevitRepository revitRepository) {
    protected readonly IComparer<Element> _elementComparer = RevitElementComparer.ElementName;

    public int Start { get; set; }
    public string Prefix { get; set; }
    public string Suffix { get; set; }

    protected RevitParam RevitParam { get; set; }
    protected string TransactionName { get; set; }

    public void Numerate(SpatialElementViewModel[] spatialElements, 
                         IProgress<int> progress = default, 
                         CancellationToken cancellationToken = default) {
        var orderedElements = OrderElements(spatialElements);
        using(var transaction = revitRepository.Document.StartTransaction(TransactionName)) {
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
            : Math.Sqrt(point.X * point.X + point.Y * point.Y);
    }
}

internal enum NumMode {
    Reset,
    Increment,
    NotChange,
}
