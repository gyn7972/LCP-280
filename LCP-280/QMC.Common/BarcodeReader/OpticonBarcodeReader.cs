using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.BarcodeReader
{
    public class OpticonBarcodeReader : BaseComponent
    {
        #region Field
        private SerialPort communicator;
        #endregion

        #region Property
        public new OpticonBarcodeReaderConfig Config { get; private set; }
        #endregion

        #region Constructor
        public OpticonBarcodeReader(string name) : base(name)
        {
            Config = new OpticonBarcodeReaderConfig($"{name}_config");
        }
        #endregion

        #region Override Method
        public override int Initialize()
        {
            return 0;
        }
        public override int Create()
        {
            return 0;
        }
        public override void Close()
        {

        }
        #endregion

        #region Method
        public int Read(out string data)
        {
            int ret = 0;
            data = "";
            return ret;
        }
        #endregion
    }
}
