using System;
using UnityEngine;
using System.Collections;
namespace Q3Movement
{
    /// <summary>
    /// Custom script based on the version from the Standard Assets.
    /// </summary>
    [Serializable]
    public class MouseLook
    {
        [SerializeField] private float m_XSensitivity = 2f;
        [SerializeField] private float m_YSensitivity = 2f;
        [SerializeField] private bool m_ClampVerticalRotation = true;
        [SerializeField] private float m_MinimumX = -90F;
        [SerializeField] private float m_MaximumX = 90F;
        [SerializeField] private bool m_Smooth = false;
        [SerializeField] private float m_SmoothTime = 7f;
        [SerializeField] private bool m_LockCursor = true;
        [SerializeField] private float m_tiltSmoothness = 7f;
        private bool m_rotastrafe = false;
        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        public void SetSmooth(bool value, float smoothtime)
        {
            m_Smooth = value;
            m_SmoothTime = smoothtime;
        }

    public void LookRotation(Transform character, Transform camera)
        {
            m_Smooth = MainGameObject.Instance.s_alwaysHardStrafeInAir;
            if (m_LockCursor == true) {
                float yRot = Input.GetAxis("Mouse X") * m_XSensitivity;
                float xRot = Input.GetAxis("Mouse Y") * m_YSensitivity;
                
                if (m_Smooth) { 
                    yRot = Mathf.Clamp(Input.GetAxis("Mouse X") * m_XSensitivity, -m_tiltSmoothness*Time.deltaTime*200f, m_tiltSmoothness * Time.deltaTime * 200f); //hell yeah no more jitter :D
                    xRot = Mathf.Clamp(Input.GetAxis("Mouse Y") * m_XSensitivity, -m_tiltSmoothness * Time.deltaTime * 200f, m_tiltSmoothness * Time.deltaTime * 200f);
                }

                if (m_rotastrafe && Input.GetAxisRaw("Vertical") <= 0f) { yRot += Input.GetAxisRaw("Horizontal"); }
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

        public void SetRotastrafe(bool value)
        {
            m_rotastrafe = value;
        }
        public bool GetRotastrafe()
        {
            return m_rotastrafe;
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

        float temptilt = 0.0f;
        public void SetTilt(float tilt) {
            tilt = Mathf.Clamp(tilt, -15f, 15f);
            temptilt = Mathf.Lerp(temptilt, tilt, 1f/m_tiltSmoothness);
            
            if (!MainGameObject.Instance.s_disableHeadTilt) { 
                m_CameraTargetRot = ClampRotationAroundXAxis(Quaternion.Euler(m_CameraTargetRot.eulerAngles.x,
                    m_CameraTargetRot.eulerAngles.y*0.0f, //DANGER DANGER DANGER
                    temptilt * 3.0f));
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
}
