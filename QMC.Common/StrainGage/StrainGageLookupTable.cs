using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    /// <summary>
    /// Strain Gage에서 얻은 전압을 압력으로 변환하기 위한 Lookup Table
    /// </summary>
    public class StrainGageLookupTable
    {
        #region Fields
        private List<(double voltage, double pressure)> lookupTable = new List<(double voltage, double pressure)>();
        #endregion

        #region Properties
        public IReadOnlyList<(double voltage, double pressure)> items => lookupTable.AsReadOnly();
        #endregion

        #region Constructor
        public StrainGageLookupTable()
        {
        }
        #endregion

        #region Methods
        public void Clear()
        {
            lookupTable.Clear();
        }

        public bool LoadFromFile(string filePath)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);

                lookupTable.Clear();

                foreach (var line in lines.Skip(1)) // 첫 줄은 헤더이므로 건너뜀
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(',');
                    if (parts.Length != 2)
                        continue;

                    if (double.TryParse(parts[0], out double voltage) &&
                        double.TryParse(parts[1], out double pressure))
                    {
                        lookupTable.Add((voltage, pressure));
                    }
                }

                lookupTable = lookupTable.OrderBy(point => point.voltage).ToList();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveToFile(string filePath)
        {
            try
            {
                var lines = new List<string>();
                lines.Add("Voltage,Pressure"); // CSV 헤더

                foreach (var (voltage, pressure) in lookupTable)
                {
                    lines.Add($"{voltage},{pressure}");
                }

                System.IO.File.WriteAllLines(filePath, lines, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void AddCalibrationPoint(double voltage, double pressure)
        {
            lookupTable.Add((voltage, pressure));
            lookupTable = lookupTable.OrderBy(point => point.voltage).ToList();
        }

        public double VoltageToPressure(double voltage)
        {
            // Implement interpolation logic to get pressure from voltage using the lookup table
            if (lookupTable.Count == 0)
                return 0;

            // Simple linear search for demonstration purposes
            for (int i = 0; i < lookupTable.Count - 1; i++)
            {
                if (voltage >= lookupTable[i].voltage && voltage <= lookupTable[i + 1].voltage)
                {
                    // Linear interpolation
                    double t = (voltage - lookupTable[i].voltage) / (lookupTable[i + 1].voltage - lookupTable[i].voltage);
                    return lookupTable[i].pressure + t * (lookupTable[i + 1].pressure - lookupTable[i].pressure);
                }
            }

            // If voltage is out of range, return closest value
            if (voltage < lookupTable[0].voltage)
                return lookupTable[0].pressure;
            else
                return lookupTable[lookupTable.Count - 1].pressure;
        }
        #endregion
    }
}
