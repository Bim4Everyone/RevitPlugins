using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Services;
internal class LengthConverterService : LengthConverter, ILengthConverter {
    public new double ConvertFromInternal(double feetValue) {
        return base.ConvertFromInternal(feetValue);
    }

    public new double ConvertToInternal(double mmValue) {
        return base.ConvertToInternal(mmValue);
    }
}
