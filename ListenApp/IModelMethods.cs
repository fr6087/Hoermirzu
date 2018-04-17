using System.Collections.Generic;
using System.Threading.Tasks;

namespace ListenToMe
{
    /// <summary>
    /// interface for containing the cortana methods
    /// </summary>
    internal interface IModelMethods
    {
        Task<List<string>> UpdatePhraseList(string phraseListName);
    }
}