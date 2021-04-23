using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charts {
    class DailyFloatValues {
        public DateTime EndDate { get; set;  }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public FloatSignal Signal { get; set; }
        public DateTime StartDate { get; set; }

        public string ToCsv () {
            return $"{EndDate:yyyy-MM-dd},{StartDate:yyyy-MM-dd},{High:F2},{Low:F2},{Signal}";
        }
    }

    enum FloatSignal {
        Buy,
        Neutral,
        Sell,
        Unknown
    }
}
