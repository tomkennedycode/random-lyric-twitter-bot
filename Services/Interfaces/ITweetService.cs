using System.Threading.Tasks;

namespace yungleanlyrics.Interfaces
{
    public interface ITweetService {
        Task<string> Tweet(string text);
    }
}
