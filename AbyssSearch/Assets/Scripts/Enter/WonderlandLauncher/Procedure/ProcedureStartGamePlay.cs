// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:57
// Ver : 1.0.0
// Description : ProcedureStartGamePlay.cs
// ChangeLog :
// **********************************************

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wonderland.Launcher
{
    public class ProcedureStartGamePlay : Procedure
    {
        
        public override async UniTask<bool> ExecuteProcedure(ProcedureContext procedureContext)
        {
            base.ExecuteProcedure(procedureContext);
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies.Length > 0)
            {
                Debug.LogError($"Search Assemblies:{this.GetType().FullName}");

                string[] targetNames =
                {
                    "Assembly-CSharp"
#if UNITY_EDITOR
                    , "Custom-CSharp"
#endif
                };

                foreach (var assembly in assemblies)
                {
                    if (targetNames.Contains(assembly.GetName().Name))
                    {
                        var type = assembly.GetType("GamePlay.GameApp");
                        if (type != null)
                        {
                            var method = type.GetMethod("Entrance");
                            object[] objects = new object[] {new object[] {assembly.GetName().Name}};
                            if (method != null)
                            {
                                method.Invoke(type, objects);
                                Debug.LogError($"LaunchGame Called");
                            }

                            break;
                        }
                    }
                }
            }

            return true;
        }
    }
}