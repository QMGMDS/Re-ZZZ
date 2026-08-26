using UnityEngine;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 输入数据归一化
    /// </summary>
    public static class InputNormalization
    {
        /// <summary>
        /// 将输入轴限制为二维单位范围
        /// </summary>
        public static Vector2 NormalizeAxis(Vector2 input)
        {
            if (input.sqrMagnitude == 0f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
