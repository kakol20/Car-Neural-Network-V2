namespace Own
{
    public static class Random
    {
        /// <summary>
        /// Set Random Seed
        /// </summary>
        public static void Init() => UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

        /// <summary>
        /// Randomisation between min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Range(float min = 0, float max = 1)
        {
            float range = max - min;
            float value = UnityEngine.Random.value;

            return (range * value) + min;
        }
    }
}