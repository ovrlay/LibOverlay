using Valve.VR;
using UnityEngine;

public class Initialize : MonoBehaviour {

	void Start () {
        var error = EVRInitError.None;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
        if (error != EVRInitError.None)
        {
            Debug.Log(error.ToString());
            enabled = false;
            return;
        }
    }
}
