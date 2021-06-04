using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace dosymep.Async {
    internal class AsyncHelper {
        private static readonly TaskFactory _myTaskFactory = new
                 TaskFactory(CancellationToken.None, TaskCreationOptions.None,
                 TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func) {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            return _myTaskFactory.StartNew(delegate {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}