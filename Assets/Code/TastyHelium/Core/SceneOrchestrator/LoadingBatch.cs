using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyHelium
{
    public class LoadingBatch
    {
        private Queue<ILoadingAction> _actions;

        public LoadingBatch()
        {
            _actions = new Queue<ILoadingAction>();
        }

        public void Add(Func<Task> asyncLoadAction, string description = "")
        {
            _actions.Enqueue(new AsyncLoadingAction
            {
                Action = asyncLoadAction,
                Description = description
            });
        }

        public void Add(Action loadAction, string description = "")
        {
            _actions.Enqueue(new LoadingAction
            {
                Action = loadAction,
                Description = description
            });
        }

        public async Task ProcessBatch(Action<LoadProgress> progressCallback)
        {
            int totalActions = _actions.Count;
            int processedActions = 0;

            progressCallback?.Invoke(new LoadProgress
            {
                Progress = totalActions > 0 ? (float)processedActions / totalActions : 1,
                Description = ""
            });

            while (_actions.Count > 0)
            {
                ILoadingAction lAction = _actions.Dequeue();
                switch (lAction)
                {
                    case AsyncLoadingAction asyncAction:
                        await asyncAction.Execute();
                        break;
                    case LoadingAction action:
                        await action.Execute();
                        break;
                }

                processedActions++;

                progressCallback?.Invoke(new LoadProgress
                {
                    Progress = (float)processedActions / totalActions,
                    Description = lAction.Description
                });
            }
        }

        public struct LoadProgress
        {
            public float Progress;
            public string Description;
        }

        public struct LoadingAction : ILoadingAction<Action>
        {
            public Action Action { get; set; }
            public string Description { get; set; }

            public Task Execute()
            {
                return Task.Run(Action);
            }
        }

        private struct AsyncLoadingAction : ILoadingAction<Func<Task>>
        {
            public Func<Task> Action { get; set; }
            public string Description { get; set; }

            public async Task Execute()
            {
                await Action();
            }
        }

        public interface ILoadingAction<T> : ILoadingAction
        {
            T Action { get; set; }
        }

        public interface ILoadingAction
        {
            string Description { get; set; }
            Task Execute();
        }
    }
}
