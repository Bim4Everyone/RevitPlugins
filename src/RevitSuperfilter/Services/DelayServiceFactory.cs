using System;

namespace RevitSuperfilter.Services;

internal sealed class DelayServiceFactory : IDelayServiceFactory {
    public IDelayService Create(int interval, Action action) {
        return new DelayService(interval, action);
    }
}
