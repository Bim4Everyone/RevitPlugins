using Autodesk.Revit.DB;

namespace RevitExamplePlugin.Models {
    internal class CustomLocation {
        public CustomLocation(XYZ start, XYZ finish) {
            Start = start;
            Finish = finish;
        }

        public XYZ Start { get; }
        public XYZ Finish { get; }


        public override string ToString() {
            return $"Start: ({Start.X}; {Start.Y}; {Start.Z}); Finish: ({Finish.X}; {Finish.Y}; {Finish.Z})";
        }
    }
}
