using UnityEngine;
using Valve.VR;

namespace LibOverlay
{
    class TrackedDevice
    {
        public readonly uint index;
        public string Name { get { return getStringProperty(ETrackedDeviceProperty.Prop_RenderModelName_String);  } }

        public enum ControllerRole
        {
            RightHand = ETrackedControllerRole.RightHand,
            LeftHand = ETrackedControllerRole.LeftHand,
        }

        public static TrackedDevice GetController(ControllerRole type)
        {
            uint index = OpenVR.System.GetTrackedDeviceIndexForControllerRole((ETrackedControllerRole)(int)type);
            return new TrackedDevice(index);
        }

        public static TrackedDevice GetHMD()
        {
            return new TrackedDevice(0);
        }

        TrackedDevice(uint index)
        {
            this.index = index;
        }

        private string getStringProperty(ETrackedDeviceProperty property)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var result = new System.Text.StringBuilder(64);
            OpenVR.System.GetStringTrackedDeviceProperty(index, property, result, 64, ref error);
            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                Debug.Log(index);
                Debug.Log("error while getting property " + property + " from device " + index + ": " + error);
            }
            return result.ToString();
        }
    }
}
