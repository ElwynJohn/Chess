using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

/* ##########################################################################

   The server processes messages in a separate thread that loops forever
   listening for messages and then sending responses. e.g.

   for (;;)
   {
     Message mess_in, mess_out;
     message_receive(&mess_in, ...); // Receive a message
     switch (mess_in.type)
     {
     case ...Request:
       mess_out.type = ...Reply;
       // Do something with the message
       break;
     }
     message_send(mess_out, ...); // Send the reply
   }

   ########################################################################## */

namespace Chess.Models
{
    /// <summary>Class for passing messages to and from another process.</summary>
    public class Message
    {
        // First 4 bytes indicate length of the message
        private int len;
        // Then we have  another 4 bytes to indicate the type
        private MessageType type;
        // 16 bytes for GUID
        private Guid guid = Guid.NewGuid();
        // Remaining bytes are for data
        private byte[] data;

        private static Mutex mtx_w = new Mutex();
        private static Mutex mtx_r = new Mutex();

        public static NamedPipeClientStream client_w = new NamedPipeClientStream("ChessIPC_Requests");
        public static NamedPipeClientStream client_r = new NamedPipeClientStream("ChessIPC_Replies");
        public static List<Message> requests = new List<Message>();
        public static Dictionary<Guid, Message> replies = new Dictionary<Guid, Message>();

        private static void message_send(Message mess)
        {
            mtx_w.WaitOne();

            if (!client_w.IsConnected)
                client_w.Dispose();

            client_w.Write(mess.len.ToByteArray<int>(), 0, sizeof(int));
            client_w.Write(((int)mess.type).ToByteArray<int>(), 0, sizeof(int));
            client_w.Write(mess.guid.ToByteArray(), 0, 16);
            if (mess.len > 0)
                client_w.Write(mess.data, 0, mess.len);

            Logger.Buffer = $"(MessageType: {mess.type}) (len: {mess.len}) (GUID: {mess.guid}) data: ";
            foreach (byte b in mess.data)
                Logger.Buffer += $"{(int)b} ";
            Logger.IWrite();

            mtx_w.ReleaseMutex();
        }

        private static Message message_receive()
        {
            mtx_r.WaitOne();

            if (!client_r.IsConnected)
                client_r.Dispose();

            byte[] message_len = new byte[4];
            byte[] message_type = new byte[4];
            byte[] message_guid = new byte[16];
            client_r.Read(message_len, 0, 4);
            client_r.Read(message_type, 0, 4);
            client_r.Read(message_guid, 0, 16);
            byte[] message_data = new byte[message_len.ToUInt32()];
            if (message_len.ToInt32() > 0)
                client_r.Read(message_data, 0, message_len.ToInt32());

            MessageType type = (MessageType)message_type.ConvertTo<int>();
            int len = BitConverter.ToInt32(message_len);

            Logger.Buffer = $"(MessageType: {type}) (len: {len}) (GUID: {new Guid(message_guid)}) data: ";
            foreach (byte b in message_data)
                Logger.Buffer += $"{(int)b} ";
            Logger.IWrite();

            Message rv = new Message(message_data, message_len.ToInt32(),
                    (MessageType)BitConverter.ToInt32(message_type))
            {
                guid = new Guid(message_guid),
            };

            mtx_r.ReleaseMutex();

            return rv;
        }

        public static Task replyThread = new Task(() =>
        {
            for (;;)
            {
                try
                {
                    Message mess = message_receive();
                    replies.Add(mess.guid, mess);
                }
                catch (Exception e)
                {
                    Logger.EWrite(e);
                    break;
                }
            }
        });

        public static Task requestThread = new Task(() =>
        {
            for (;;)
            {
                try
                {
                    while (requests.Count == 0)
                        Thread.Sleep(5);

                    message_send(requests[0]);
                    requests.RemoveAt(0);
                }
                catch (Exception e)
                {
                    Logger.EWrite(e);
                    break;
                }
            }
        });

        public enum MessageType : int
        {
            LegalMoveRequest,
            LegalMoveReply,
            MakeMoveRequest,
            MakeMoveReply,
            BestMoveRequest,
            BestMoveReply,
            BoardStateRequest,
            BoardStateReply,
            GetMovesRequest,
            GetMovesReply,
            SetBoardRequest,
            SetBoardReply,
            PromotionRequest,
            PromotionReply,
            IsInCheckRequest,
            IsInCheckReply,
            IsInCheckmateRequest,
            IsInCheckmateReply,
            IsInStalemateRequest,
            IsInStalemateReply,
        }

        public int Length
        {
            get => len;
            set => len = value;
        }

        public MessageType Type
        {
            get => type;
            set => type = value;
        }

        public byte[] Bytes
        {
            get => data;
            set
            {
                len = value.Length;
                data = new byte[len];
                data = value;
            }
        }

        public void Send()
        {
            requests.Add(this);
        }

        public void Receive()
        {
            Message? tmp;
            while (!replies.TryGetValue(this.guid, out tmp))
                Thread.Sleep(5);

            Debug.Assert(tmp != null);
            Debug.Assert(tmp.guid == this.guid);

            len = tmp.len;
            type = tmp.type;
            guid = tmp.guid;
            data = tmp.data;

            replies.Remove(tmp.guid);
        }

        public Message(MessageType type)
        {
            len = 1;
            data = new byte[len];
            Type = type;
        }

        public Message(byte[] data, int size, MessageType type) : this(type)
        {
            this.data = data;
            len = size;
        }
    }
}
