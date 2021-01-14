using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    partial class CLRBindings
    {

        static private Action<ILRuntime.Runtime.Enviorment.AppDomain> InitializeAction;
        static public void InitializeSmart(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            if (InitializeAction != null)
            {
                //Debug.Log("CLRBindings.InitializeAction is set, run InitializeAction");
                InitializeAction(app);
            }
            else
            {
                //Debug.Log("CLRBindings.InitializeAction is null, skip InitializeAction");
            }
        }

    }
}
