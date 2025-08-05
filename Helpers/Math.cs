namespace AnotherGamepadPlus.Helpers
{
    public static class MathTool
    {
        /// <summary>
        /// 将一个值从源范围映射到目标范围
        /// </summary>
        /// <param name="value">需要映射的原始值</param>
        /// <param name="sourceMin">源范围最小值</param>
        /// <param name="sourceMax">源范围最大值</param>
        /// <param name="targetMin">目标范围最小值</param>
        /// <param name="targetMax">目标范围最大值</param>
        /// <returns>映射到目标范围后的结果</returns>
        public static float MapValue(
            float value, 
            float sourceMin, 
            float sourceMax, 
            float targetMin, 
            float targetMax)
        {
            // 处理源范围相同的特殊情况（避免除零）
            if (Math.Abs(sourceMax - sourceMin) < float.Epsilon)
            {
                return (targetMin + targetMax) / 2f;
            }
            
            // 核心映射公式：(值 - 源最小值) / (源最大值 - 源最小值) * (目标最大值 - 目标最小值) + 目标最小值
            return (value - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
        }

        /// <summary>
        /// 将一个值从源范围映射到目标范围，并限制在目标范围内
        /// </summary>
        /// <param name="value">需要映射的原始值</param>
        /// <param name="sourceMin">源范围最小值</param>
        /// <param name="sourceMax">源范围最大值</param>
        /// <param name="targetMin">目标范围最小值</param>
        /// <param name="targetMax">目标范围最大值</param>
        /// <returns>映射并钳位后的结果</returns>
        public static float MapValueClamped(
            float value, 
            float sourceMin, 
            float sourceMax, 
            float targetMin, 
            float targetMax)
        {
            // 先映射
            float mapped = MapValue(value, sourceMin, sourceMax, targetMin, targetMax);
            // 再限制在目标范围内
            return Math.Clamp(mapped, targetMin, targetMax);
        }
    }
}
