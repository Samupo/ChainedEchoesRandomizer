using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CERandomizer
{
    public class ActionQuery : MonoBehaviour
    {
        static ActionQuery instance;
        
        public class QueriedAction
        {
            public Action action;
            public float remainingTime;

            public QueriedAction(Action action, float remainingTime)
            {
                this.action = action;
                this.remainingTime = remainingTime;
            }
        }

        List<QueriedAction> queriedActions = new List<QueriedAction>();

        public void Awake()
        {
            instance = this;
        }

        public static void AddAction(Action action, float time)
        {
            instance.queriedActions.Add(new QueriedAction(action, time));
        }

        void Update()
        {
            List<QueriedAction> toRemove= new List<QueriedAction>();
            foreach (QueriedAction action in queriedActions)
            {
                action.remainingTime -= Time.deltaTime;
                if (action.remainingTime <= 0)
                {
                    toRemove.Add(action);
                    action.action();
                }
            }
            foreach (QueriedAction action in toRemove)
            {
                queriedActions.Remove(action);
            }
        }
    }
}
