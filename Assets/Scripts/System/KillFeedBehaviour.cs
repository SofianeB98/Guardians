using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedBehaviour : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float speedFadeOut = 2f;
    
    private void Update()
    {
        this.image.color = Color.Lerp(this.image.color,
            new Color(this.image.color.r, this.image.color.g, this.image.color.b, 0), Time.deltaTime * this.speedFadeOut);

        this.text.color = Color.Lerp(this.text.color,
            new Color(this.text.color.r, this.text.color.g, this.text.color.b, 0), Time.deltaTime * this.speedFadeOut);
        
    }
}
