using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class levynkansimotion : MonoBehaviour
{

        [SerializeField] private float m_XSensitivity = 2f;
        [SerializeField] private float m_YSensitivity = 2f;
        [SerializeField] private bool m_ClampVerticalRotation = true;
        [SerializeField] private float m_MinimumX = -90F;
        [SerializeField] private float m_MaximumX = 90F;
        [SerializeField] private bool m_Smooth = false;
        [SerializeField] private float m_SmoothTime = 5f;
        [SerializeField] private bool m_LockCursor = true;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;
    private void Start()
    {
        Init(transform, transform);
    }
    private void Update()
    {
        LookRotation(transform, transform);

    }

    public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera)
        {

                float yRot = Input.GetAxis("Mouse X") * m_XSensitivity;
                float xRot = Input.GetAxis("Mouse Y") * m_YSensitivity;

                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (m_ClampVerticalRotation)
                {
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
                }

                if (m_Smooth)
                {
                    character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                        m_SmoothTime * Time.deltaTime);
                    camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                        m_SmoothTime * Time.deltaTime);
                }
                else
                {
                    character.localRotation = m_CharacterTargetRot;
                    camera.localRotation = m_CameraTargetRot;
                }

            UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            m_LockCursor = value;
            if (!m_LockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        public bool GetCursorLock()
        {
            return m_LockCursor;
        }
        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (m_LockCursor)
            {
                InternalLockUpdate();
            }
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }


        public void SetTilt(float tilt)
        {
            if (!MainGameObject.Instance.s_disableHeadTilt)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(Quaternion.Euler(m_CameraTargetRot.eulerAngles.x,
                    m_CameraTargetRot.eulerAngles.y,
                    tilt));
            }
        }
        public void AddFallTilt(float tilt)
        {

            m_CameraTargetRot = ClampRotationAroundXAxis(
                Quaternion.Euler(Mathf.Lerp(tilt, m_CameraTargetRot.eulerAngles.x, 0.0f),
                m_CameraTargetRot.eulerAngles.y,
                m_CameraTargetRot.eulerAngles.z));

        }
        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }

