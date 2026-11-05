using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        readonly ConcurrentQueue<Action> pending=new();
        public void Enqueue(Action action){if(action!=null)pending.Enqueue(action);}
        void Update(){while(pending.TryDequeue(out Action action))action();}
    }
}
