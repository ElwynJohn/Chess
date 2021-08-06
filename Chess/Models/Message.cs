using System;
using System.IO.Pipes;
using System.Diagnostics;

/* ##########################################################################
   Anytime you send a message you should immediately attempt to receive the
   response from the server.

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

   Note that a reply should always be sent even if that reply doesn't contain
   any actual data. For now, all transactions must send AT LEAST ONE BYTE.

   ########################################################################## */

namespace Chess.Models
{
    /// <summary>Class for passing messages to and from another process.</summary>
    public class Message
    {
        // First 4 bytes indicate length of the message
        private UInt32 len;
        // Then we have (hopefully) another 4 bytes to indicate the type
        private MessageType type;
        private byte[] data;

        // Mutex
        private static bool isBusy = false;

        public enum MessageType
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

        public UInt32 Length
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
                len = (uint)value.GetLength(0);
                data = new byte[len];
                data = value;
            }
        }

        public void Send(NamedPipeClientStream client)
        {
            while (isBusy)
                System.Threading.Thread.Sleep(10);
            isBusy = true;

            Logger.Buffer = ($"(MessageType: {Type}) (len: {Length}) data: ");
            foreach (byte b in Bytes)
                Logger.Buffer += $"{(int)b} ";
            Logger.IWrite();

            client.Write(BitConverter.GetBytes(len), 0, sizeof(int));
            client.Write(BitConverter.GetBytes((int)type), 0, sizeof(int));
            if (len > 0)
                client.Write(data, 0, (int)len);
            isBusy = false;
        }

        public void Receive(NamedPipeClientStream client)
        {
            while (isBusy)
                System.Threading.Thread.Sleep(10);
            isBusy = true;
            byte[] message_len = new byte[4];
            byte[] message_type = new byte[4];
            client.Read(message_len, 0, 4);
            Length = BitConverter.ToUInt32(message_len, 0);
            client.Read(message_type, 0, 4);
            Type = (MessageType)BitConverter.ToUInt32(message_type, 0);
            Bytes = new byte[len];
            if (len > 0)
                client.Read(data, 0, (int)len);

            Logger.Buffer = $"(MessageType: {Type}) (len: {Length}) data: ";
            foreach (byte b in Bytes)
                Logger.Buffer += $"{(int)b} ";
            Logger.IWrite();
            isBusy = false;
        }

        public Message(MessageType type)
        {
            len = 1;
            data = new byte[len];
            Type = type;
        }

        public Message(byte[] data, UInt32 size, MessageType type) : this(type)
        {
            this.data = data;
            len = size;
        }
    }
}
