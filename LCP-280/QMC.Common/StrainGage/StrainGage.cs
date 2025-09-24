using QMC.Common.Component;
using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    public class StrainGage : BaseComponent
    {
        #region Fields
        private double voltage = 0;
        private double zeroSetVoltage = 0;
        private StrainGageLookupTable lookupTable = new StrainGageLookupTable();

        private QmcLowPassFilter lowPassFilter = new QmcLowPassFilter();
        #endregion

        #region Properties
        public double Voltage => voltage;
        public double Pressure => GetPressure();
        public StrainGageLookupTable LookupTable => lookupTable;
        public new StrainGageConfig Config { get; private set; }
        #endregion

        #region Constructor
        public StrainGage(string name) : base(name)
        {
            Config = new StrainGageConfig(name);
        }
        #endregion

        #region Override Methods
        public override int Initialize()
        {
            try
            {
                if (!Config.Validate())
                    return -1;
                if (!LoadLookupTable(Config.LookupTableFilePath))
                    return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 0;
        }
        public override int Create()
        {
            return base.Create();
        }
        public override void Close()
        {
            base.Close();
        }
        #endregion

        #region Methods
        public void UpdateVoltage(double voltage)
        {
            this.voltage = voltage;
        }
        public void UpdateZeroSetVoltage(double voltage)
        {
            // low pass filter 적용
            double filteredVoltage = lowPassFilter.AddValue(voltage);
            this.zeroSetVoltage = filteredVoltage;
        }
        public void ClearLookupTable()
        {
            lookupTable.Clear();
        }
        public bool LoadLookupTable(string filePath)
        {
            return lookupTable.LoadFromFile(filePath);
        }
        public bool SaveLookupTable(string filePath)
        {
            return lookupTable.SaveToFile(filePath);
        }
        private double GetPressure()
        {
            if (lookupTable == null)
                return 0;
            if (lookupTable.items.Count == 0)
                return 0;

            return lookupTable.VoltageToPressure(voltage);
        }
        #endregion
    }
}
