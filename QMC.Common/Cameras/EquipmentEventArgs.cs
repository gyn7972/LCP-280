using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
	public class EquipmentEventArgs : EventArgs
	{
		private int m_Result;

		public int Result
		{
			get
			{
				return m_Result;
			}
			set
			{
				m_Result = value;
			}
		}
		public EquipmentEventArgs()
		{
		}
	}
}
