using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class MidiHookProxy : RealProxy
    {
        private MarshalByRefObject f_Target;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="t"></param>
        public MidiHookProxy(MarshalByRefObject target, Type t) : base(t)
        {
            this.f_Target = target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage call = (IMethodCallMessage)msg;
            IMethodReturnMessage res = null;
            IConstructionCallMessage ctor = call as IConstructionCallMessage;

            if (ctor != null)
            {
                //以下、コンストラクタを実行する処理

                RealProxy rp = RemotingServices.GetRealProxy(this.f_Target);
                res = rp.InitializeServerObject(ctor);
                MarshalByRefObject tp = this.GetTransparentProxy() as MarshalByRefObject;
                res = EnterpriseServicesHelper.CreateConstructionReturnMessage(ctor, tp);
            }
            else
            {
                //以下、コンストラクタ以外のメソッドを実行する処理

                try
                {
                    //メソッド前処理
                    lockSlim.EnterWriteLock();

                    //メソッド実行
                    res = RemotingServices.ExecuteMessage(this.f_Target, call);
                }
                finally
                {
                    //メソッド後処理
                    lockSlim.ExitWriteLock();
                }
            }

            return res;
        }


        private static ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    }
}
