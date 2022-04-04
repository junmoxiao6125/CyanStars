using UnityEngine;
using System.Collections.Generic;

namespace CyanStars.Gameplay.Effect
{
    public class NoteClickEffect : MonoBehaviour
    {
        public bool willDestroy;
        public float destroyTime;

        public List<ParticleSystem> particleSystemList;

        void Start()
        {
            //Debug.Log(transform.position);
            if (willDestroy) Destroy(gameObject, destroyTime);
        }

        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }
}