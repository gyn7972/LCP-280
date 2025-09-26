using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;

namespace QMC.Common.StrainGage
{
    public class NIDAQResource
    {
        #region Fields
        private string deviceName = "";
        private List<string> aiPhysicalChannels = new List<string>();
        private List<string> aoPhysicalChannels = new List<string>();
        #endregion

        #region Properties
        public string DeviceName { get => deviceName; set => deviceName = value; }
        public List<string> AIPhysicalChannels => aiPhysicalChannels;
        public List<string> AOPhysicalChannels => aoPhysicalChannels;
        #endregion

        #region Constructor
        public NIDAQResource()
        {
        }
        #endregion
    }

    public static class NIDAQResourceManager
    {
        #region Method
        public static List<NIDAQResource> FindAll()
        {
            List<NIDAQResource> resources = new List<NIDAQResource>();
            try
            {
                // Get all available devices
                var system = DaqSystem.Local;
                foreach (var deviceName in system.Devices)
                {
                    var device = system.LoadDevice(deviceName);
                    NIDAQResource resource = new NIDAQResource();
                    resource.DeviceName = deviceName;
                    resource.AIPhysicalChannels.AddRange(device.AIPhysicalChannels);
                    resource.AOPhysicalChannels.AddRange(device.AOPhysicalChannels);
                    resources.Add(resource);
                }
            }
            catch (Exception)
            {
                resources.Clear();
            }
            return resources;
        }
        #endregion
    }
}
