using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace StlVault.Util.Unity
{
    public class GuiCallbackQueue : MonoBehaviour
    {
        private static readonly Queue<Action> Callbacks = new Queue<Action>();
        private static volatile bool _instanceCreated;

        public static void Enqueue([NotNull] Action callback)
        {
            if(!_instanceCreated) ThrowNoInstance();
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            lock (Callbacks)
            {
                Callbacks.Enqueue(callback);
            }
        }

        private static void ThrowNoInstance() => 
            throw new InvalidOperationException("No Callback Queue worker exists. Enqueued tasks will not be processed!");
        
        private static void ThrowMultiple() => 
            throw new InvalidOperationException("Multiple Callback Queues created! Timeouts will not be accurate!");

        private void Awake()
        {
            if(_instanceCreated) ThrowMultiple();
            _instanceCreated = true;
        }

        private void Update()
        {
            lock (Callbacks)
            {
                var sw = Stopwatch.StartNew();
                while (Callbacks.Count > 0)
                {
                    var action = Callbacks.Dequeue();
                    action.Invoke();
                    
                    if (sw.ElapsedMilliseconds > 8) break;
                }
                
                sw.Stop();
            }
        }
    }
}