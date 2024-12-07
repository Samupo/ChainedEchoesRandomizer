using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CERandomizer
{
    public class ActionQueue : MonoBehaviour
    {
        static ActionQueue instance;
        
        public class QueuedAction
        {
            public Action action;
            public float remainingTime;

            public QueuedAction(Action action, float remainingTime)
            {
                this.action = action;
                this.remainingTime = remainingTime;
            }
        }

        List<QueuedAction> queriedActions = new List<QueuedAction>();

        public void Awake()
        {
            instance = this;
        }

        public static void AddAction(Action action, float time)
        {
            instance.queriedActions.Add(new QueuedAction(action, time));
        }

        void Update()
        {
            List<QueuedAction> toRemove= new List<QueuedAction>();
            foreach (QueuedAction action in queriedActions)
            {
                action.remainingTime -= Time.deltaTime;
                if (action.remainingTime <= 0)
                {
                    toRemove.Add(action);
                    action.action();
                }
            }
            foreach (QueuedAction action in toRemove)
            {
                queriedActions.Remove(action);
            }
        }
    }
}
