using System.Threading.Tasks;

namespace yungleanlyrics.Interfaces
{
    public interface ITweetService {
        Task<string> Tweet(string text);
        Task<string> TweetReply(string tweetId, string text);
        bool ShouldTweet();
    }
}
