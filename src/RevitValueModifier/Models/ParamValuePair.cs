namespace RevitValueModifier.Models {
    internal class ParamValuePair {
        public ParamValuePair(RevitParameter revitParameter, string value) {
            RevitParam = revitParameter;
            ParamValue = value;
        }

        public RevitParameter RevitParam { get; }
        public string ParamValue { get; }
    }
}
