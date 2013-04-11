using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UDPServer
{
    class EventHandlers
    {
        public static event EventHandler UpdateReceivedHandler;
        public static void UpdateReceived(string text) { UpdateReceivedHandler(text, EventArgs.Empty); }
        public static event EventHandler UpdateSendHandler;
        public static void UpdateSend(string text) { UpdateSendHandler(text, EventArgs.Empty); }
        public static event EventHandler ButtonStateHandler;
        public static void UpdateButtonState(int state) { ButtonStateHandler(state, EventArgs.Empty); }
    }
}
