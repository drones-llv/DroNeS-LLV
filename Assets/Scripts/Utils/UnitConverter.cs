using System;
using System.Collections.Generic;

namespace Utils
{
    public static class UnitConverter
    {
        #region Conversions
        private static readonly Dictionary<Enum, float> _LengthConversions = new Dictionary<Enum, float>
        {
            {Length.m, 1f},
            {Length.km, 0.001f},
            {Length.mi, 0.0006213712f},
            {Length.yd, 1.0936132983f},
            {Length.ft, 3.280839895f},
            {Length.inch, 39.37007874f}
        };

        private static readonly Dictionary<Enum, float> _PowerConversions = new Dictionary<Enum, float>
        {
            {Power.W, 1f},
            {Power.kW, 0.001f},
            {Power.hpe, 1 / 746f}
        };

        private static readonly Dictionary<Enum, float> _MassConversions = new Dictionary<Enum, float>
        {
            {Mass.kg, 1f},
            {Mass.g, 1000f},
            {Mass.mt, 0.001f},
            {Mass.lt, 0.0009842073f},
            {Mass.sht, 0.0011023122f},
            {Mass.lb, 2.2046244202f},
            {Mass.oz, 35.273990723f}
        };

        private static readonly Dictionary<Enum, float> _EnergyConversions = new Dictionary<Enum, float>
        {
            {Energy.J, 1f},
            {Energy.kWh, 1/3.6e6f},
            {Energy.Wh, 1/3600},
            {Energy.BTU, 0.0009478672986f}
        };

        private static readonly Dictionary<Enum, float> _AreaConversions = new Dictionary<Enum, float>
        {
            {Area.sqm, 1f},
            {Area.sqmi, 0.0000003861f},
            {Area.sqyd, 1.1959900463f},
            {Area.sqft, 10.763910417f},
            {Area.sqin, 1550.0031f}
        };

        private static readonly Dictionary<Enum, float> _TimeConversions = new Dictionary<Enum, float>
        {
            {Chronos.s, 1f},
            {Chronos.min, 1 / 60f},
            {Chronos.h, 1 / 3600f},
            {Chronos.day, 1 / 24f / 3600f}
        };

        private static readonly Dictionary<Enum, float> _ForceConversions = new Dictionary<Enum, float>
        {
            {Force.N, 1f},
            {Force.kgf, 0.101972f},
            {Force.lbf, 0.224808f},
        };

        private static readonly Dictionary<Enum, float> _CurrentConversions = new Dictionary<Enum, float>
        {
            {Current.A, 1f},
            {Current.mA, 1000f}
        };

        private static readonly Dictionary<Enum, float> _ChargeConversions = new Dictionary<Enum, float>
        {
            {Charge.C, 1f},
            {Charge.mC, 1000f},
            {Charge.Ah, 1 / 3600f},
            {Charge.mAh, 1 / 3.6f}
        };

        private static readonly Dictionary<Enum, float> _VoltageConversions = new Dictionary<Enum, float>
        {
            {Voltage.V, 1f},
            {Voltage.mV, 1000f},
        };
        
        private static readonly Dictionary<Enum, float> _CurrencyFormat = new Dictionary<Enum, float>
        {
            {Currency.USD, 1f},
            {Currency.GBP, 1f}
        };

        private static readonly Dictionary<Type, Dictionary<Enum, float>> _Conversions = new Dictionary<Type, Dictionary<Enum, float>>
        {
            {typeof(Length), _LengthConversions},
            {typeof(Mass), _MassConversions},
            {typeof(Power), _PowerConversions},
            {typeof(Energy), _EnergyConversions},
            {typeof(Area), _AreaConversions},
            {typeof(Chronos), _TimeConversions},
            {typeof(Force), _ForceConversions},
            {typeof(Current), _CurrentConversions},
            {typeof(Charge), _ChargeConversions},
            {typeof(Voltage), _VoltageConversions},
            {typeof(Currency), _CurrencyFormat}
        };
        
        
        #endregion

        public static string Convert(Enum unit, float? input)
        {
            if (!_Conversions.TryGetValue(unit.GetType(), out Dictionary<Enum, float> k)) return "";
            
            input *= k[unit];
            return input?.ToString("0.00") + " " + unit;
        }

        public static float ConvertValue(Enum unit, float input)
        {
            if (_Conversions.TryGetValue(unit.GetType(), out Dictionary<Enum, float> k))
            {
                return input * k[unit];
            }
            return default;
        }
    }


}