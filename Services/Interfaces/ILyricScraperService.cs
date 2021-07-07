using System.Threading.Tasks;
using yungleanlyrics.Models;

namespace yungleanlyrics.Interfaces
{
    public interface ILyricScraperService {
        Task<string> CallUrl(string fullUrl);
        Song ParseHTML(string html);
        string GetSongLink(string html);
    }
}
