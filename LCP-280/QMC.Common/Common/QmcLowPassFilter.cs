using System;

namespace QMC.Common
{
    [Serializable]
    public class QmcLowPassFilter
    {
        double m_dCutoffFrequence;
        double m_dX;
        bool m_hasPrev = false;
        public QmcLowPassFilter()
        {
            m_dCutoffFrequence = 0.03;
            //m_dCutoffFrequence = 0;
            m_dX = 0;
        }
        public double CutoffFrequence
        {
            get { return m_dCutoffFrequence; }
            set
            {
                if (value > 0 && value < 0.5)
                {
                    m_dCutoffFrequence = value;
                }
            }
        }
        public void ResetValue(double dX)
        {
            this.m_dX = dX;
        }
        public double AddValue(double dValue)
        {
            if (!m_hasPrev)
            {
                ResetValue(dValue);
                m_hasPrev = true;
            }

            //if(m_dX == 0)
            //{
            //    m_dX = dValue;
            //}
            else
            { 
                m_dX = m_dX * (1 - m_dCutoffFrequence) + dValue * m_dCutoffFrequence;
            }

            return m_dX;
        }
        public double AddValue(int nValue)
        {
            return AddValue((double)nValue);
        }
        public double CurrentValue
        {
            get { return m_dX; }

        }
        public void SetCurrentValue(double dValue)
        {
            m_dX = dValue;
        }

        public override string ToString()
        {
            return String.Format("{0:0.000}", m_dX);
        }
    }

}
