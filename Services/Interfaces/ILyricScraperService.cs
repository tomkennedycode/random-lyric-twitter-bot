using System.Threading.Tasks;

namespace yungleanlyrics.Interfaces
{
    public interface ILyricScraperService {
        Task<string> CallUrl(string fullUrl);
        string ParseHTML(string html);
    }
}
