using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JustForFunLoadingThing : MonoBehaviour
{
    [SerializeField] private List<Sprite> loadingSprites;
    private Image image;
    private int counter = 0;

    private void Start()
    {
        image = GetComponent<Image>();
        InvokeRepeating(nameof(ChangeSprite), 0f, 0.3f);
    }
    private void ChangeSprite()
    {
        if (loadingSprites.Count == 0) 
            return;
        
        if (counter >= loadingSprites.Count) 
            counter = 0;
        
        image.sprite = loadingSprites[counter];
        counter++;
    }

    private void OnEnable()
    {
        counter = 0;
    }
}
