using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AAPZ_Backend.SignalR
{
    public interface IInferenceHub
    {
        Task InferenceMessage(string label);
    }
}
