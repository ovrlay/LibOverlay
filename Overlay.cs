using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Random = System.Random;

namespace LibOverlay
{
    class Overlay
    {
        private Texture _overlayTexture;
        public Texture OverlayTexture { get { return _overlayTexture; } set { _overlayTexture = value; UpdateTexture(); } }
        private Vector4 UvOffset = new Vector4(0, 0, 1, 1);
        public float Alpha = 1;
        private readonly string key;
        private readonly string name;
        public ulong Handle { get { return _handle; } }
        protected ulong _handle = OpenVR.k_ulOverlayHandleInvalid;
        private bool enabled = false;
        private TrackedDevice tracked;
        public TrackedDevice TrackedDevice { get { return tracked; } set { tracked = value; AttachToDevice(tracked); } }
        private HmdMatrix34_t _hmdTransform;
        private Transform _transform;
        public Transform Transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                var offset = new SteamVR_Utils.RigidTransform(OverlayReference.transform, _transform);
                offset.pos.x /= OverlayReference.transform.localScale.x;
                offset.pos.y /= OverlayReference.transform.localScale.y;
                offset.pos.z /= OverlayReference.transform.localScale.z;
                _hmdTransform = offset.ToHmdMatrix34();
                UpdateOverlayTransform();
            }
        }

        public Overlay(string key, string name)
        {
            this.key = key;
            this.name = name;
            var offset = new SteamVR_Utils.RigidTransform
            {
                rot = Quaternion.identity
            };
            _hmdTransform = offset.ToHmdMatrix34();
        }

        public void Enable()
        {
            var svr = SteamVR.instance;
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;
            var error = overlay.CreateOverlay(key, name, ref _handle);
            if (error != EVROverlayError.None)
            {
                Debug.Log(error.ToString());
                enabled = false;
                return;
            }
            OverlayReference.transform.localRotation = Quaternion.identity;

            InitOverlay();
        }

        public void Disable()
        {
            if (_handle == OpenVR.k_ulOverlayHandleInvalid) return;
            var overlay = OpenVR.Overlay;
            if (overlay != null) overlay.DestroyOverlay(_handle);
            _handle = OpenVR.k_ulOverlayHandleInvalid;
        }

        private void InitOverlay()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;
            if (OverlayTexture != null)
            {
                var error = overlay.ShowOverlay(_handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(key, ref _handle) != EVROverlayError.None) return;
                }
                var tex = new Texture_t
                {
                    handle = OverlayTexture.GetNativeTexturePtr(),
                    eType = SteamVR.instance.textureType,
                    eColorSpace = EColorSpace.Auto
                };
                overlay.SetOverlayColor(_handle, 1f, 1f, 1f);
                overlay.SetOverlayTexture(_handle, ref tex);
                overlay.SetOverlayAlpha(_handle, Alpha);
                var textureBounds = new VRTextureBounds_t
                {
                    uMin = (0 + UvOffset.x) * UvOffset.z,
                    vMin = (1 + UvOffset.y) * UvOffset.w,
                    uMax = (1 + UvOffset.x) * UvOffset.z,
                    vMax = (0 + UvOffset.y) * UvOffset.w
                };
                overlay.SetOverlayTextureBounds(_handle, ref textureBounds);
                UpdateOverlayTransform();
                enabled = true;
            }
            else
            {
                overlay.HideOverlay(_handle);
            }
        }

        public void UpdateTexture(bool refresh = false)
        {
            if ((refresh && OverlayTexture == null) || !enabled) return;
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;
            var tex = new Texture_t
            {
                handle = OverlayTexture.GetNativeTexturePtr(),
                eType = SteamVR.instance.textureType,
                eColorSpace = EColorSpace.Auto
            };
            overlay.SetOverlayTexture(_handle, ref tex);
        }

        public GameObject OverlayReference
        {
            get
            {
                return _overlayReference ?? (_overlayReference = new GameObject("Overlay Reference" + GetType()));
            }
        }
        private GameObject _overlayReference;


        private void AttachToDevice(TrackedDevice device)
        {
            if (tracked != device)
            {
                tracked = device;
                UpdateOverlayTransform();
            }
        }

        private void UpdateOverlayTransform()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null) return;
            if (tracked == null)
            {
                overlay.SetOverlayTransformAbsolute(_handle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _hmdTransform);
            }
            else
            {
                overlay.SetOverlayTransformTrackedDeviceRelative(_handle, tracked.index, ref _hmdTransform);
            }
        }
    }
}
