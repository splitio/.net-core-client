using Splitio.Domain;
using System.Collections.Generic;

namespace Splitio.Services.Cache.Interfaces
{
    public interface IImpressionsCache
    {
        void AddImpression(KeyImpression impression);

        List<KeyImpression> FetchAllAndClear();
    }
}
