// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:42
// Ver : 1.0.0
// Description : ProcedureAdaptScreenView.cs
// ChangeLog :
// **********************************************

using Cysharp.Threading.Tasks;
using Wonderland.Utility;

namespace Wonderland.Launcher
{
    public class ProcedureAdaptScreenView:Procedure
    {
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            DesignResolution.SetUpViewResolution();
            return true;
        }
    }
}