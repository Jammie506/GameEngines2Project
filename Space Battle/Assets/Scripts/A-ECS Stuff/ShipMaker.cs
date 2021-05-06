using System.Collections;
using ew;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

//[AddComponentMenu("DOTS Samples/SpawnFromMonoBehaviour/Spawner")]
public class ShipMaker : MonoBehaviour
{
    public GameObject Prefab;
    
    [SerializeField] private float maxSize;

    [SerializeField] private int shipCount;
    
    private EntityManager entityManager;
    
    public Vector3 constrainTranslation;
    
    public float baseConstrainWeight = 1.0f;
    public float constrainWeight = 1.0f;
    
    public int totalNeighbours = 50;
    public float limitUpAndDown = 0.5f;
    public float seperationWeight = 1.0f;
    public float cohesionWeight = 2.0f;
    public float neighbourDistance = 20;
    public float seekWeight = 0;
    public float fleeWeight = 1.0f;
    public float fleeDistance = 50;
    
    private EntityArchetype shipArchitype;
    NativeArray<Entity> allTheShips;
    
    private RenderMesh bodyMesh;
    public Mesh mesh;
    public Material material;

    public int maxBoidsPerFrame = 500;
    
    public float size = 3.0f;
    
    public Coroutine cr;
    
    Entity CreateShip (Vector3 pos, Quaternion q, int boidId, float size)
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        
        prefab = entityManager.CreateEntity(shipArchitype);
        allTheShips[boidId] = prefab;

        Translation p = new Translation();
        p.Value = pos;

        Rotation r = new Rotation();
        r.Value = q;

        entityManager.SetComponentData(prefab, p);
        entityManager.SetComponentData(prefab, r);

        NonUniformScale s = new NonUniformScale();
        s.Value = new Vector3(size * 0.5f, size, size);
        //s.Value = new Vector3(2, 4, 10);

        entityManager.SetComponentData(prefab, s);


        entityManager.SetComponentData(prefab, new Boid() { boidId = boidId, mass = 1, maxSpeed = 100, maxForce = 400, weight = 200 });
        entityManager.SetComponentData(prefab, new Seperation());
        entityManager.SetComponentData(prefab, new Alignment());
        entityManager.SetComponentData(prefab, new Cohesion());
        entityManager.SetComponentData(prefab, new Constrain());
        entityManager.SetComponentData(prefab, new Flee());
        entityManager.SetComponentData(prefab, new Wander()
        {
            distance = 2
            ,
            radius = 1.2f,
            jitter = 80,
            target = UnityEngine.Random.insideUnitSphere * 1.2f
        });
        entityManager.SetComponentData(prefab, new Spine() { parent = -1, spineId = boidId });

        entityManager.SetComponentData(prefab, new ObstacleAvoidance() {forwardFeelerDepth = 50, forceType = ObstacleAvoidance.ForceType.normal });
    }
    
    void Start()
    {
        BoidJobSystem.Instance.Enabled = true;
        allTheShips = new NativeArray<Entity>(shipCount, Allocator.Persistent);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        constrainTranslation = transform.position;
        Cursor.visible = false;
        constrainWeight = baseConstrainWeight;

        shipArchitype = entityManager.CreateArchetype(
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
            typeof(ObstacleAvoidance)

        );
        
        
        bodyMesh = new RenderMesh
        {
            mesh = mesh,
            material = material
        };
        
        StartCoroutine(CreateBoids());
            
        Cursor.visible = false;

        cr = StartCoroutine(Show());
        
        /*// Create entity prefab from the game object hierarchy once
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // Efficiently instantiate a bunch of entities from the already converted entity prefab
        var instance = entityManager.Instantiate(prefab);

        // Place the instantiated entity in a grid with some noise
        var position = new Vector3(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY), Random.Range(-maxZ, maxZ));
        entityManager.SetComponentData(instance, new Translation {Value = position});*/
    }
    
    IEnumerator CreateBoids()
    {
        int created = 0;
        BoidJobSystem.Instance.Enabled = true;
        
        while (created < shipCount)
        {
            Vector3 pos = UnityEngine.Random.insideUnitSphere * maxSize;
            Quaternion q = Quaternion.Euler(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(0, 360), 0);
            CreateShip(transform.position + pos, q, created, size);
            created++;
            if (created % maxBoidsPerFrame == 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    void DoExplosion(int expType)
        {
            switch (expType)
            {
                case 1:
                    maxSize = 10;
                    totalNeighbours = 1;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    //constrainTranslation = Camera.main.transform.Translation;
                    break;
                case 2:
                    maxSize = 1000;
                    cohesionWeight = 0;
                    totalNeighbours = 100;
                    neighbourDistance = 100;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 3:
                    maxSize = 1300;
                    cohesionWeight = 0;
                    totalNeighbours = 100;
                    neighbourDistance = 100;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 4:
                    maxSize = 1500;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 0;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 5:
                    maxSize = 2000;
                    neighbourDistance = 0;
                    totalNeighbours = 100;
                    cohesionWeight = 0;
                    limitUpAndDown = 1;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 6:
                    maxSize = 800;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 7:
                    maxSize = 1000;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 8:
                    maxSize = 1500;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 9:
                    maxSize = 2000;
                    neighbourDistance = 150;
                    totalNeighbours = 100;
                    cohesionWeight = 2;
                    limitUpAndDown = 0.9f;
                    seekWeight = 0;
                    constrainWeight = baseConstrainWeight;
                    break;
                case 10:
                    seekWeight = 1;
                    maxSize = 2000;
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
}
