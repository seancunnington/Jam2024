using UnityEngine;


public class PlayerController : MonoBehaviour
{
    
    // Outside Attributes
    Transform _cameraTransform;
    [SerializeField] Transform _modelTransform;
    
    // Player Attributes and Movement
    Transform _transform;
    Rigidbody _body;
    Vector3 velocity = Vector3.zero;
    
    
    [SerializeField, Range(1f, 20f)] float maxSpeed = 5f;
    [SerializeField, Range(1f, 20f)] float acceleration = 5f;
    [SerializeField, Range(0.1f, 1.0f)] float drag = 1f;
    
    bool drifting = false;
    bool activateDash = false;
    Vector3 driftingDirection = Vector3.zero;
    [SerializeField, Range(1f, 30f)] float maxDashSpeed = 5f;
    float dashStrength = 0f;
    
    
    
    // SFX
    private AudioSource _audioSource;
    public AudioClip sfx_Jump = null;
    
    
    private void CreateAudioSource()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        
        _audioSource.clip = sfx_Jump;
    }
    
    

    void Awake()
    {
        // Get components
        _cameraTransform = Camera.main.transform;
        _transform = this.transform;
        _body = GetComponent<Rigidbody>();
        _body.useGravity = false;
        
        // Check that public values have been set
        if (sfx_Jump == null) { print("PlayerController script missing: sfx_Jump"); }
        
        // SFX
        CreateAudioSource();
          
    }


    void Update()
    {
        
        // Rotate fish to face forward direction
        _modelTransform.rotation = Quaternion.LookRotation(velocity);
        
        
        DriftAndDash();
        
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
