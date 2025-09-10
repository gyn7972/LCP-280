using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Keithley
{
    /// <summary>
    /// Keithley 계측기와의 통신을 담당하는 클래스입니다. (NI-Visa 사용)
    /// </summary>
    public class KeithleyInstrumentCommunicator
    {
        #region Field
        private MessageBasedSession session;
        private bool isConnected;
        #endregion

        #region Property
        public MessageBasedSession Session { get => session; }
        public bool IsConnected { get => isConnected; }
        #endregion

        #region Constructor
        public KeithleyInstrumentCommunicator()
        {
            session = null;            
            isConnected = false;
        }

        private void OnServiceRequest(object sender, VisaEventArgs e)
        {
            try
            {
                var statusByte = session.ReadStatusByte();
                if ((statusByte & StatusByteFlags.MessageAvailable) != 0)
                {
                    OnReceived?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
            }
        }
        #endregion

        #region Event
        public event EventHandler OnSessionOpened;
        public event EventHandler OnSessionClosed;
        public event EventHandler OnReceived;
        #endregion

        #region Methods

        #region Open, Close Session
        public bool OpenSession(string resourceName)
        {
            bool result = false;
            try
            {
                CloseSession();

                using (var resourceManager = new ResourceManager())
                {
                    session = (MessageBasedSession)resourceManager.Open(resourceName);
                    session.SynchronizeCallbacks = true;
                    session.TimeoutMilliseconds = 1000;
                    session.ServiceRequest += OnServiceRequest;
                }
                
                isConnected = true;

                // Open Session Event
                OnSessionOpened?.Invoke(this, EventArgs.Empty);
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
            }
            return result;
        }

        public bool CloseSession()
        {
            bool result = false;
            try
            {
                if (session != null)
                {
                    session.Clear();

                    // Disable Service Request Event
                    session.DiscardEvents(EventType.AllEnabled);
                    session.DisableEvent(EventType.AllEnabled);
                    session.ServiceRequest -= OnServiceRequest;

                    // Close Session
                    session.Dispose();
                    OnSessionClosed?.Invoke(this, EventArgs.Empty);
                }

                isConnected = false;
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
            }
            return result;
        }
        #endregion

        #region Query, Write, Read
        public bool Query(string writeText, ref string readText)
        {
            return Write(writeText) && Read(ref readText);
        }
        public bool Write(string writeText)
        {
            if (session == null || !isConnected)
                return false;

            bool result = false;
            try
            {
                string buffer = ReplaceCommonEscapeSequences(writeText);
                session.RawIO.Write(buffer);
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
            }
            return result;
        }
        public bool Read(ref string readText)
        {
            if (session == null || !isConnected)
                return false;

            bool result = false;
            try
            {
                readText = InsertCommonEscapeSequences(session.RawIO.ReadString());
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
                readText = "";
                Log.Write(ex);
            }
            return result;
        }
        #endregion

        #region Common Escape Sequence
        private string ReplaceCommonEscapeSequences(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return s;
            //return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }
        private string InsertCommonEscapeSequences(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return s;
            //return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }
        #endregion

        #endregion
    }
}