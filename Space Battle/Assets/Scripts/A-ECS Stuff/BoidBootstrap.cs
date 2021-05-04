using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


namespace ew
{
    public class BoidBootstrap : MonoBehaviour
    {

        public GameObject Prefab;
        
        public static int MAX_BOIDS = 25000;
        public static int MAX_SPINES = 50;
        public static int MAX_NEIGHBOURS = 150;

        private EntityArchetype boidArchitype;
        private EntityArchetype headArchitype;
        private EntityArchetype tailArchitype;
        private EntityArchetype spineArchitype;

        private EntityManager entityManager;

        private RenderMesh bodyMesh;
        //public Mesh mesh;
        //public Material material;

        public float seperationWeight = 1.0f;
        public float cohesionWeight = 2.0f;
        public float alignmentWeight = 1.0f;
        public float wanderWeight = 1.0f;
        public float baseConstrainWeight = 1.0f;
        public float constrainWeight = 1.0f;

        public float fleeWeight = 1.0f;
        public float fleeDistance = 50;

        public float headAmplitude = 20;
        public float tailAmplitude = 30;
        public float animationFrequency = 1;

        public int totalNeighbours = 50;

        public bool threedcells = false;

        public int spineLength = 4;
        public float bondDamping = 10;
        public float angularDamping = 10;

        public float limitUpAndDown = 0.5f;

        public int maxBoidsPerFrame = 500;

        public float seekWeight = 0;

        public Vector3 constrainTranslation;

        NativeArray<Entity> allTheBoids;
        NativeArray<Entity> allTheheadsAndTails;
        NativeArray<Entity> allTheSpines;

        BoidJobSystem boidJobSystem;

        public Coroutine cr;

        public static float Map(float value, float r1, float r2, float m1, float m2)
        {
            float dist = value - r1;
            float range1 = r2 - r1;
            float range2 = m2 - m1;
            return m1 + ((dist / range1) * range2);
        }

        public void OnDestroy()
        {
            Debug.Log("OnDestroy BoidBootstrap");

            if (World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated)
            {
                Debug.Log("Destroying the entities");
                entityManager.DestroyEntity(allTheBoids);
                entityManager.DestroyEntity(allTheheadsAndTails);
                entityManager.DestroyEntity(allTheSpines);
                BoidJobSystem.Instance.Enabled = false;
                SpineSystem.Instance.Enabled = false;
                HeadsAndTailsSystem.Instance.Enabled = false;
            }    
            allTheBoids.Dispose();
            allTheheadsAndTails.Dispose();
            allTheSpines.Dispose();
        }
        
        Entity CreateBoidWithTrail(Vector3 pos, Quaternion q, int boidId, float size)
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            prefab = entityManager.CreateEntity(boidArchitype);
            allTheBoids[boidId] = prefab;
            Translation p = new Translation
            {
                Value = pos
            };

            Rotation r = new Rotation
            {
                Value = q
            };

            entityManager.SetComponentData(prefab, p);
            entityManager.SetComponentData(prefab, r);

            NonUniformScale s = new NonUniformScale
            {
                Value = new Vector3(size * 0.3f, size, size)
            };

            entityManager.SetComponentData(prefab, s);

            entityManager.SetComponentData(prefab, new Boid() { boidId = boidId, mass = 1, maxSpeed = 100 * UnityEngine.Random.Range(0.9f, 1.1f), maxForce = 400, weight = 200 });
            entityManager.SetComponentData(prefab, new Seperation());
            entityManager.SetComponentData(prefab, new Alignment());
            entityManager.SetComponentData(prefab, new Cohesion());
            entityManager.SetComponentData(prefab, new Constrain());
            entityManager.SetComponentData(prefab, new Flee());
            entityManager.SetComponentData(prefab, new Wander()
            {
                distance = 2,
                radius = 1.2f,
                jitter = 80,
                target = UnityEngine.Random.insideUnitSphere * 1.2f
            });
            
            entityManager.SetComponentData(prefab, new Spine() { parent = -1, spineId = (spineLength + 1) * boidId });
            entityManager.SetComponentData(prefab, new ObstacleAvoidance() {forwardFeelerDepth = 50, forceType = ObstacleAvoidance.ForceType.normal});

            entityManager.AddSharedComponentData(prefab, bodyMesh);

            for (int i = 0; i < spineLength; i++)
            {
                int parentId = (boidId * (spineLength + 1)) + i;
                Translation sp = new Translation
                {
                    Value = pos - (q * Vector3.forward) * size * (float)(i + 1)
                };
                Entity spineEntity = entityManager.CreateEntity(spineArchitype);
                int spineIndex = (boidId * spineLength) + i;
                allTheSpines[spineIndex] = spineEntity;

                entityManager.SetComponentData(spineEntity, sp);
                entityManager.SetComponentData(spineEntity, r);
                entityManager.SetComponentData(spineEntity, new Spine() { parent = parentId, spineId = parentId + 1, offset = new Vector3(0, 0, -size) });
                entityManager.AddSharedComponentData(spineEntity, bodyMesh);
                s = new NonUniformScale
                {
                    Value = new Vector3(0.01f, Map(i, 0, spineLength, size, 0.01f * size), size)
                };
                //s.Value = new Vector3(2, 4, 10);
                entityManager.SetComponentData(spineEntity, s);

            }

