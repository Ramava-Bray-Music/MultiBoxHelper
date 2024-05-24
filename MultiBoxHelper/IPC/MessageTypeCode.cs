using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.IPC;
public enum MessageTypeCode
{
    Hello = 1,
    Bye,
    Acknowledge,

    GetMaster,
    SetSlave,
    SetUnslave,

    SetOption = 100,
    ShowWindow,
    SyncAllSettings,
    Object,
    SyncPlayStatus,
    PlaybackSpeed,
    GlobalTranspose,
    MoveToTime,

    ErrPlaybackNull = 1000
}
