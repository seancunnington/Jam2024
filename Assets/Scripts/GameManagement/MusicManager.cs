using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
 
    [Header("Settings")]
    [SerializeField, Range(0.0f, 1.0f)] float masterVolume = 1f;
    [SerializeField] int targetClip = 0;
    [SerializeField] bool increasingSource = false;
    [SerializeField] bool swapStyles = false;
    [SerializeField] float progressTracker = 0f;
    [SerializeField] float progressSpeed = 1f;
    
    
    // Kyles Music
    [SerializeField] List<AudioClip> list_KyleSongs; 
    List<AudioSource> list_KyleSources;
    bool kyleMusicActive = false;
    
    // Maggies Music
    [SerializeField] List<AudioClip> list_MaggieSongs; 
    List<AudioSource> list_MaggieSources;
    
    
    
    private void Awake() 
    {
        // Create kyles audio sources
        GameObject kyleMusic = new GameObject("Kyle Music");
        kyleMusic.transform.parent = transform;
        
        list_KyleSources = new List<AudioSource>();
        
        for (int i = 0; i < list_KyleSongs.Count; i++)
        {
            GameObject newSong = new GameObject(list_KyleSongs[i].name);
            newSong.transform.parent = kyleMusic.transform;
            
            AudioSource newSource = newSong.AddComponent<AudioSource>();
            newSource.clip = list_KyleSongs[i];
            newSource.loop = true;
            newSource.playOnAwake = false;
            
            if (i == 0)
                newSource.volume = 1f;
            else 
                newSource.volume = 0f;
            
            list_KyleSources.Add(newSource);
        }
        
        // Create maggies audio sources
        GameObject maggieMusic = new GameObject("Maggie Music");
        maggieMusic.transform.parent = transform;
        
        list_MaggieSources = new List<AudioSource>();
        
        for (int i = 0; i < list_MaggieSongs.Count; i++)
        {
            GameObject newSong = new GameObject(list_MaggieSongs[i].name);
            newSong.transform.parent = maggieMusic.transform;
            
            AudioSource newSource = newSong.AddComponent<AudioSource>();
            newSource.clip = list_MaggieSongs[i];
            newSource.loop = true;
            newSource.playOnAwake = false;
    
            newSource.volume = 0f;
            
            list_MaggieSources.Add(newSource);
        }
        
        targetClip = 0;
        increasingSource = false;
        swapStyles = false;
        
        PlayKyleSongs(true);
        //PlayMaggieSongs(true);
    }

    
    void Update()
    {
        
        // increase an audio source volume
        if (increasingSource == true)
        {
            if (kyleMusicActive == true)
            {
                progressTracker = Mathf.MoveTowards(progressTracker, 1f, progressSpeed * Time.deltaTime);
                list_KyleSources[targetClip].volume = progressTracker;
                increasingSource = progressTracker >= 1f ? false : true;
            }
            else            
            {
                progressTracker = Mathf.MoveTowards(progressTracker, 1f, progressSpeed * Time.deltaTime);
                list_MaggieSources[targetClip].volume = progressTracker;
                increasingSource = progressTracker >= 1f ? false : true;
            }
        }
        
        // swap between kyle and maggie
        else if (swapStyles == true)
        {
            progressTracker = Mathf.MoveTowards(progressTracker, 1f, progressSpeed * Time.deltaTime);
            
            // from Kyle to Maggie
            if (kyleMusicActive == false)
            {
                for (int i = 0; i < list_KyleSources.Count; i++)
                    list_KyleSources[i].volume = 1 - progressTracker;
                    
                list_MaggieSources[0].volume = progressTracker;
                
                if (progressTracker >= 1f){
                    swapStyles = false;
                    PlayKyleSongs(false);
                }
            }
            else            
            {
                for (int i = 0; i < list_MaggieSources.Count; i++)
                    list_MaggieSources[i].volume = 1 - progressTracker;
                    
                list_KyleSources[0].volume = progressTracker;
                
                if (progressTracker >= 1f){
                    swapStyles = false;
                    PlayMaggieSongs(false);
                }
            }
             
        }
        
        // else reset lerping values
        else
        {
            progressTracker = 0f;
        }
    }
    
    
    public void IncreaseNextSource()
    {
        if (increasingSource == true)
            return;     
        
        if (kyleMusicActive == true)
        {
            if (targetClip < list_KyleSongs.Count-1)
            {
                targetClip++;
                increasingSource = true;
            }
            else 
            {
                targetClip = 0;
                swapStyles = true;
                kyleMusicActive = false;
                PlayMaggieSongs(true);
            }
        }
        else
        {
            if (targetClip < list_MaggieSongs.Count-1)
            {
                targetClip++;
                increasingSource = true;                
            }
            else
            {
                targetClip = 0;
                swapStyles = true;
                kyleMusicActive = true;
                PlayKyleSongs(true);
            }
        }
    }
    
    
    void PlayKyleSongs(bool value)
    {
        if (value == true)
        {
            for (int i = 0; i < list_KyleSongs.Count; i++)
            {
                list_KyleSources[i].Play();
            }
            kyleMusicActive = true;
        }
        else
        {
            for (int i = 0; i < list_KyleSongs.Count; i++)
            {
                list_KyleSources[i].Stop();
            }
            kyleMusicActive = false;
        }
    }
    
    void PlayMaggieSongs(bool value)
    {
        if (value == true)
        {
            for (int i = 0; i < list_MaggieSongs.Count; i++)
            {
                list_MaggieSources[i].Play();
            }
        }
        else
        {
            for (int i = 0; i < list_MaggieSongs.Count; i++)
            {
                list_MaggieSources[i].Stop();
            }
        }
    }
}
