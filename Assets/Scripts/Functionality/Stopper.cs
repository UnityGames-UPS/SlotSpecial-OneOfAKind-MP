using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stopper : MonoBehaviour
{
    [SerializeField] internal bool stop;
    [SerializeField] internal string stopAT;
    [SerializeField] internal Transform ImageTransform;
    [SerializeField] internal Transform ResultTransform;
    [SerializeField] private SlotBehaviour slotBehaviour;
    [SerializeField] private BackgroundController bgController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object has an Image component
        Image imageComponent = collision.gameObject.GetComponent<Image>();
        if(stopAT != "-1"){
            if (imageComponent != null && imageComponent.sprite != null && string.Equals(imageComponent.name, stopAT))
            {
                Debug.Log("Stopping wheel at: " + stopAT + " and object name = " + imageComponent.name);
                slotBehaviour.wheelStopped = true;
                bgController.StopWheel();
                ImageTransform = collision.transform;
            }
        }
        else{
            if(imageComponent.color.a==0){
                Debug.Log("Here");
                slotBehaviour.wheelStopped = true;
                bgController.StopWheel();
                ImageTransform = collision.transform;
            }
        }
    }
}
