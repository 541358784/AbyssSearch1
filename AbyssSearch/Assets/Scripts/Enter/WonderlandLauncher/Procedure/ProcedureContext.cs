// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/16:07
// Ver : 1.0.0
// Description : ProcedureContext.cs
// ChangeLog :
// **********************************************

using TMPro;
using UnityEngine;

namespace Wonderland.Launcher
{
    public class ProcedureContext
    {
        public TMP_Text DebugText;
        public GameObject LoadingView;
        public string ProcedureName;
        public float currentLoadingProcess = 0;
    }
}