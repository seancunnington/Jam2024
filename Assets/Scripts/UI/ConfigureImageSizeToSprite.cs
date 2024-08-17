


#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;

/*
    This script resizes an attached UI Image component to the size of the attached sprite.
    - does nothing if either the UI Image component or sprite are missing.
*/


[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class ConfigureImageSizeToSprite : MonoBehaviour
{
  
    private Image image;  
    
    [SerializeField] float imageScale = 1;

   
    // Resize UI Image transform to same dimensions as Sprite - does not affect scale.
    void Update()
    {
        // Gaurds
        if( image == null)
            image = GetComponent<Image>();

        if( image.sprite == null ) // if a sprite is not attached, do nothing.
        {
            return;
        }
            
     
        // Resizing
        float width = image.sprite.rect.width;
        float height = image.sprite.rect.height;
        
        if( image.rectTransform.sizeDelta.x != width || image.rectTransform.sizeDelta.y != height)
            image.rectTransform.sizeDelta = new Vector2(width, height);


        // Scaling
        if( image.rectTransform.localScale.x != imageScale)
            image.rectTransform.localScale = new Vector3(imageScale, imageScale, imageScale);

    }
}

#endif
