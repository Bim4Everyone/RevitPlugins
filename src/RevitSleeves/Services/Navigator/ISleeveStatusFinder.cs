using RevitSleeves.Models.Navigator;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Navigator;
internal interface ISleeveStatusFinder {
    SleeveStatus GetStatus(SleeveModel sleeve);
}
