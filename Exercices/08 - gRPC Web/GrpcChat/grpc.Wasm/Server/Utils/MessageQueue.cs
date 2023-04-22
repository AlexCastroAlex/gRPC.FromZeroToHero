using ChatServer.Protos;
using Google.Protobuf.WellKnownTypes;

namespace grpc.Wasm.Server.Utils
{
    public class MessageQueue
    {
        private static readonly Queue<ReceivedMessage> _queue;

        static MessageQueue()  {
            _queue = new Queue<ReceivedMessage>();
        }


        public static ReceivedMessage GetNextMessage()  {
            return _queue.Dequeue();
        }  

        public static int GetMessagesCount()  {
            return _queue.Count;
        }  
    }
}
