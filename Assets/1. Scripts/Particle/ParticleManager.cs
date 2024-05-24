
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particle
{
    public class ParticleWrapper : MonoBehaviour
    {
        public ParticleManager Manager { get; private set; }
        public ParticleSO ParticleSO { get; private set; }
        public ParticleSystem ParticleSystem { get; private set; }
        private ParticleSystem.Particle[] particles;
        public bool IsActive { get; private set; } = false;
        public bool IsReturning { get; private set; } = false;

        public void Assign(ParticleSystem particleSystem, ParticleSO particleSO, ParticleManager manager)
        {
            Manager = manager;
            ParticleSystem = particleSystem;
            ParticleSO = particleSO;

            ParticleSystem.Stop();
            gameObject.SetActive(false);
            AttachParticleSystem(manager.transform);
        }

        public void Activate()
        {
            if (IsActive == false)
            {
                gameObject.SetActive(true);
                IsActive = true;
                ParticleSystem.Play();
            }
        }

        public void AttachParticleSystem(Transform parent)
        {
            transform.SetParent(parent);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void SetOrientation(Vector3 pos, Quaternion rot)
        {
           transform.SetPositionAndRotation(pos, rot);
        }

        public void ChangeParticalSize(float size)
        {
            size *= ParticleSO.SizeModifier;

            if (size < 0)
            {
                size = 0;
            }

            var mainModule = ParticleSystem.main;
            mainModule.startSize = size;

            particles = new ParticleSystem.Particle[ParticleSystem.main.maxParticles];

            int numParticlesAlive = ParticleSystem.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                particles[i].startSize = size;
            }


            ParticleSystem.SetParticles(particles, numParticlesAlive);
        }

        public void OnParticleSystemStopped()
        {
            ReturnParticles();
        }

        public void ReturnParticles(Quaternion lingerRotation = default)
        {
            if (IsActive && !IsReturning)
            {
                StartCoroutine(ReturnCoroutine(lingerRotation));
            }
        }

        private IEnumerator ReturnCoroutine(Quaternion lingerRotation = default)
        {
            IsReturning = true;

            Vector3 pos = transform.position;
            AttachParticleSystem(Manager.transform);
            SetOrientation(pos, lingerRotation);

            LingerParticles();

            yield return new WaitForSeconds(ParticleSO.LingerTime);

            AttachParticleSystem(Manager.transform);

            gameObject.SetActive(false);

            IsActive = false;
            IsReturning = false;

            Manager.ReturnParticleSystemToPool(this);
        }

        private void LingerParticles()
        {
            ParticleSystem.Stop(true);

            if (ParticleSO.ClearPrimaryParticles)
            {
                if (ParticleSO.ClearSecondaryParticles)
                {
                    ParticleSystem.Clear(true);
                }

                else ParticleSystem.Clear(false);
            }

            else if (ParticleSO.ClearSecondaryParticles)
            {
                var systems = GetComponentsInChildren<ParticleSystem>();

                for (int i = 1; i < systems.Length; i++)
                {
                    systems[i].Clear();
                }
            }
        }
    }


    public class ParticleManager : StaticInstance<ParticleManager>
    {

        [field: SerializeField] public List<ParticleSO> ParticleSystemPreloadData { get; private set; }

        [SerializeField, Header("Pool")] int initalPoolSize = 200;
        [SerializeField] int poolExtension = 25;

        [SerializeField, Header("Debug")] private ParticleSO testParticleSO;
        [SerializeField] private Vector3 testSpawnLocation;

        private Dictionary<ParticleSO, Queue<ParticleWrapper>> particleSystemPools = new();


        private void Start()
        {
            foreach (ParticleSO p in ParticleSystemPreloadData)
            {
                CreateNewParticlePool(p);
            }
        }

        private void CreateNewParticlePool(ParticleSO particleSO)
        {
            particleSystemPools.Add(particleSO, new());
            ExtendParticlePool(particleSO, initalPoolSize);
        }

        private void ExtendParticlePool(ParticleSO particleSO, int amount)
        {

            for (int i = 0; i < amount; i++)
            {

                ParticleSystem particleSystem = Instantiate(
                    particleSO.ParticleSystemPrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    transform);

                ParticleWrapper wrapper = particleSystem.gameObject.AddComponent<ParticleWrapper>();
                wrapper.Assign(particleSystem, particleSO, this);

                particleSystemPools[particleSO].Enqueue(wrapper);
            }
        }

        public ParticleWrapper RequestParticleSystem(ParticleSO ParticleSO)
        {
            if (!particleSystemPools.ContainsKey(ParticleSO))
            {
                CreateNewParticlePool(ParticleSO);
            }

            if (!particleSystemPools[ParticleSO].TryDequeue(out ParticleWrapper result))
            {
                ExtendParticlePool(ParticleSO, poolExtension);
                result = particleSystemPools[ParticleSO].Dequeue();
            }

            return result;
        }

        public void ReturnParticleSystemToPool(ParticleWrapper particleWrapper)
        {
            if (!particleSystemPools.ContainsKey(particleWrapper.ParticleSO))
            {
                return;
            }

            particleSystemPools[particleWrapper.ParticleSO].Enqueue(particleWrapper);
        }


        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void SpawnTestParticles()
        {
            var wrapper = RequestParticleSystem(testParticleSO);
            wrapper.AttachParticleSystem(null);
            wrapper.SetOrientation(testSpawnLocation, Quaternion.identity);
            wrapper.Activate();
        }
    }
}