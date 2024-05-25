using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.Ipc.Callers;
public interface IIpcCaller
{
    bool IsAvailable { get; }
    void CheckAPI();
}
