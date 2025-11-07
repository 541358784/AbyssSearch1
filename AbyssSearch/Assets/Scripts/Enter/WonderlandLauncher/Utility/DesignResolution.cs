// **********************************************
// Copyright(c) 2021 by com.ustar
// All right reserved
// 
// Author : Jian.Wang
// Date : 2024/02/23/13:51
// Ver : 1.0.0
// Description : DesignResolution.cs
// ChangeLog :
// **********************************************

using System;
using UnityEngine;

namespace Wonderland.Utility
{
    public class DesignResolution
    {
        /// <summary>
        /// 设计分辨率
        /// </summary>
        public static Vector2 designSize = new Vector2(1536, 768);

        /// <summary>
        /// 游戏参考分辨率
        /// </summary>
        public static Vector2 matchResolution;


        public static void SetUpViewResolution()
        {
            var screenWidth = Math.Max(Screen.width, Screen.height);
            var screenHeight = Math.Min(Screen.width, Screen.height);

            var referenceSizeY = designSize.y;
            var referenceWidth = (float) (screenWidth) / screenHeight * referenceSizeY;

            matchResolution = new Vector2(referenceWidth, referenceSizeY);
        }
    }
}