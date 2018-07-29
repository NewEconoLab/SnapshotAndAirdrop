using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotAndAirdrop.Handle
{
    interface ITask
    {
        void StartTask(params object[] args);
        void Handle(params object[] args);
        void StopTask();
    }


    public abstract class PTask : ITask
    {
        public delegate void DeleRuntime(params string[] describes);
        public DeleRuntime deleRuntime;

        public delegate void DeleResult(params string[] describes);
        public DeleResult deleResult;

        public void StartTask(params object[] args)
        {
            Task task = new Task(() => {
                Handle(args);
            });
            task.Start();
        }

        public abstract void Handle(params object[] args);

        public void StopTask()
        {

        }
    }
}
