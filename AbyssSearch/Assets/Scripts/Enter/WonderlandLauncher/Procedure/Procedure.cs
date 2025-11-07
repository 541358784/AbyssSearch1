// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:42
// Ver : 1.0.0
// Description : ProcedureFetchVersionInfo.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wonderland.Launcher
{
    public class Procedure
    {
        public virtual async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            procedureContext.currentLoadingProcess += 0.05f;
            procedureContext.ProcedureName = GetType().Name;
            Debug.LogError("ExecuteProcedure:" + procedureContext.ProcedureName);
            return true;
        }
    }
}