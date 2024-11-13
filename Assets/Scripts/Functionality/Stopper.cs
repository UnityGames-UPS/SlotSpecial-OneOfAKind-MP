using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Debug.Log("on trigger enter");
        // Check if the collided object has an Image component
        if(stop){
            Debug.Log("Inside stop");
            if(stopAT != "-1"){
                Debug.Log("Inside -1");
                if (collision.name==stopAT)
                {
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping wheel at: " + stopAT + " and object name = " + collision.name);
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
            else{
                if(collision.name == "Empty"){
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping at a empty space");
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Debug.Log("on trigger exit");
        // Check if the collided object has an Image component
        if(stop){
            Debug.Log("Inside stop");
            if(stopAT != "-1"){
                Debug.Log("Inside -1");
                if (collision.name==stopAT)
                {
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping wheel at: " + stopAT + " and object name = " + collision.name);
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
            else{
                if(collision.name == "Empty"){
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping at a empty space");
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        // Debug.Log("on trigger stay");
        // Check if the collided object has an Image component
        if(stop){
            Debug.Log("Inside stop");
            if(stopAT != "-1"){
                Debug.Log("Inside -1");
                if (collision.name==stopAT)
                {
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping wheel at: " + stopAT + " and object name = " + collision.name);
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
            else{
                if(collision.name == "Empty"){
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    stop = false;
                    Debug.Log("Stopping at a empty space");
                    slotBehaviour.wheelStopped = true;
                    bgController.StopWheel();
                    ImageTransform = collision.transform;
                }
            }
        }
    }
}
