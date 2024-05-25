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
// along with this program.  If not, see https://github.com/akira0245/MidiBard/blob/master/LICENSE.
// 
// This code was originally written by akira0245 and was originally used in the MidiBard project. Any usage of this code must prominently credit the author, akira0245, and indicate that it was originally used in the MidiBard project.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using TinyIpc.IO;
using TinyIpc.Messaging;
using MultiBoxHelper.Util;
using System.IO.Compression;
using System.IO;
using MultiBoxHelper.Ipc.Callers;

namespace MultiBoxHelper.Ipc;

internal class IpcManager : IDisposable
{
    private readonly bool initFailed;
    private bool messagesQueueRunning = true;
    private readonly TinyMessageBus? messageBus;
    private readonly ConcurrentQueue<(byte[] serialized, bool includeSelf)> messageQueue = new();
    private readonly AutoResetEvent autoResetEvent = new(false);
    private readonly Dictionary<MessageTypeCode, Action<IpcEnvelope>>? methodInfos;

    public IpcPenumbra Penumbra { get; private set; }

    internal IpcManager()
    {
        Penumbra = new();

        try
        {
            const long maxFileSize = 1 << 24;
            messageBus = new TinyMessageBus(new TinyMemoryMappedFile("MultiBoxHelper.IPC", maxFileSize), true);
            messageBus.MessageReceived += MessageBus_MessageReceived;

            methodInfos = typeof(IpcHandles)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Select(i => (i.GetCustomAttribute<IpcHandleAttribute>()?.TypeCode, methodInfo: i))
                .Where(i => i.TypeCode != null)
                .ToDictionary(i => (MessageTypeCode)i.TypeCode!,
                    i => i.methodInfo.CreateDelegate<Action<IpcEnvelope>>(null));

            var thread = new Thread(() =>
            {
                Service.Log.Information($"IPC message queue worker thread started");
                while (messagesQueueRunning)
                {
                    Service.Log.Verbose($"Try dequeue message");
                    while (messageQueue.TryDequeue(out var dequeue))
                    {
                        try
                        {

                            var message = dequeue.serialized;
                            var messageLength = message.Length;
                            Service.Log.Verbose($"Dequeue serialized. length: {Dalamud.Utility.Util.FormatBytes(messageLength)}");
                            if (messageLength > maxFileSize)
                            {
                                throw new InvalidOperationException($"Message size is too large! TinyIpc will crash when handling this, not gonna let it through. maxFileSize: {Dalamud.Utility.Util.FormatBytes(maxFileSize)}");
                            }

                            if (messageBus.PublishAsync(message).Wait(5000))
                            {
                                Service.Log.Verbose($"Message published.");
                                if (dequeue.includeSelf)
                                    MessageBus_MessageReceived(this, new TinyMessageReceivedEventArgs(message));
                            }
                            else
                            {
                                throw new TimeoutException("IPC didn't published in 5000 ms, what happened?");
                            }
                        }
                        catch (Exception e)
                        {
                            Service.Log.Warning(e, $"Error when try publishing ipc");
                        }
                    }

                    autoResetEvent.WaitOne();
                }
                Service.Log.Information($"IPC message queue worker thread ended");
            });
            thread.IsBackground = true;
            thread.Start();
        }
        catch (PlatformNotSupportedException e)
        {
            Service.Log.Error(e, $"TinyIpc init failed. Unfortunately TinyIpc is not available on Linux. local ensemble sync will not function properly.");
            initFailed = true;
        }
        catch (Exception e)
        {
            Service.Log.Error(e, $"TinyIpc init failed. local ensemble sync will not function properly.");
            initFailed = true;
        }
    }

    // I don't know why the extension doesn't work for me, but this does.
    // TODO: figure out why the extension doesn't work. Low priority, but still.
    private static byte[] DecompressMessage(byte[] message)
    {
        using MemoryStream memoryStream = new MemoryStream(message);
        using MemoryStream destination = new MemoryStream();
        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
        {
            gzipStream.CopyTo((Stream)destination);
        }
        return destination.ToArray();
    }

    private void MessageBus_MessageReceived(object? sender, TinyMessageReceivedEventArgs e)
    {
        if (initFailed)
        {
            Service.Log.Debug("IPC Manager init failed, so we can't process a message.");
            return;
        }

        try
        {
            var sw = Stopwatch.StartNew();
            Service.Log.Verbose($"message received");
            byte[] bytes = DecompressMessage(e.Message.ToArray<byte>());
            Service.Log.Verbose($"message decompressed in {sw.Elapsed.TotalMilliseconds}ms");
            var message = bytes.ProtoDeserialize<IpcEnvelope>();
            Service.Log.Verbose($"proto deserialized in {sw.Elapsed.TotalMilliseconds}ms");
            ProcessMessage(message);


        }
        catch (Exception exception)
        {
            Service.Log.Error(exception, "error when processing received message");
        }
    }

    private void ProcessMessage(IpcEnvelope message)
    {
        if (methodInfos != null)
        {
            methodInfos[message.MessageType](message);
        }
    }

    public void BroadCast(byte[] serialized, bool includeSelf = false)
    {
        if (initFailed)
        {
            return;
        }

        try
        {
            Service.Log.Verbose($"queuing message. length: {Dalamud.Utility.Util.FormatBytes(serialized.Length)}" + (includeSelf ? " includeSelf" : null));
            messageQueue.Enqueue(new(serialized, includeSelf));
            autoResetEvent.Set();
        }
        catch (Exception e)
        {
            Service.Log.Warning(e, "error when queuing message");
        }
    }

    private void ReleaseUnmanagedResources(bool disposing)
    {
        try
        {
            messagesQueueRunning = false;
            if (messageBus != null)
            {
                messageBus.MessageReceived -= MessageBus_MessageReceived;
            }
            autoResetEvent?.Set();
            autoResetEvent?.Dispose();
        }
        finally
        {
            //RPCResponse = delegate { };
        }

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources(true);
    }

    ~IpcManager()
    {
        ReleaseUnmanagedResources(false);
    }
}
