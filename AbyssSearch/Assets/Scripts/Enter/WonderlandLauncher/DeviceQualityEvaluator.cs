using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wonderland.Launcher
{
    public static class DeviceQualityEvaluator
    {
        public enum QualityTier
        {
            High, // 旗舰设备
            Low // 低端设备
        }

        private static QualityTier? _cachedTier;
        private static string _qualitySuffix;
        private static string _evaluationLog;

        public static QualityTier CurrentTier
        {
            get
            {
                if (!_cachedTier.HasValue)
                {
                    _cachedTier = EvaluateDeviceTier();
                    _qualitySuffix = "_" + _cachedTier.ToString();
                }

                return _cachedTier.Value;
            }
        }

        public static string QualitySuffix => _qualitySuffix;

        private static QualityTier EvaluateDeviceTier()
        {
            // 调试覆盖
            if (PlayerPrefs.HasKey("ForceQuality"))
            {
                return (QualityTier) PlayerPrefs.GetInt("ForceQuality", (int) QualityTier.High);
            }

            // 创建评估日志
            StringBuilder log = new StringBuilder();
            log.AppendLine("===== Device Evaluation Report =====");
            log.AppendLine($"Device Model: {SystemInfo.deviceModel}");
            log.AppendLine($"OS: {SystemInfo.operatingSystem}");

            // 1. 内存优先策略：小于3GB直接判定为低端设备
            int systemMemory = SystemInfo.systemMemorySize;
            log.AppendLine($"System Memory: {systemMemory}MB");

            if (systemMemory < 3000)
            {
                log.AppendLine("Decision: Low (Memory < 3GB)");
                _evaluationLog = log.ToString();
                return QualityTier.Low;
            }

            // 2. 综合评估逻辑
            float score = CalculateDeviceScore(log);
            log.AppendLine($"Final Score: {score:F1}/12");

            // 调整阈值：7分以上为高端
            QualityTier tier = score >= 4.0f ? QualityTier.High : QualityTier.Low;
            log.AppendLine($"Decision: {tier}");

            _evaluationLog = log.ToString();
            Debug.LogError(_evaluationLog);
            return tier;
        }

        private static float CalculateDeviceScore(StringBuilder log)
        {
            float score = 0f;

            // GPU评分 (0-6分)
            float gpuScore = EvaluateGPU(log);
            score += gpuScore;

            // CPU评分 (0-3分)
            float cpuScore = EvaluateCPU(log);
            score += cpuScore;

            // 内存评分 (0-2分)
            float memScore = EvaluateMemory(log);
            score += memScore;

            // 系统评分 (0-1分)
            float osScore = EvaluateSystem(log);
            score += osScore;

            return score;
        }

        private static float EvaluateGPU(StringBuilder log)
        {
            string gpu = SystemInfo.graphicsDeviceName.ToLower();
            int vram = SystemInfo.graphicsMemorySize;

            // GPU型号评分字典
            var gpuScores = new Dictionary<string, float>
            {
                // 高端GPU (6分)
                {"adreno 740", 6f}, {"adreno 750", 6f}, {"adreno 7c", 6f},
                {"mali-g710", 6f}, {"mali-g715", 6f}, {"mali-g720", 6f},
                {"apple a16", 6f}, {"apple a17", 6f}, {"apple m1", 6f}, {"apple m2", 6f},
                {"nvidia orin", 6f}, {"tensor g3", 6f}, {"dimensity 9200", 6f}, {"dimensity 9300", 6f},

                // 中高端GPU (5分)
                {"adreno 730", 5f}, {"adreno 732", 5f},
                {"mali-g610", 5f}, {"mali-g615", 5f},
                {"apple a15", 5f}, {"apple a14", 5f},
                {"tensor g2", 5f}, {"dimensity 9000", 5f}, {"exynos 2200", 5f},

                // 中端GPU (4分)
                {"adreno 642", 4f}, {"adreno 650", 4f}, {"adreno 660", 4f}, {"adreno 680", 4f},
                 {"mali-g52", 4f}, {"mali-g78", 4f},
                {"apple a13", 4f}, {"apple a12", 4f},
                {"tensor g1", 4f}, {"dimensity 8100", 4f}, {"dimensity 8200", 4f}, {"exynos 1380", 4f},
                {"snapdragon 7+ gen 2", 4f}, {"snapdragon 7 gen 3", 4f},

                // 中低端GPU (3分)
                {"adreno 619", 3f}, {"adreno 620", 3f}, {"adreno 630", 3f},
                {"mali-g57", 3f}, {"mali-g68", 3f},
                {"apple a11", 3f}, {"apple a10", 3f},
                {"dimensity 800u", 3f}, {"dimensity 810", 3f}, {"exynos 1280", 3f},
                {"snapdragon 695", 3f}, {"snapdragon 4 gen 1", 3f},

                // 低端GPU (2分)
                {"adreno 610", 2f}, {"adreno 612", 2f}, {"adreno 616", 2f},
                {"mali-g35", 2f}, {"mali-g31", 2f},
                {"apple a9", 2f}, {"apple a8", 2f},
                {"dimensity 700", 2f}, {"dimensity 720", 2f}, {"exynos 850", 2f},
                {"snapdragon 480", 2f}, {"snapdragon 680", 2f}
            };

            // 查找最匹配的GPU型号
            float gpuScore = 0f;
            string matchedModel = "Unknown";

            foreach (var kvp in gpuScores)
            {
                if (gpu.Contains(kvp.Key))
                {
                    if (kvp.Value > gpuScore)
                    {
                        gpuScore = kvp.Value;
                        matchedModel = kvp.Key;
                    }
                }
            }

            // 显存加成（高端GPU额外加分）
            if (gpuScore >= 4f)
            {
                if (vram >= 6000)
                {
                    gpuScore += 1f;
                    log.AppendLine($"GPU Bonus: +1 (VRAM >= 6000MB)");
                }
                else if (vram >= 4000)
                {
                    gpuScore += 0.5f;
                    log.AppendLine($"GPU Bonus: +0.5 (VRAM >= 4000MB)");
                }
            }

            // 未识别GPU使用保守估计
            if (gpuScore == 0f)
            {
                gpuScore = vram switch
                {
                    >= 6000 => 5f, // 6GB+显存
                    >= 4000 => 4f, // 4GB显存
                    >= 2000 => 3f, // 2GB显存
                    _ => 2f // 低显存
                };
                matchedModel = "Generic";
            }

            gpuScore = Mathf.Clamp(gpuScore, 0f, 6f);
            log.AppendLine($"GPU: {SystemInfo.graphicsDeviceName} ({vram}MB)");
            log.AppendLine($"- Matched Model: {matchedModel}");
            log.AppendLine(
                $"- Base Score: {gpuScore - (gpuScore >= 4f ? (vram >= 4000 ? (vram >= 6000 ? 1f : 0.5f) : 0f) : 0f):F1}");
            log.AppendLine($"- Final GPU Score: {gpuScore:F1}/6");

            return gpuScore;
        }

        private static float EvaluateCPU(StringBuilder log)
        {
            int cores = SystemInfo.processorCount;
            float frequency = SystemInfo.processorFrequency;
            string cpu = SystemInfo.processorType.ToLower();

            // CPU型号评分
            var cpuScores = new Dictionary<string, float>
            {
                // 高端CPU (3分)
                {"snapdragon 8 gen 2", 3f}, {"snapdragon 8 gen 3", 3f},
                {"dimensity 9200", 3f}, {"dimensity 9300", 3f},
                {"apple a17", 3f}, {"apple m2", 3f},
                {"exynos 2200", 3f}, {"exynos 2300", 3f},

                // 中高端CPU (2.5分)
                {"snapdragon 8+ gen 1", 2.5f}, {"snapdragon 8 gen 1", 2.5f},
                {"dimensity 9000", 2.5f}, {"dimensity 9000+", 2.5f},
                {"apple a16", 2.5f}, {"apple a15", 2.5f},
                {"exynos 2100", 2.5f},

                // 中端CPU (2分)
                {"snapdragon 7+ gen 2", 2f}, {"snapdragon 7 gen 3", 2f},
                {"dimensity 8100", 2f}, {"dimensity 8200", 2f},
                {"apple a14", 2f}, {"apple a13", 2f},
                {"exynos 1380", 2f}, {"exynos 1480", 2f},

                // 中低端CPU (1.5分)
                {"snapdragon 695", 1.5f}, {"snapdragon 4 gen 1", 1.5f},
                {"dimensity 800u", 1.5f}, {"dimensity 810", 1.5f},
                {"apple a12", 1.5f}, {"apple a11", 1.5f},
                {"exynos 1280", 1.5f},

                // 低端CPU (1分)
                {"snapdragon 480", 1f}, {"snapdragon 680", 1f},
                {"dimensity 700", 1f}, {"dimensity 720", 1f},
                {"apple a10", 1f}, {"apple a9", 1f},
                {"exynos 850", 1f}, {"exynos 880", 1f}
            };

            // 查找最匹配的CPU型号
            float cpuScore = 0f;
            string matchedModel = "Unknown";

            foreach (var kvp in cpuScores)
            {
                if (cpu.Contains(kvp.Key))
                {
                    if (kvp.Value > cpuScore)
                    {
                        cpuScore = kvp.Value;
                        matchedModel = kvp.Key;
                    }
                }
            }

            // 核心数和频率加成
            if (cores >= 8)
            {
                cpuScore += 1f;
                log.AppendLine($"CPU Bonus: +1 (Cores >= 8)");
            }
            else if (cores >= 6)
            {
                cpuScore += 0.5f;
                log.AppendLine($"CPU Bonus: +0.5 (Cores >= 6)");
            }

            if (frequency >= 2500)
            {
                cpuScore += 0.5f;
                log.AppendLine($"CPU Bonus: +0.5 (Freq >= 2.5GHz)");
            }

            // 未识别CPU使用保守估计
            if (cpuScore == 0f)
            {
                cpuScore = cores switch
                {
                    >= 8 => 3.5f, // 8核及以上
                    >= 6 => 2.5f, // 6核
                    >= 4 => 1.5f, // 4核
                    _ => 1f // 4核以下
                };
                matchedModel = "Generic";
            }

            cpuScore = Mathf.Clamp(cpuScore, 0f, 3f);
            log.AppendLine($"CPU: {SystemInfo.processorType} ({cores} cores, {frequency}MHz)");
            log.AppendLine($"- Matched Model: {matchedModel}");
            log.AppendLine(
                $"- Base Score: {cpuScore - (cores >= 6 ? (cores >= 8 ? 1f : 0.5f) : 0f) - (frequency >= 2500 ? 0.5f : 0f):F1}");
            log.AppendLine($"- Final CPU Score: {cpuScore:F1}/3");

            return cpuScore;
        }

        private static float EvaluateMemory(StringBuilder log)
        {
            int ram = SystemInfo.systemMemorySize;
            float memScore = ram switch
            {
                >= 12000 => 2.5f, // 12GB+
                >= 8000 => 2f, // 8GB
                >= 6000 => 1.5f, // 6GB
                >= 4000 => 1f, // 4GB
                >= 3000 => 0.5f, // 3GB
                _ => 0f // <3GB
            };

            log.AppendLine($"Memory Score: {memScore:F1}/2.5 ({ram}MB)");
            return memScore;
        }

        private static float EvaluateSystem(StringBuilder log)
        {
            // 操作系统加成
            string os = SystemInfo.operatingSystem.ToLower();
            float osScore = 0f;

            // Android版本评分
            if (os.Contains("android"))
            {
                if (os.Contains("14")) osScore = 1f;
                else if (os.Contains("13")) osScore = 0.8f;
                else if (os.Contains("12")) osScore = 0.6f;
                else if (os.Contains("11")) osScore = 0.4f;
                else if (os.Contains("10")) osScore = 0.2f;
            }
            // iOS版本评分
            else if (os.Contains("iphone") || os.Contains("ipad"))
            {
                if (os.Contains("17")) osScore = 1f;
                else if (os.Contains("16")) osScore = 0.8f;
                else if (os.Contains("15")) osScore = 0.6f;
                else if (os.Contains("14")) osScore = 0.4f;
                else if (os.Contains("13")) osScore = 0.2f;
            }

            log.AppendLine($"OS Score: {osScore:F1}/1 ({SystemInfo.operatingSystem})");
            return osScore;
        }

        // 供调试用
        public static void ForceQuality(QualityTier tier)
        {
            _cachedTier = tier;
            _qualitySuffix = "_" + tier.ToString();
            PlayerPrefs.SetInt("ForceQuality", (int) tier);
        }
    }
}