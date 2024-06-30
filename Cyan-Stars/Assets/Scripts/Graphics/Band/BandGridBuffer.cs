using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CyanStars.Graphics.Band
{
    public class BandGridBuffer : MonoBehaviour
    {
        //实际的Count会+1因为Shader那边的特性,为了直观就写作20
        //这里的Count指一侧的条带数量，最大是Aspect.x/2
        public int Count = 20;
        public Vector4 Aspect;
        public Material Material;
        private ComputeBuffer computeBuffer;
        private float[] offsets;

        public void UpdateBuffer()
        {
            for (int i = 0; i < computeBuffer.count; i++)
            {
                offsets[i] = Random.Range(0.0f, 0.5f);
            }
            computeBuffer.SetData(offsets);
        }

        public void GenerateBuffer()
        {
            Release();
            offsets = new float[Count * 2 + 1];
            computeBuffer = new ComputeBuffer(Count * 2 + 1, sizeof(float), ComputeBufferType.Default);
            Material.SetBuffer("grid", computeBuffer);
            Material.SetVector("_Aspect", Aspect);
            Material.SetFloat("_Width", Aspect.x / 2 - Count);
        }

        public void Release()
        {
            if (computeBuffer != null)
            {
                computeBuffer.Release();
            }
        }
    }
}
