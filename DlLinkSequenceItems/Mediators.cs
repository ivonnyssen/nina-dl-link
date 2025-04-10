using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    public enum Mediators {
        None,
        Camera,
        Focuser,
        FilterWheel,
        Telescope,
        Guider,
        Rotator,
        Dome,
        Switch,
        FlatDevice,
        WeatherData,
        SafetyMonitor
    }
}