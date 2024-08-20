//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEditor.Rendering;
using UnityEngine;




public class FishPool : MonoBehaviour
{
    
    enum FishType { clownFish }
    
    FishType[] fishTypeList = { FishType.clownFish };
    
    // Data per fish type
    [SerializeField] List<Mesh> fishMesh;           // list of all meshes needed for each fish - must be same as enum list
    [SerializeField] List<Material> fishMaterials;  // list of all materials needed for each fish - must be same as enum list
    float[] fishMaxSpeed = { 40, 4 };      // list of scales to adjust each mesh
    int[] fishEatAmount = { 15, 7 };
    float[] prop_ZOffset = { 1.5f, 1.8f };
    float[] prop_Yaw = { 2, 3 };
    float[] prop_Roll = { 2, 3 };
    float[] prop_Scale = { 0.3f, 0.5f };
    
    
    // Arrays of per-frame data for each fish
    FishType[] fishType;
    Vector3[] fishPosition;
    Vector3[] fishVelocity;
    Vector3[] prevFishVelocity;
    bool[] running;
    float[] moveTimer;
    float[] animSpeed;
    float[] impulseTimer;
    MaterialPropertyBlock[] props;
    
    
    // Fish Count
    int activeFish = 0;
    private const int MAX_FISH = 1000;


    // Fish Details
    Camera _camera;
    Transform _cameraTransform;
    Transform _playerFishTrans;
    Vector3 playerFishPosition;
    PlayerController playerController;
    MaterialPropertyBlock propertyBlock;
    private const float CAMERA_DOT_LIMIT = 0.1f; 
    
    // Player Interactions
    private const float DRAG = 0.96f;
    private const float MOVE_TIMER_SET = 0.2f;
    private const int IMPULSE_TIMER_SET = 10;
    private const float RUN_DISTANCE = 10f;
    private const float EAT_DISTANCE = 2f;
    
    // Grouping
    private const int GROUP_SIZE = 3;
    private const float GROUP_TIME_SET = GROUP_SIZE * MOVE_TIMER_SET;
    float time;
    
    
    private void Awake() 
    {
        // Setup the property block
        propertyBlock = new MaterialPropertyBlock();
        
        // Get camera
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        
        // Get Player
        _playerFishTrans = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = _playerFishTrans.GetComponent<PlayerController>();
        
        fishType = new FishType[MAX_FISH];
        fishPosition = new Vector3[MAX_FISH];
        fishVelocity = new Vector3[MAX_FISH];
        prevFishVelocity = new Vector3[MAX_FISH];
        running = new bool[MAX_FISH];
        moveTimer = new float[MAX_FISH];
        animSpeed = new float[MAX_FISH];
        impulseTimer = new float[MAX_FISH];
        props = new MaterialPropertyBlock[MAX_FISH];
        
        for (int i = 0; i < MAX_FISH; i++)
        {
            //fishList[i] = newFishData;
            fishType[i] = FishType.clownFish;
            fishPosition[i] = Vector3.zero;
            fishVelocity[i] = Vector3.zero;
            prevFishVelocity[i] = Vector3.zero;
            running[i] = false; 
            moveTimer[i] = 0f;
            animSpeed[i] = 0f;
            impulseTimer[i] = 0f;
            props[i] = propertyBlock;
        } 
        
    }


