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
// along with this program.  If not, see https://github.com/akira0245/MidiBard/blob/master/LICENSE.
// 
// This code is written by akira0245 and was originally used in the MidiBard project. Any usage of this code must prominently credit the author, akira0245, and indicate that it was originally used in the MidiBard project.

using MultiBoxHelper.Util;
using ProtoBuf;
using System;
using System.Diagnostics;

namespace MultiBoxHelper.Ipc;

[ProtoContract]
internal class IpcEnvelope
{
    public IpcEnvelope(MessageTypeCode messageType, byte[] data, params string[] stringData)
    {
        MessageType = messageType;
        BroadcasterId = (long)Service.ClientState.LocalContentId;
        Data = data;
        StringData = stringData;
    }
    private IpcEnvelope() { }

    public static IpcEnvelope Create<T>(MessageTypeCode messageType, T data) where T : unmanaged => new(messageType, data.ToBytesUnmanaged());
    public static IpcEnvelope Create(MessageTypeCode messageType, byte[] data) => new(messageType, data);
    public static IpcEnvelope Create(MessageTypeCode messageType, params string[] stringData) => new(messageType, null, stringData);
    public void BroadCast(bool includeSelf = false)
    {
        var sw = Stopwatch.StartNew();
        var protoSerialize = this.ProtoSerialize();
        Service.Log.Verbose($"proto serialized in {sw.Elapsed.TotalMilliseconds}ms");
        var serialized = protoSerialize.Compress();
        Service.Log.Verbose($"data compressed in {sw.Elapsed.TotalMilliseconds}ms");
        Plugin.IpcManager.BroadCast(serialized, includeSelf);
    }

    public T DataStruct<T>() where T : unmanaged
    {
        if (Data != null)
        {
            return Data.ToStructUnmanaged<T>();
        }

        throw new ArgumentNullException();
    }

    public override string ToString() => $"{nameof(IpcEnvelope)}:{TimeStamp:O}:{MessageType}:{BroadcasterId:X}:{Data?.Length}:{StringData?.Length}";

    [ProtoMember(1)]
    public MessageTypeCode MessageType
    {
        get; init;
    }

    [ProtoMember(2)]
    public long BroadcasterId
    {
        get; init;
    }
    [ProtoMember(3)]
    public int ProcessId
    {
        get; init;
    } = Environment.ProcessId;
    [ProtoMember(4)]
    public DateTime TimeStamp
    {
        get; init;
    } = DateTime.Now;
    [ProtoMember(5)]
    public byte[]? Data
    {
        get; init;
    }
    [ProtoMember(6)]
    public string[] StringData
    {
        get; init;
    }
}
