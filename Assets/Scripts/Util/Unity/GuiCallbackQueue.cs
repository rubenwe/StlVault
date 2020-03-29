﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace StlVault.Util.Unity
{
    public class GuiCallbackQueue : MonoBehaviour
    {
        private static readonly Queue<Action> Callbacks = new Queue<Action>();

        public static void Enqueue([NotNull] Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            lock (Callbacks)
            {
                Callbacks.Enqueue(callback);
            }
        }

        private void Update()
        {
            lock (Callbacks)
            {
                while (Callbacks.Count > 0)
                {
                    var action = Callbacks.Dequeue();
                    action.Invoke();
                }
            }
        }
    }
}