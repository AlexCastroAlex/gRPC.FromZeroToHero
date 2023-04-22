using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;

namespace ChatServer.Utils
{
    public class MessageQueue
    {
        private static readonly Queue<ReceivedMessage> _queue;

        static MessageQueue()  {
            _queue = new Queue<ReceivedMessage>();
        }

        public static void AddNewsToQueue(NewsFlash news)  {
            var msg = new ReceivedMessage();
            msg.Content = news.NewsItem;
            msg.User = "NewsBot";
            msg.MsgTime = Timestamp.FromDateTime(DateTime.UtcNow);
            _queue.Enqueue(msg);
        }

        public static ReceivedMessage GetNextMessage()  {
            return _queue.Dequeue();
        }  

        public static int GetMessagesCount()  {
            return _queue.Count;
        }  
    }
}