    // Just for debuggin mass fish
    private void Start() 
    {
        int x, y, z;
        for (int i = 0; i < 50; i++)
        {
            x = Random.Range(-30, 30);
            y = Random.Range(-30, 30);
            z = Random.Range(-30, 30);
            AddFish(FishType.clownFish, new Vector3(x, y, z), Vector3.forward);
        }
    }

    
    void Update()
    {
        playerFishPosition = _playerFishTrans.position;
        time = Time.time;
       
        // Loop through every fish
        for (int i = 0; i < activeFish; i++)
        {
            // always move fish, even if not rendering
            int type = (int)fishType[i];
            MoveFish(type, i);
            
            //float camDot = Vector3.Dot(_cameraTransform.forward, fishList[i].position.normalized);           
            //if (camDot > CAMERA_DOT_LIMIT)
            //{
                
                Graphics.DrawMesh(  fishMesh[type],                             // Mesh
                                    fishPosition[i],                       // Position
                                    Quaternion.LookRotation(prevFishVelocity[i]),   // Rotation
                                    fishMaterials[type],                        // Material
                                    0,                                          // Layer
                                    _camera,                                    // Camera
                                    0,                                          // Submesh Index
                                    SetMaterialPropertyBlock(type, i),                                   // Material Property Block
                                    true,                                       // Casts Shadows
                                    true,                                       // Receives Shadows
                                    false                                       // Use Light Probes
                                    );
            //}
        }
        
    }
    
    
    void AddFish(FishType type, Vector3 position, Vector3 direction)
    {
        if (activeFish >= MAX_FISH)
        {
            Debug.LogWarning("Attempted to create more fish than max.");
            return;
        }
              
        fishType[activeFish] = type;
        fishPosition[activeFish] = position;
        fishVelocity[activeFish] = Vector3.zero;
        prevFishVelocity[activeFish] = Vector3.forward;
        running[activeFish] = false;
        animSpeed[activeFish] = 0f;
        impulseTimer[activeFish] = 0f;
        
        // Increase index last, because array starts at 0
        activeFish += 1;
    }
    
    
    void DeleteFish(int i)
    {
        // Swap last fish with the fish being removed - delete by overwrite
        activeFish -= 1;
        
        fishType[i] = fishType[activeFish];
        fishPosition[i] = fishPosition[activeFish];
        fishVelocity[i] = fishVelocity[activeFish];
        running[i] = running[activeFish];
        animSpeed[i] = animSpeed[activeFish];
        impulseTimer[i] = impulseTimer[activeFish];
        props[i] = props[activeFish];
    }
    
    
    void MoveFish(int type, int index)
    {
        
        Vector3 position = fishPosition[index];
        Vector3 velocity = fishVelocity[index];
        float maxSpeed = fishMaxSpeed[type];
                    
                               
        // If player is within a certain range, than make a dash impulse (on timer) away from player
        float distance = (playerFishPosition - position).magnitude;
        float runDistance = RUN_DISTANCE * _playerFishTrans.localScale.x;
        float eatDistance = EAT_DISTANCE * _playerFishTrans.localScale.x;
   
        
        // run away preset
        float xDiff = position.x - playerFishPosition.x;
        float yDiff = position.y - playerFishPosition.y;
        float zDiff = position.z - playerFishPosition.z;
        
        if (distance < runDistance)
            running[index] = true;
 
        if (running[index] == false)
        {
            xDiff = playerFishPosition.x - position.x;
            yDiff = playerFishPosition.y - position.y;
            zDiff = playerFishPosition.z - position.z;
        }
        
        if (distance > runDistance + 5)
            running[index] = false;
        
        float scaledMagnitude = maxSpeed / Mathf.Sqrt((xDiff * xDiff) + (yDiff * yDiff) + (zDiff * zDiff));
        float ACCEL_DELTA = 50 * Time.deltaTime * Time.deltaTime * 0.5f;
        velocity.x = xDiff * scaledMagnitude * ACCEL_DELTA;
        velocity.y = yDiff * scaledMagnitude * ACCEL_DELTA;
        velocity.z = zDiff * scaledMagnitude * ACCEL_DELTA;  
          
        // Get eaten if close enough   
        if (distance < eatDistance)
        {           
            playerController.EatObject(fishEatAmount[type]);
            DeleteFish(index);
            
            // Create some fish
            int num = Random.Range(0, 4);
            int randType = Random.Range(0, fishTypeList.Length);
            int x, y, z;
            FishType newType;
            for (int i = 0; i < num; i++)
            {
                newType = fishTypeList[randType];
                x = Random.Range(-30, 30);
                y = Random.Range(-30, 30);
                z = Random.Range(-30, 30);
                AddFish(newType, new Vector3(x, y, z), Vector3.forward);
            }
        }

        
        // Apply velocity calc to this fish
        if (velocity != Vector3.zero) 
            prevFishVelocity[index] = velocity;
        fishVelocity[index] = velocity;
        fishPosition[index] += velocity;
        animSpeed[index] += Time.deltaTime * velocity.magnitude * 10;
    }
    
    
    MaterialPropertyBlock SetMaterialPropertyBlock(int type, int index)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        
        mpb.SetFloat("_ZOffset", prop_ZOffset[type]);
        mpb.SetFloat("_Yaw", prop_Yaw[type]);
        mpb.SetFloat("_Roll", prop_Roll[type]);
        mpb.SetFloat("_Scale", prop_Scale[type]);    
        
        mpb.SetFloat("_Instance_Time", animSpeed[index]);
        
        return mpb;
    }
    
}