            return prefab;
        }

        public int numBoids = 100;
        public float radius = 2000;
        public float neighbourDistance = 20;

        [Range(0.0f, 10.0f)]
        public float speed = 1.0f;

        public bool isContainer = false;

        // Start is called before the first frame update
        void Start()
        {
            BoidJobSystem.Instance.Enabled = true;
            HeadsAndTailsSystem.Instance.Enabled = true;
            SpineSystem.Instance.Enabled = true;
            allTheBoids = new NativeArray<Entity>(numBoids, Allocator.Persistent);
            allTheheadsAndTails = new NativeArray<Entity>(numBoids * 2, Allocator.Persistent);
            allTheSpines = new NativeArray<Entity>(numBoids * spineLength, Allocator.Persistent);

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            constrainTranslation = transform.position;
            Cursor.visible = false;
            constrainWeight = baseConstrainWeight;

            boidArchitype = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                typeof(LocalToWorld),
                typeof(RenderBounds),
                typeof(Boid),
                typeof(Seperation),
                typeof(Cohesion),
                typeof(Alignment),
                typeof(Wander),
                typeof(Constrain),
                typeof(Flee),
                typeof(Seek),
                typeof(ObstacleAvoidance),
                typeof(Spine)

            );

            /*bodyMesh = new RenderMesh
            {
                mesh = mesh,
                material = material
            };*/
            
            StartCoroutine(CreateBoids());
            
            Cursor.visible = false;

            cr = StartCoroutine(Show());

            //Cursor.lockState = CursorLockMode.Locked;
        }

        IEnumerator CreateBoids()
        {
            int created = 0;
            //BoidJobSystem.Instance.Enabled = true;
            //SpineSystem.Instance.Enabled = true;
            //HeadsAndTailsSystem.Instance.Enabled = true;
            while (created < numBoids)
            {
                Vector3 pos = UnityEngine.Random.insideUnitSphere * radius;
                Quaternion q = Quaternion.Euler(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(0, 360), 0);
                CreateBoidWithTrail(transform.position + pos, q, created, size);
                created++;
                if (created % maxBoidsPerFrame == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        public float size = 3.0f;

        public int cellSize = 50;
        public int gridSize = 10000;
        public bool usePartitioning = true;

        //Material boidMaterial;

        public void Update()
        {
            if (isContainer)
                return;

            BoidJobSystem.Instance.bootstrap = this;
            //SpineSystem.Instance.bootstrap = this;
            //HeadsAndTailsSystem.Instance.bootstrap = this;
            
            Explosion();
        }

        float ellapsed = 1000;
        public float toPass = 0.3f;
        public int clickCount = 0;
        

        void DoExplosion(int expType)
        {
            switch (expType)
            {
                case 1:
                    radius = 10;
                    totalNeighbours = 1;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    //constrainTranslation = Camera.main.transform.Translation;
                    break;
                case 2:
                    radius = 1000;
                    cohesionWeight = 0;
                    totalNeighbours = 100;
                    neighbourDistance = 100;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 3:
                    radius = 1300;
                    cohesionWeight = 0;
                    totalNeighbours = 100;
                    neighbourDistance = 100;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 4:
                    radius = 1500;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 0;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 5:
                    radius = 2000;
                    neighbourDistance = 0;
                    totalNeighbours = 100;
                    cohesionWeight = 0;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 6:
                    radius = 800;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 7:
                    radius = 1000;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 8:
                    radius = 1500;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 9:
                    radius = 2000;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 10:
                    seekWeight = 1;
                    radius = 2000;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    fleeWeight = 3.0f;
                    cohesionWeight = 2;
                    constrainWeight = 0;
                    limitUpAndDown = 0.9f;
                    break;
            }
        }

        public IEnumerator Show()
        {
            while (true)
            {
                yield return new WaitForSeconds(30);
                DoExplosion(1);
                yield return new WaitForSeconds(UnityEngine.Random.Range(4, 6));
                int exp = UnityEngine.Random.Range(2, 10);
                DoExplosion(exp);
                Debug.Log(exp);
            }
        }


        void Explosion()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                clickCount = (clickCount + 1) % 10;
                ellapsed = 0;
            }
            ellapsed += Time.deltaTime;

            if (ellapsed > toPass && clickCount > 0)
            {

                Debug.Log(clickCount);
                DoExplosion(clickCount);
                clickCount = 0;
            }

        }
    }
}
