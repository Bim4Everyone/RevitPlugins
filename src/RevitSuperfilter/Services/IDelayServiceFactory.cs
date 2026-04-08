using System;

namespace RevitSuperfilter.Services;

internal interface IDelayServiceFactory {
    IDelayService Create(int interval, Action action);
}
