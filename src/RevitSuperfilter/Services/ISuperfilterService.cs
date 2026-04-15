using RevitSuperfilter.Models;
using RevitSuperfilter.ViewModels;

namespace RevitSuperfilter.Services;

internal interface ISuperfilterService : IElementIndexList {
    Selection Selection { get; }
    string DisplaySelection { get; }
    
    CategoriesViewModel CategoriesViewModel { get; }

    ISuperfilterService Build();
}
