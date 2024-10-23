using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal class ElementOffsetFinder : IOutcomingTaskOffsetFinder<Element> {
        private readonly IOutcomingTaskOffsetFinder<Pipe> _pipeFinder;
        private readonly IOutcomingTaskOffsetFinder<Duct> _ductFinder;
        private readonly IOutcomingTaskOffsetFinder<Conduit> _conduitFinder;
        private readonly IOutcomingTaskOffsetFinder<CableTray> _cableTrayFinder;
        private readonly IOutcomingTaskOffsetFinder<FamilyInstance> _familyInstanceFinder;


        public ElementOffsetFinder(
            IOutcomingTaskOffsetFinder<Pipe> pipeFinder,
            IOutcomingTaskOffsetFinder<Duct> ductFinder,
            IOutcomingTaskOffsetFinder<Conduit> conduitFinder,
            IOutcomingTaskOffsetFinder<CableTray> cableTrayFinder,
            IOutcomingTaskOffsetFinder<FamilyInstance> familyInstanceFinder) {

            _pipeFinder = pipeFinder
                ?? throw new ArgumentNullException(nameof(pipeFinder));
            _ductFinder = ductFinder
                ?? throw new ArgumentNullException(nameof(ductFinder));
            _conduitFinder = conduitFinder
                ?? throw new ArgumentNullException(nameof(conduitFinder));
            _cableTrayFinder = cableTrayFinder
                ?? throw new ArgumentNullException(nameof(cableTrayFinder));
            _familyInstanceFinder = familyInstanceFinder
                ?? throw new ArgumentNullException(nameof(familyInstanceFinder));
        }


        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.FindHorizontalOffsetsSum(opening, pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.FindHorizontalOffsetsSum(opening, duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.FindHorizontalOffsetsSum(opening, conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.FindHorizontalOffsetsSum(opening, cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.FindHorizontalOffsetsSum(opening, familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.FindVerticalOffsetsSum(opening, pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.FindVerticalOffsetsSum(opening, duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.FindVerticalOffsetsSum(opening, conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.FindVerticalOffsetsSum(opening, cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.FindVerticalOffsetsSum(opening, familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }

        public double GetMaxHorizontalOffsetSum(Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.GetMaxHorizontalOffsetSum(pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.GetMaxHorizontalOffsetSum(duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.GetMaxHorizontalOffsetSum(conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.GetMaxHorizontalOffsetSum(cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.GetMaxHorizontalOffsetSum(familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }

        public double GetMinHorizontalOffsetSum(Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.GetMinHorizontalOffsetSum(pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.GetMinHorizontalOffsetSum(duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.GetMinHorizontalOffsetSum(conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.GetMinHorizontalOffsetSum(cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.GetMinHorizontalOffsetSum(familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }

        public double GetMaxVerticalOffsetSum(Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.GetMaxVerticalOffsetSum(pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.GetMaxVerticalOffsetSum(duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.GetMaxVerticalOffsetSum(conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.GetMaxVerticalOffsetSum(cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.GetMaxVerticalOffsetSum(familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }

        public double GetMinVerticalOffsetSum(Element mepElement) {
            if(mepElement is Pipe pipe) {
                return _pipeFinder.GetMinVerticalOffsetSum(pipe);
            } else if(mepElement is Duct duct) {
                return _ductFinder.GetMinVerticalOffsetSum(duct);
            } else if(mepElement is Conduit conduit) {
                return _conduitFinder.GetMinVerticalOffsetSum(conduit);
            } else if(mepElement is CableTray cableTray) {
                return _cableTrayFinder.GetMinVerticalOffsetSum(cableTray);
            } else if(mepElement is FamilyInstance familyInstance) {
                return _familyInstanceFinder.GetMinVerticalOffsetSum(familyInstance);
            } else {
                throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
            }
        }
    }
}
