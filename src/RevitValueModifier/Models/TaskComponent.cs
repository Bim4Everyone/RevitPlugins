namespace RevitValueModifier.Models {
    internal class TaskComponent {
        public TaskComponent() { }

        public string Prefix { get; set; }
        public RevitParameter RevitParam { get; set; }
        public string Suffix { get; set; }
    }
}
