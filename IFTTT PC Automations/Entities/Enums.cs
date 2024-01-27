using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFTTT_PC_Automations.Entities
{
    public enum EventType // when update, also update extensions -> ToReadableString si IsEventValueAvailable
    {
        BatteryBoE = 0,
        BatteryAoE = 1,
        Shutdown = 2
    }

    public enum InputType
    {
        Event = 0,
        Action = 1
    }

    public enum TagType
    {
        Date = 0,
        Time = 1,
        DateTime = 2,
        UserName = 3,
        PcName = 4,
        GUID = 5,
        Rnd10 = 6,
        Rnd50 = 7,
        Rnd100 = 8,
        Rnd1000 = 9
    }

    public enum ActionsType
    {
        Enabled = 0,
        Disabled = 1,
        All = 2,
        Specific = 3
    }

    public enum ViewType
    {
        Main = 0,
        Settings = 1,
        LogsStats = 2,
        AppErrors = 3
    }

    public enum UpdateType
    {
        Events = 0,
        Actions = 1,
        All = 2
    }

    public enum StringRepeatType
    {
        Replace = 0,
        Concat = 1,
        SBInsert = 2,
        SBAppendJoin = 3
    }

    public enum LogsType
    {
        Logs = 0,
        AppErrors = 1
    }
}
