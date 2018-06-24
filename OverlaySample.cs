using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using LibOverlay;
using Valve.VR;

class OverlaySample : MonoBehaviour
{
    public Texture OverlayTexture;
    public float Alpha = 1;
    public static Random Rand = new Random();
    public static string Key { get { return "unity:" + Application.companyName + "." + Application.productName + "." + Rand.Next(); } }
    private Overlay overlay;

    public void OnEnable()
    {
        var error = EVRInitError.None;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
        if (error != EVRInitError.None)
        {
            Debug.Log(error.ToString());
            enabled = false;
            return;
        }

        overlay = new Overlay(Key + gameObject.GetInstanceID(), gameObject.name)
        {
            OverlayTexture = OverlayTexture,
            Alpha = Alpha
        };
        overlay.TrackedDevice = TrackedDevice.GetController(TrackedDevice.ControllerRole.RightHand);
        overlay.Enable();
        overlay.Transform = gameObject.transform;
    }

    public void OnDisable()
    {
        overlay.Disable();
    }

}
