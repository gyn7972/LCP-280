using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Keithley
{
    /// <summary>
    /// Keithley 계측기와의 통신을 담당하는 클래스입니다. (NI-Visa 사용)
    /// </summary>
    public class KeithleyInstrumentCommunicator
    {
        #region Field & Property
        private MessageBasedSession session;
        private bool isConnected;

        public MessageBasedSession Session
        {
            get { return session; }
        }
        public bool IsConnected
        {
            get { return isConnected; }
        }
        #endregion

        #region Constructor
        public KeithleyInstrumentCommunicator()
        {
            this.session = null;
            this.isConnected = false;
        }
        #endregion

        #region Methods

        // Session Methods
        #region Session Methods
        public bool OpenSession(string resourceName)
        {
            bool result = false;
            try
            {
                CloseSession();

                ResourceManager resourceManager = new ResourceManager();
                
                this.session = (MessageBasedSession)resourceManager.Open(resourceName);
                this.session.SynchronizeCallbacks = true;
                this.session.TimeoutMilliseconds = 5000; // Set a timeout for operations
                if (this.session.ReadStatusByte() != StatusByteFlags.MessageAvailable)
                {
                    throw new InvalidOperationException("Failed to open session. No message available.");
                }

                this.isConnected = true;
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        public bool CloseSession()
        {
            bool result = false;
            try
            {
                if (this.session != null)
                {
                    this.session.Dispose();
                }
                this.isConnected = false;
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        #endregion

        // Communication Methods
        #region Communication Methods
        public bool Query(string writeText, ref string readText)
        {
            return Write(writeText) && Read(ref readText);
        }
        public bool Write(string writeText)
        {
            if (this.session == null || !this.isConnected)
                return false;

            bool result = false;
            try
            {
                string buffer = ReplaceCommonEscapeSequences(writeText);
                this.session.RawIO.Write(buffer);
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        public bool Read(ref string readText)
        {
            if (this.session == null || !this.isConnected)
                return false;

            bool result = false;
            try
            {
                readText = InsertCommonEscapeSequences(this.session.RawIO.ReadString());
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
                readText = "";
            }
            return result;
        }
        #endregion

        // Other Methods
        #region Other Methods
        private string ReplaceCommonEscapeSequences(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }
        private string InsertCommonEscapeSequences(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }
        #endregion

        #endregion
    }
}