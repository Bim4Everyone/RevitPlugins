using System;

using RevitSleeves.Models;
using RevitSleeves.Models.Navigator;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Navigator;
internal class SleeveStatusFinder : ISleeveStatusFinder {
    private readonly RevitRepository _revitRepository;

    public SleeveStatusFinder(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }


    public SleeveStatus GetStatus(SleeveModel sleeve) {
        // TODO
        var rnd = new Random();
        return (SleeveStatus) rnd.Next(0, 3);
    }
}
