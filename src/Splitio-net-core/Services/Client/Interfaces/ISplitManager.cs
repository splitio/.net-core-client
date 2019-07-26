using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Client.Interfaces
{
    public interface ISplitManager
    {
        List<SplitView> Splits();

        List<string> SplitNames();

        SplitView Split(string featureName);

        void BlockUntilReady(int blockMilisecondsUntilReady);
    }
}
