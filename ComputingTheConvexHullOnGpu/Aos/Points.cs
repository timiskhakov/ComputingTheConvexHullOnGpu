namespace ComputingTheConvexHullOnGpu.Aos
{
    public readonly struct Points
    {
        public float[] Xs { get; }
        public float[] Ys { get; }

        public Points(int length)
        {
            Xs = new float[length];
            Ys = new float[length];
        }
    }
}