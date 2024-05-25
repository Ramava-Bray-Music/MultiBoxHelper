// Copyright (C) 2024 Ramava Bray
// Copyright (C) 2022 akira0245
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see https://github.com/Ramava-Bray-Music/MultiBoxHelper/blob/master/LICENSE.md.
// 
// This code was originally written by akira0245 and was originally used in the MidiBard project. Any usage of this code must prominently credit the author, akira0245, and indicate that it was originally used in the MidiBard project.

using MultiBoxHelper.Util;
using System.Buffers;

namespace MultiBoxHelper.Ipc;

static class IpcHandles
{
    [IpcHandle(MessageTypeCode.Hello)]
    private static void HandleHello(IpcEnvelope message)
    {
        // Currently does nothing?
        _ = new ArrayBufferWriter<byte>();
    }

    public static void SyncAllSettings()
    {
        var config = Plugin.Configuration.JsonSerialize();
        IpcEnvelope.Create(MessageTypeCode.SyncAllSettings, config).BroadCast();
    }

    [IpcHandle(MessageTypeCode.SyncAllSettings)]
    private static void HandleSyncAllSettings(IpcEnvelope message)
    {
        var str = message.StringData[0];
        var jsonDeserialize = str.JsonDeserialize<Settings.Configuration>();
        Plugin.UpdateConfiguration(jsonDeserialize);
    }

}
