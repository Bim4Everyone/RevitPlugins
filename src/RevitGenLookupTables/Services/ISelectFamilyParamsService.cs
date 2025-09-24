using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using RevitGenLookupTables.ViewModels;

namespace RevitGenLookupTables.Services;

internal interface ISelectFamilyParamsService {
    IReadOnlyCollection<FamilyParamViewModel> SelectedFamilyParams { get; }

    Task<bool?> ShowDialogAsync(
        IEnumerable<FamilyParamViewModel> familyParams,
        CancellationToken cancellationToken = default);
}
