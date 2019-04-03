using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.SignalR
{
    public interface IInferenceHub
    {
        Task InferenceMessage(string label);
    }
}
