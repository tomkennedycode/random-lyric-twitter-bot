using System.Threading.Tasks;
using System.Collections.Generic;

namespace yungleanlyrics.Interfaces
{
    public interface ILyricScraperService {
        Task<string> CallUrl(string fullUrl);
        string ParseHTML(string html);
        string GetSongLink(string html);
    }
}
