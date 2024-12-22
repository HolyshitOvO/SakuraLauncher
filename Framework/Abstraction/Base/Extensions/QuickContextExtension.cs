using System.Collections.Generic;
using CandyLauncher.Abstraction.Action;

namespace CandyLauncher.Abstraction.Base
{
    public static class QuickContextExtension
    {
        public static void AddAsyncActions(this IQuickContext context, IEnumerable<AsyncActionUpdate> asyncActions)
        {
            foreach (AsyncActionUpdate asyncAction in asyncActions)
                context.AddAsyncAction(asyncAction);
        }
    }
}
