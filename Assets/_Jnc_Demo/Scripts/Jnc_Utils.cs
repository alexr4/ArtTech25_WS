using UnityEngine;

public static class Jnc_Utils
{
    public class Wiggle
    {
        class Position
        {
            public Vector3 position;
            public float time;
            public float timeMax;

            public float Progress => Mathf.Clamp01(time / timeMax);
            public float Influence => Mathf.Sin(Progress * Mathf.PI);

            public void Reset(float period, float amplitude)
            {
                time = 0;
                timeMax = period * Mathf.Pow(2, Random.Range(-1f, 1f));
                position = Random.insideUnitSphere * amplitude;
            }

            public void Update(float period, float amplitude, float deltaTime)
            {
                time += deltaTime;

                if (time >= timeMax)
                    Reset(period, amplitude);
            }
        }

        public float frequency = 1;

        public float amplitude = 1;

        public Vector3 Delta { get; private set; }

        Random.State randomState;
        Position[] positions = new Position[0];

        public void Initialize(int seed = 0, int count = 4)
        {
            Random.InitState(seed);

            positions = new Position[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = new Position();
                positions[i].Reset(1f / frequency, amplitude);
            }

            randomState = Random.state;
        }

        public void Update(float deltaTime)
        {
            Random.state = randomState;

            Delta = new Vector3();
            foreach (var p in positions)
            {
                p.Update(1f / frequency, amplitude, deltaTime);
                Delta += p.position * p.Influence;
            }

            randomState = Random.state;
        }

        public void DrawGizmos(Vector3 position)
        {
            Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * amplitude);

            foreach (var p in positions)
            {
                Gizmos.DrawSphere(p.position, 0.1f);
                Gizmos.DrawWireSphere(p.position, 0.1f + p.Influence);
            }

            Gizmos.DrawSphere(Delta, 0.1f);
        }

    }
}
