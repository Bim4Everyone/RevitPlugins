namespace RevitArchitecturalDocumentation.Models.Options {
    internal sealed class MainOptions {
        public MainOptions(SheetOptions sheetOptions, ViewOptions viewOptions, SpecOptions specOptions) {
            SheetOpts = sheetOptions;
            ViewOpts = viewOptions;
            SpecOpts = specOptions;
        }

        public SheetOptions SheetOpts { get; set; }
        public ViewOptions ViewOpts { get; set; }
        public SpecOptions SpecOpts { get; set; }
    }
}
