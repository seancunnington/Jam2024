using UnityEngine;


public class PlayerController : MonoBehaviour
{
    
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
        // Check that public values have been set
        if (sfx_Jump == null) { print("PlayerController script missing: sfx_Jump"); }
        
        // SFX
        CreateAudioSource();
          
    }


    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();   
        }
    }
    
    
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
}
