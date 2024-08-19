using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.UIElements.Experimental;


public class PlayerController : MonoBehaviour
{
    
    // Outside Attributes
    Transform _cameraTransform;
    OrbitCamera _orbitCamera;
    
    // Player Attributes and Movement
    [Header("Movement")]
    [SerializeField] Transform _modelTransform;
    Transform _transform;
    Rigidbody _body;
    Vector3 velocity = Vector3.zero;
    
    [SerializeField, Range(1f, 20f)] float maxSpeed = 5f;
    [SerializeField, Range(1f, 20f)] float acceleration = 5f;
    
    // Dashing
    bool drifting = false;
    bool activateDash = false;
    Vector3 driftingDirection = Vector3.zero;
    [SerializeField, Range(0.1f, 1.0f)] float drag = 1f;
    [SerializeField, Range(1f, 30f)] float maxDashSpeed = 5f;
    float dashStrength = 0f;
    
    // Level and Stomach
    public int currentLevel { get; private set;}
    [Header("Level and Stomach")]
    [SerializeField] int maxLevel = 10; // the highest level that can be achieved
    [SerializeField] int expCap = 1000; // the total exp needed to reach the maxLevel
    [SerializeField] AnimationCurve curve_levelUp;
    public int stomachMeter { get; private set;}
    public int stomachMeterTarget {get; private set;} // the next number the stomachMeter needs to get to after eating something with a food amount
    public int previousStomachMeter {get; private set;}
    public int stomachMeterLevelUp {get; private set;} // the amount needed to level up
    [SerializeField] float stomachMeterUpdateSpeed = 10f;
    float stomachMeterProgressTracker = 0f;
    EasingFunction.Function ease_cubic = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutCubic);
    EasingFunction.Function ease_sine = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutSine);
    
    // Scaling
    float previousScale = 1f;
    float currentScale = 1f;
    float nextScale = 1f;
    float scaleProgessTracker = 0f;
    [Header("Scaling")]
    [SerializeField] int maxScale = 10;
    [SerializeField] AnimationCurve curve_fishScaling;
    [SerializeField] float scaleUpSpeed = 1f;
    
    // SFX
    private AudioSource _audioSource;
    [Header("SFX")]
    public AudioClip sfx_Jump = null;
    
    
    private void CreateAudioSource()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        
        _audioSource.clip = sfx_Jump;
    }
    
    
    //-----------------------------------------//
    //                  Setup                  //
    //-----------------------------------------//

    void Awake()
    {
        // Get components
        _cameraTransform = Camera.main.transform;
        _orbitCamera = Camera.main.transform.GetComponent<OrbitCamera>();
        _transform = this.transform;
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        
        // Set up stomach and leveling
        currentLevel = 1;
        stomachMeter = 0;
        stomachMeterTarget = 0;
        previousStomachMeter = 0;
        SetNextStomachLevelUpAmount();
        
        // Set up scaling
        previousScale = 1f;
        currentScale = 1f;
        _orbitCamera.fishScale = currentScale;
        nextScale = 1f;
        scaleProgessTracker = 0f;
        
        // Check that public values have been set
        if (sfx_Jump == null) { print("PlayerController script missing: sfx_Jump"); }
        
        // SFX
        CreateAudioSource();
          
    }


    //-----------------------------------------//
    //               Update Cycles             //
    //-----------------------------------------//

    void Update()
    {       
        // Rotate fish to face forward direction
        _modelTransform.rotation = Quaternion.LookRotation(velocity);
        
        // Dashing Mechanics
        DriftAndDash();
        
        // Update the stomach amount and scale over time based on what's been eaten and current level
        UpdateStomachLevelScale();
    }
    
    
    void FixedUpdate()
    {
        // Keep track of PhsyX velocity
        velocity = _body.velocity;
        
        AdjustVelocity();
        
        // Make any velocity changes here
        if (activateDash == true)
        {
            activateDash = false;
            velocity += _cameraTransform.forward * dashStrength;
        }
        
        // Update the PhysX velocity
        _body.velocity = velocity;
    }
    
    
    //-----------------------------------------//
    //      Velocity and Character Actions     //
    //-----------------------------------------//
    
    void AdjustVelocity()
    {   
        Vector3 desiredVelocity;
        
        if (drifting)
        {
            desiredVelocity = driftingDirection * drag;
        }
        else
        {
            desiredVelocity = _cameraTransform.forward * maxSpeed;
        }
        
        float maxSpeedChange = acceleration * Time.deltaTime;
        
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
    }
    
    
    void DriftAndDash()
    {
        // Holding 'Space' down to drift
        if (Input.GetButton("Space"))
        {
            drifting = true;
            
            // save the direction to drift in
            if (driftingDirection == Vector3.zero)              
                driftingDirection = _cameraTransform.forward;
                
            // set dash strength
            dashStrength = maxDashSpeed;
        }
        
        // Release 'Space' to dash
        if (Input.GetButtonUp("Space"))
        {
            drifting = false;
            driftingDirection = Vector3.zero;   // reset the drifting direction to zero, preparing for next drift
            activateDash = true;                // activate the dash
        }
    }
    
    
    //-----------------------------------------//
    //      Stomach, Leveling and Scaling      //
    //-----------------------------------------//
    
    void UpdateStomachLevelScale()
    {
        // Update stomach meter
        if (stomachMeter < stomachMeterTarget)
        { 
            stomachMeter = (int)Mathf.Floor(ease_cubic(previousStomachMeter, stomachMeterTarget, stomachMeterProgressTracker));
            stomachMeterProgressTracker += Time.deltaTime * stomachMeterUpdateSpeed;
        }
        
        // Update fish level and next scale amount
        if (stomachMeter > stomachMeterLevelUp)
        {
            currentLevel += 1;                  // level up
            SetNextStomachLevelUpAmount();     // get stomach amount for next level up
            SetNextScaleAmount();
        }
        
        // Update scale
        if (currentScale < nextScale && stomachMeterProgressTracker >= 1)
        {
            ScaleUp();
        }
    }
    
    
    void SetNextStomachLevelUpAmount()
    {
        float t = ((float)currentLevel+1) / maxLevel;
        stomachMeterLevelUp = (int)Mathf.Floor(curve_levelUp.Evaluate(t) * expCap);
    }
    
    
    public void EatObject(int foodAmount)
    {
        stomachMeterProgressTracker = 0f;
        previousStomachMeter = stomachMeter;
        stomachMeterTarget += foodAmount;
    }
    
    
    void SetNextScaleAmount()
    {
        float t = ((float)currentLevel) / maxLevel;
        nextScale = 1 + curve_fishScaling.Evaluate(t) * maxScale; // add to 1, since fish scale starts at 1
        previousScale = currentScale;
        scaleProgessTracker = 0f;
    }
    
    
    void ScaleUp()
    {         
        currentScale = ease_cubic(previousScale, nextScale, scaleProgessTracker);
        scaleProgessTracker += Time.deltaTime * scaleUpSpeed;
        
        _transform.localScale = Vector3.one * currentScale;
        _orbitCamera.fishScale = currentScale; // also need to increase distance of camera to fish, as fish grows
    }
    
    
    //-----------------------------------------//
    //                 Debugging               //
    //-----------------------------------------//
    
    private void OnDrawGizmos() {
        
        // Fish forward direction
        Gizmos.color = Color.red;
        Vector3 end = transform.position + (Camera.main.transform.forward * 5);
        Gizmos.DrawLine(transform.position, end);
    }
    
    
    /*
    // Performs jump mechanics and plays jump sound.
    void Jump()
    {
        //print("Jump button pressed");
        
        // gaurds
        if(sfx_Jump == null)
        {
            print("Jump sound has not been set - add jump sound to PlayerController");
            return;
        }
        
        // jump mechanics
        
        
        // attach and play the jump sound clip
        if(_audioSource.clip != sfx_Jump)
        {
            _audioSource.clip = sfx_Jump;
        }
        _audioSource.Play();
        
    }
    */
    
}
