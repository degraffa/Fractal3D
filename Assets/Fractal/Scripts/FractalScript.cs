// This script makes a shit-ton of GameObjects around each other

using UnityEngine;
using System.Collections;

public class FractalScript : MonoBehaviour { 
    // The amount of child objects each object can make
    int depth;
    // The maximum amount of child objects any object can make
    public int maxDepth = 5;

    // Changes size of child objects as new children are created
    public float childScale = 0.55f;
    private float originalChildScale = 0.55f;
    // Whether or not childScale is random
    public bool randomChildScale = true;

    // Chance of child being created
    public float spawnProbablility;

    // Cap on rotation speed
    public float maxRotationSpeed;
    // Random range of rotation speed based on maxRotationSpeed
    private float rotationSpeed;
	// Cap on SPIN BABY
	[Range(0, 5)]
	public float maxSpin;

    // Cap on random branch angles to add chaos
    public float maxTwist;

    // Wait times for objects to sapwn
    public float minWaitTime = 0.1f;
    public float maxWaitTime = 0.6f;

    // Multipliers for child object size
    public float minChildMultiplier = 0.75f;
    public float maxChildMultiplier = 1.25f;

    // Initialization of colors for objects
    public Color color1, color2, color3, color4, color5, color6;

    public Mesh[] meshs; // Reference to mesh for objects in fractal
    public Material material; // Reference to material for objects in fractal

    //stores directions and orientations in arrays to put through loop in functions instead of initializing 4 different times
    private static Vector3[] childDirections = { Vector3.up, Vector3.right, /*Vector3.down*, */ Vector3.left, Vector3.forward, Vector3.back };
    private static Quaternion[] childOrientations = { Quaternion.identity, Quaternion.Euler(0f, 0f, -90f),/*Quaternion.Euler(0f, 0f, 180)*/ Quaternion.Euler(0f, 0f, 90f),
                                                      Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)};

    // Store Materials in array in order to loop through them in InitializeMaterials in order to have less unique materials
    // Makes Unity use dynamic batching (combining meshes with the same material) more, which is more optimized. Triples FPS.
    // NOT STATIC because it relies on maxDepth which is private.
    private Material[,] materials;

    // Runs at launch
    private void Start() {
        //if (Application.isEditor) { // keeps view in scene mode
        //    UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        //}
        
        // Checks to see if it's done this before, will only run once, no matter how many instances of the script there are
        if (materials == null) {
            InitializeMaterials();
        }

        originalChildScale = childScale; 

        // Sets Rotation Speed for instance
        rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
        // Sets branch angle for instance
        transform.Rotate(Random.Range(-maxTwist, maxTwist), 0f, 0f);

        // Adds MeshFilter Component to the object the script is attached to (passes mesh from assets into mesh renderer)
        gameObject.AddComponent<MeshFilter>().mesh = meshs[Random.Range(0, meshs.Length)]; 
        // Adds MeshRenderer to object the script is attached to (renders mesh from mesh filter), color set in IntializeMaterials(), picks random one.
        gameObject.AddComponent<MeshRenderer>().material = materials[depth, Random.Range(0,2)]; 
        
        // Only makes new child if the depth is not at maximum
        if (depth < maxDepth)
        {
            // Coroutine requires method of return type IEnumerator, allows you to wait to perform next action
            StartCoroutine(CreateChildren());
        }
    }

    // Runs every frame
    void Update() {
        // Makes parts of fractal spin at varying speeds based on maxRotationSpeed
        // transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f); normal
		transform.Rotate(maxSpin, rotationSpeed * Time.deltaTime, maxSpin);
    }

    // IEnumerator so that function can be used in CoRoutine
    IEnumerator CreateChildren()
    {
        // For every direction in the childDirections array...
        for (int i = 0; i < childDirections.Length; i++) {
            // If statement has a spawnProbability% chance of being executed
            if (Random.value < spawnProbablility)
            {
                // wait for random small amount (so that processing power is more consistent)
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
                // spawn new childObject with childDirections[i] and childOrientations[i]
                new GameObject("Fractal Child").AddComponent<FractalScript>().InitializeObjects(this, i);
            }
        }
    }

    // Loops through materials to change color for each layer of depth
    void InitializeMaterials()
    {
        // maxDepth + 1 because initial block exists
        materials = new Material[maxDepth + 1, 2];
        // For each layer of depth...
        for (int i = 0; i <= maxDepth; i++)
        {
            // Set the the material for all instances at this layer of depth to material, and have the same color
            // The longer the time, the deeper the color
            float t = i / (maxDepth - 1f);
            // Squaring the time makes for a smoother transition. 
            t *= t;
            // First set of colors
            materials[i, 0] = new Material(material);
            materials[i, 0].color = Color.Lerp(color1, color2, t);

            // Second set of colors
            materials[i, 1] = new Material(material);
            materials[i, 1].color = Color.Lerp(color4, color5, t);
        }
        // Cool last color
        materials[maxDepth, 0].color = color3;
        materials[maxDepth, 1].color = color6;
    }

    // Establishes variables for new children objects which were set for original object
    // Just put all the variables up there in here. Just set variable = parent.variable if u wanna keep it the same
    void InitializeObjects(FractalScript parent, int childIndex)
    {
        meshs = parent.meshs;
        materials = parent.materials;
        
        maxDepth = parent.maxDepth;
        // Increases the depth of the object ensuring the recursive (shout out to big J) loop caused by the creation of multiple instances of this script is stopped eventually
        depth = parent.depth + 1;

        // sets childScale for new object
        randomChildScale = parent.randomChildScale;
        originalChildScale = parent.originalChildScale;
        if (randomChildScale) {
            childScale = parent.originalChildScale * Random.Range(minChildMultiplier, maxChildMultiplier);
        }
        else {
            childScale = parent.childScale;
        }
        
        spawnProbablility = parent.spawnProbablility;
        
        maxRotationSpeed = parent.maxRotationSpeed;
        
        maxTwist = parent.maxTwist;

        //  this nests the new object in the parent
        transform.parent = parent.transform;

        // sets the scale of the object equal to (1,1,1) * childscale
        transform.localScale = Vector3.one * childScale;
        // Moves the new block up 0.5 + half the block size relative to the parent object
        transform.localPosition = childDirections[childIndex] * (0.5f + 0.5f * childScale);
        // Adjusts rotation so that blocks are not created inside of each other. 
        transform.localRotation = childOrientations[childIndex]; 
    }
}