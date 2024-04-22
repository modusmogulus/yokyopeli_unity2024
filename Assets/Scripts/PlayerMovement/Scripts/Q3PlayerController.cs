using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using EasyTransition;
using static DamageTypes;
using WiimoteApi;

namespace Q3Movement
{
    /// <summary>
    /// This script handles Quake III CPM(A) mod style player movement logic.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Q3PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public class MovementSettings
        {
            public float MaxSpeed;
            public float Acceleration;
            public float Deceleration;

            public MovementSettings(float maxSpeed, float accel, float decel)
            {
                MaxSpeed = maxSpeed;
                Acceleration = accel;
                Deceleration = decel;
            }
        }

        [Header("Aiming")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private GameObject m_Head;
        [SerializeField] private GameObject m_HeadAnchor;
        [SerializeField] public MouseLook m_MouseLook = new MouseLook();
        [Header("Movement")]
        [SerializeField] private float m_Friction = 6;
        [SerializeField] private float m_Gravity = 20;
        [SerializeField] public float m_MaxFallSpeed = 320f;
        [SerializeField] private float m_JumpForce = 8;
        [SerializeField] public int m_maxWallrunningStamina = 30;
        private float defaultFOV;
        [Tooltip("Automatically jump when holding jump button")]
        [SerializeField] private bool m_AutoBunnyHop = false;
        [SerializeField] public AudioSource m_wallrunningSoundLoop;
        [Tooltip("How precise air control is")]
        [SerializeField] private float m_AirControl = 0.7f;
        [SerializeField] public FootstepPlayer ft_player;
        [SerializeField] private MovementSettings m_GroundSettings = new MovementSettings(7, 14, 10);
        [SerializeField] private MovementSettings m_AirSettings = new MovementSettings(7, 2, 2);
        [SerializeField] private MovementSettings m_StrafeSettings = new MovementSettings(1, 50, 50);
        [SerializeField] private Animator headAnimator;
        [SerializeField] public float m_TiltAmount = 1.0f;
        [SerializeField] public AudioSource m_SpeedWindSound;
        [SerializeField] public AudioSource m_SlideSound;
        [SerializeField] public AudioSource MouthAudioSource;
        private Vector3 slopeNormal;
        
        [SerializeField] public PostProcessVolume speedEffect;
        private DamageTypes lastDamageType;
        private int wallrunningStamina = 0;
        private bool isGoingTowardsWall = false;
        public TransitionSettings deathTransition;
        public Vector3 savePos;
        public Vector3 saveVelo;
        /// <summary>
        /// Returns player's current speed.
        /// </summary>
        public float Speed { get { return m_Character.velocity.magnitude; } }

        private CharacterController m_Character;
        private Vector3 m_MoveDirectionNorm = Vector3.zero;
        private Vector3 m_PlayerVelocity = Vector3.zero;

        // Used to queue the next jump just before hitting the ground.
        private bool m_JumpQueued = false;
        private bool isCurrentlyGrounded = true;
        // Used to display real time friction values.
        private float m_PlayerFriction = 0;
        public bool m_inRollZone = false;
        private Vector3 m_MoveInput;
        private Wiimote wiimote;
        private Transform m_Tran;
        private Transform m_CamTran;
        public bool m_IsRolling = false;
        private bool m_IsCrouching = false;
        private bool m_WasGrounded = false;
        private float m_CrouchHeight = 0.5f;
        private float m_OriginalHeight;
        private float m_LandingTimer = 0.0f;
        private float m_currentfallSpeed = 0.0f;
        private bool intentsRoll;
        private bool damageApplied;
        private bool hasRolled;
        public bool showControlTips = false;
        public bool indoorWalking = false;
        private bool wasWallrunning = false;
        //-- HP --
        public float maxhp = 100.0f;
        public float health = 100.0f;
        public PostProcessVolume damageFx;
        public PostProcessVolume fallFx;
        [SerializeField] public bool m_parkourEnabled = true;
        [SerializeField] public bool m_wallrunningAllowed = true;
        private bool isWallRunning = false;
        private bool hasKickJumped = false;
        private bool isOnSlope = false;

        public bool saveGame = true;
        //IMPORTANT ABOUT snowInBoots:
        //----------- * snowInBoots is mostly handled in FootstepPlayer script * -----------
        //FootstepPlayer can be found in the object called Head (or whatever Body idk)
        //Snow in boots is also handled in: GameObject, MainUIScript (on canvas), FootstepPlayer, Q3PlayerController
        //It is handled in footstep player to SYNC walking and snow in boots
        //Also capslock is to help future you with ADHD, not for agressive tone :3

        private int snowInBoots = 0;
        private bool snowBootMechanics = false;

        private void Start()
        {
            health = maxhp;
            m_Tran = transform;
            m_Character = GetComponent<CharacterController>();
            m_OriginalHeight = m_Character.height;

            if (!m_Camera)
                m_Camera = Camera.main;

            m_CamTran = m_Camera.transform;
            m_MouseLook.Init(m_Tran, m_CamTran);
            headAnimator = m_Head.GetComponent<Animator>();
            MainGameObject.Instance.playerController = this;
            defaultFOV = m_Camera.fieldOfView;
        }


        public void CheckAndUpdateCrouch()
        {
            
            if (Input.GetKey(KeyCode.LeftShift)) {
                m_IsCrouching = true;
                if (m_Character.height > m_CrouchHeight) {
                    m_Character.height -= 0.3f;
                    m_Character.transform.Translate(0f, -0.0515f, 0f);
                }
            }
            else
            {
                if (m_Character.height < m_OriginalHeight)
                {
                    m_IsCrouching = false;
                    m_Character.height += 0.3f;
                    m_Character.transform.Translate(0f, 0.0555f, 0f);
                }
                else
                {
                    m_Character.height = m_OriginalHeight;
                }
            }
        }


        public IEnumerator RollCoroutine()
        {
            bool m_IsRolling = true;

            if (headAnimator != null)
            {

                m_PlayerVelocity = m_PlayerVelocity + transform.forward * 10;
                headAnimator.SetTrigger("Roll");
                AudioManager.Instance.PlayAudio("FT_Roll");

            }

            else
            {
                Debug.LogError("Animator component not found on m_Head object!");
            }

            yield return new WaitForSeconds(0.1f);

            if (headAnimator != null)
            {
                headAnimator.ResetTrigger("Roll");

            }

            m_IsRolling = false;
            hasRolled = true;
        }

        public int GetSnowInBoots()
        {
            return snowInBoots;
        }

        public int SetSnowInBoots(int value)
        {
            snowInBoots = Mathf.Max(value, 9);
            return Mathf.Max(value, 9);
        }

        public bool GetSnowBootMechanics(bool value) { return snowBootMechanics; }
        public bool SetSnowBootMechanics(bool value) { return snowBootMechanics = value; }



        private void ApplyFallDamage(float multiplier, float threshold)
        {
            var sphereCastVerticalOffset = m_Character.height / 2 - m_Character.radius;
            var castOrigin = transform.position - new Vector3(0, sphereCastVerticalOffset, 0);



            if (m_currentfallSpeed > 3)
            {
                AudioManager.Instance.PlayAudio("SFX_JumpUp");
                ft_player.PlayOtherMaterialSound(FootstepPlayer.MATERIAL_SOUND_TYPE.FOOTSTEP);
            }
            if (m_currentfallSpeed > 10)
            {
                AudioManager.Instance.PlayAudio("SFX_JumpUp");
                ft_player.PlayOtherMaterialSound(FootstepPlayer.MATERIAL_SOUND_TYPE.LANDING);
            }
            if (m_currentfallSpeed > 100.5f && m_PlayerVelocity.magnitude > 10)
            {
                ft_player.PlayOtherMaterialSound(FootstepPlayer.MATERIAL_SOUND_TYPE.SLIP);
                ft_player.PlayOtherMaterialSound(FootstepPlayer.MATERIAL_SOUND_TYPE.LANDING);
            }
            if (m_PlayerVelocity.magnitude < m_currentfallSpeed && m_currentfallSpeed > threshold)
            {
                if (Physics.SphereCast(castOrigin, m_Character.radius - .01f, Vector3.down,
                out var hit, .11f, LayerMask.GetMask("NoFalldamage"), QueryTriggerInteraction.Ignore))
                {
                    AudioManager.Instance.PlayAudio("SFX_SoftImpact", transform.position);
                    return;
                }
                //AudioManager.Instance.PlayAudio("SFX_Damage", m_Head.transform.position);
                MouthAudioSource.Play();
                health = Mathf.Clamp(health - (2 + m_currentfallSpeed * multiplier), -10.0f, maxhp);

            }

        }
        public DamageTypes GetLastDmgType() { return lastDamageType; }
        public DamageTypes SetLastDmgType(DamageTypes value) { return lastDamageType = value; }

        public void DealDamage(float dmg, DamageTypes damageType)
        {
            SetLastDmgType(damageType);
            health = Mathf.Clamp(health - dmg, -10.0f, maxhp);
            m_MouseLook.SetTilt(0.4f);
            if (health <= 0) { Die(); }
        }

        private void Die()
        {
            TransitionManager.Instance().Transition(SceneManager.GetActiveScene().name, deathTransition, 0);
        }

        private void Regen()
        {
            health = Mathf.Clamp(health + 0.05f, 0f, maxhp); //Hp regen
        }
        bool smoothBoolean(bool rawValue, int age)
        {
            float lastValue;
            float value;
            value = System.Convert.ToSingle(rawValue);

            lastValue = value;
            value = Mathf.Lerp(lastValue, value, 1 / Mathf.Pow(2, age));
            return (value > 0.5);
        }

        private void Update()
        {
            //DEBUG SAVE POS FEATURE
            if (Input.GetKey(KeyCode.Alpha3))
            {
                saveVelo = m_PlayerVelocity;
                savePos = transform.position;
            }
            if (Input.GetKey(KeyCode.Alpha4))
            {
                transform.position = savePos;
                m_PlayerVelocity = saveVelo;
            }



            RunGroundCheck();
            if (Input.GetKeyDown(KeyCode.O))
            {
                Die();
            }

            Climb();
            wallrunningStamina += 1;
            // Control Tip Text for parkour roll
            if (MainGameObject.Instance.controlTips != null && m_currentfallSpeed / (40 - 30) > 0.9) { MainGameObject.Instance.controlTips.text = "shift"; }
            else
            {
                if (MainGameObject.Instance.controlTips != null && MainGameObject.Instance.controlTips.text == "shift") { MainGameObject.Instance.controlTips.text = ""; }
            }
            fallFx.weight = Mathf.Clamp((m_currentfallSpeed - 3) / 20, 0f, 1f);


            float targetFOV;
            float targetSpeedSound;
            Vector3 playerHVelo = new Vector3(m_PlayerVelocity.x, 0.0f, m_PlayerVelocity.z);
            targetFOV = defaultFOV + defaultFOV * Mathf.Clamp((playerHVelo.magnitude - 3) / 20, 0f, 0.7f);
            m_Camera.fieldOfView = Mathf.Clamp(Mathf.Lerp(m_Camera.fieldOfView, targetFOV, Time.deltaTime * 2f), 30f, 120f);

            if (!indoorWalking)
            {
                targetSpeedSound = Mathf.Clamp((playerHVelo.magnitude - 0.1f) / 20, 0.2f, 1f); //hehee clever lerp juggling :3
                m_SpeedWindSound.pitch = Mathf.Lerp(m_SpeedWindSound.pitch, 0.4f + (targetSpeedSound * 0.6f), Time.deltaTime * 8f);
                targetSpeedSound = Mathf.Clamp((playerHVelo.magnitude - 0.1f) / 20, 0.0f, 1f);
                m_SpeedWindSound.volume = Mathf.Clamp(Mathf.Lerp(m_SpeedWindSound.volume, targetSpeedSound * 0.5f, 0.1f), 0f, 1f);
                Sharpen cameraSharpenScript = m_Camera.GetComponent<Sharpen>();

                speedEffect.weight = Mathf.Lerp(speedEffect.weight, targetSpeedSound, Time.deltaTime * 2f);

                if (cameraSharpenScript)
                {
                    cameraSharpenScript.sharpness = 2 + targetSpeedSound * 2;
                }
            }


            m_MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            if (MainGameObject.Instance.wiimote != null)
            {
                if (MainGameObject.Instance.wiimote.Button.d_right) m_MoveInput.x = 1f;
                if (MainGameObject.Instance.wiimote.Button.d_left) m_MoveInput.x = -1f;
                if (MainGameObject.Instance.wiimote.Button.d_up) m_MoveInput.y = 1f;
                if (MainGameObject.Instance.wiimote.Button.d_down) m_MoveInput.y = -1;
            }

            if (!isCurrentlyGrounded && !isOnSlope && MainGameObject.Instance.s_alwaysHardStrafeInAir && !isWallRunning)
            {
                m_MouseLook.SetRotastrafe(true);
                if (m_MoveInput.x != 0 && m_MoveInput.z != 0)
                {
                    m_MoveInput.z = 0;
                }
            }
            else
            {
                m_MouseLook.SetRotastrafe(false);
            }

            m_MouseLook.UpdateCursorLock();

            QueueJump();
            if (isCurrentlyGrounded)
            {
                fallFx.weight = 0.0f;
            }
            if (isCurrentlyGrounded && !m_WasGrounded)
            {
                wallrunningStamina = m_maxWallrunningStamina;
                hasKickJumped = false;
                wasWallrunning = false;
                //headAnimator.ResetTrigger("Jump");
                m_LandingTimer = 0.0f;
            }
            else
            {
                headAnimator.SetBool("Running", false); //may do weird shit because this is update
                headAnimator.ResetTrigger("Running");
            }

            
            Regen();
            damageFx.weight = health / maxhp * -1 + 1;
            AudioLowPassFilter damageAudioFilter;
            damageAudioFilter = m_Camera.GetComponent<AudioLowPassFilter>();
            if (damageAudioFilter)
            {
                //otherwise frequency will be capped to 22000 khz always which is shit lol
                if (health == maxhp) { damageAudioFilter.enabled = false; }
                else { damageAudioFilter.enabled = true; }

                damageAudioFilter.cutoffFrequency = Mathf.Clamp(((health - 20) / (maxhp - 20)) * 22000, 100f, 22000f);

            }

            if (health <= 1) { Die(); } //Death

            if (Input.GetKeyDown(KeyCode.LeftShift) && isCurrentlyGrounded == false)
            {
                intentsRoll = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                intentsRoll = false;
            }

            if (isCurrentlyGrounded)
            {

                UpdateSlopeSliding();
                GroundMove();
                
                if (MainGameObject.Instance.controlTips != null && MainGameObject.Instance.controlTips.text == "shift") { MainGameObject.Instance.controlTips.text = ""; }
                m_LandingTimer += Time.deltaTime; // Increment the landing timer when the player is grounded

                if (intentsRoll && m_inRollZone == true && hasRolled == false)
                {
                    StartCoroutine(RollCoroutine());
                    hasRolled = true;
                }
                if (m_LandingTimer <= 0.2f && m_LandingTimer >= 0.01f)
                {
                    // Check for roll input within the first 0.2 seconds of landing
                    if (damageApplied == false && hasRolled == false)
                    {
                        if (intentsRoll)
                        {
                            if (m_currentfallSpeed >= 10.0)
                            {
                                if (!m_IsRolling && isCurrentlyGrounded && !isOnSlope)
                                {
                                    hasRolled = true;
                                    StartCoroutine(RollCoroutine());
                                    ApplyFallDamage(0.5f, 40f);
                                    MainGameObject.Instance.score += 20;
                                    AudioManager.Instance.PlayAudio("SFX_Coin");
                                    intentsRoll = false;
                                }
                            }
                        }
                    }
                }
                if (intentsRoll == false)
                {
                    if (damageApplied == false && hasRolled == false)
                    {
                        ApplyFallDamage(2f, 10f);
                        damageApplied = true;
                    }
                }
            }

            else
            {
                m_currentfallSpeed = m_PlayerVelocity.y * -1;


                
                AirMove();
                UpdateSlopeSliding();
                damageApplied = false;
                hasRolled = false;
            }

            // Rotate the character and camera.
            m_MouseLook.LookRotation(m_Tran, m_CamTran);

            Debug.DrawLine(transform.position, m_PlayerVelocity * 100f, new Color(1.0f, 0.0f, 1.0f));
            // Move the character.
            m_Character.Move(m_PlayerVelocity * Time.deltaTime);


            CheckAndUpdateCrouch();
            if (Input.GetKeyUp(KeyCode.Numlock))
            {
                Vector3 pos = transform.position;
                pos.y += m_OriginalHeight - m_CrouchHeight + 0.001f;
                m_Character.height = m_OriginalHeight;
                transform.position = pos;
                m_IsCrouching = false;
            }

        }

        private void Climb()
        {
            var p = transform.position;
            var d = transform.forward;
            var hit1 = Physics.Raycast(p, d, out var h1, 1f * (m_PlayerVelocity.magnitude / 7), 3);
            var hit2 = Physics.Raycast(p + Vector3.up * 1/*meters*/, d, out var h2, 1f, 3);


            if (hit1 && !hit2 && m_JumpQueued && m_parkourEnabled)
            {
                Debug.Log(Mathf.Abs(Vector3.Dot(h1.normal, m_Head.transform.forward)));
                if (Mathf.Abs(Vector3.Dot(h1.normal, m_Head.transform.forward)) > 0.9f && isCurrentlyGrounded == false)
                {

                    //Debug.Log($"Hit {h1.collider.gameObject.name} at point: {h1.point}, normal: {h1.normal}");
                    m_PlayerVelocity.y = 8;
                    wallrunningStamina = m_maxWallrunningStamina;

                    if (headAnimator != null)
                    {
                        headAnimator.SetTrigger("Vault");
                        MainGameObject.Instance.score += 5;
                        AudioManager.Instance.PlayAudio("SFX_Climb", transform.position);

                    }
                }
            }
            else
            {
                //Debug.Log("Conditions for second raycast hit not met.");
            }
        }
        private bool TowardsWallCheck(float distance)
        {
            var p = transform.position;
            var d = transform.forward;
            RaycastHit hit;
            Physics.Raycast(p, d, out hit, distance);
            var wallAngle = Vector3.Angle(Vector3.up, hit.normal);

            if (Vector3.Dot(hit.normal, m_PlayerVelocity) > -10f &&
                Vector3.Dot(hit.normal, m_PlayerVelocity) < -1f &&
                wallAngle > m_Character.slopeLimit)
            {
                return true;
            }

            else { return false; }
        }

        private void Wallrun(ControllerColliderHit hit)
        {

            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f
                && Input.GetButton("Jump") && wallrunningStamina > 0) //getbutton jump is better than m_jumpqueued in this context!
            {
                wallrunningStamina -= 2;
                float angleOfAttack;
                angleOfAttack = Mathf.Abs(Vector3.Dot(hit.normal, m_Head.transform.forward));
                if (Mathf.Abs(Vector3.Dot(hit.normal, m_Head.transform.forward)) < 0.8f && !isCurrentlyGrounded)
                {

                    isWallRunning = true;
                    //Wallrun Start
                    Vector3 wallrunVector = new Vector3(0, 10, 0);

                    m_PlayerVelocity.y *= 0.05f;
                    m_PlayerVelocity += (-hit.normal * 0.1f + m_Camera.transform.forward * 10f * wallrunningStamina / m_maxWallrunningStamina * Time.deltaTime * angleOfAttack);

                    m_PlayerVelocity.y = Vector3.Dot(m_Camera.transform.forward, Vector3.up) * 10f;


                    isWallRunning = true;


                    m_wallrunningSoundLoop.volume = 0.3f;
                    wasWallrunning = true;

                }
            }



            else
            {


                isWallRunning = false;
                if (wasWallrunning)
                {
                    m_PlayerVelocity *= 0.1f;
                    m_PlayerVelocity += (hit.normal * m_StrafeSettings.MaxSpeed * 7f);
                    m_PlayerVelocity += m_Camera.transform.forward * 10f;

                    wallrunningStamina = 0;
                    m_PlayerVelocity.y += 5f; //little jump after wallrunning
                    AudioManager.Instance.PlayAudio("SFX_WallrunEnd", transform.position);
                    wasWallrunning = false;

                }
                m_wallrunningSoundLoop.volume = 0.0f;
            }

        }

        private void Kickjump(ControllerColliderHit hit)
        {

            if (Vector3.Dot(hit.normal, transform.forward) > 0.9f &&
                Vector3.Dot(hit.normal, transform.forward) < 1.0f &&
                m_JumpQueued == true && m_PlayerVelocity.y < 0 &&
                hasKickJumped == false &&
                Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f)

            {
                AudioManager.Instance.PlayAudio("SFX_Epic");
                wallrunningStamina += 10;
                float angleOfAttack;
                angleOfAttack = Mathf.Abs(Vector3.Dot(hit.normal, m_Head.transform.right));

                hasKickJumped = true;

                m_PlayerVelocity += m_Head.transform.forward * 10f;

                m_PlayerVelocity.y *= 0.1f;
                m_PlayerVelocity.y += 10.5f;

                m_PlayerVelocity += (hit.normal * m_StrafeSettings.MaxSpeed * 0.6f);
            }

        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (m_wallrunningAllowed)
            {
                Wallrun(hit);
            }
            Kickjump(hit);
            /*
            if (wallrunningStamina > 0) { Wallrun(hit); }
            else { isWallRunning = false;  }
        */
        }

        // Queues the next jump.
        private void QueueJump()
        {
            if (m_MouseLook.GetCursorLock() == true)
            { //otherways playuer is able to jump in yarn interactions for example
                if (m_AutoBunnyHop && !intentsRoll)
                {
                    //print(intentsRoll);
                    m_JumpQueued = Input.GetButton("Jump");
                    return;
                }



                if (Input.GetButtonDown("Jump") && !m_JumpQueued && !intentsRoll)
                {
                    //headAnimator.ResetTrigger("Jump");
                    //headAnimator.SetTrigger("Jump");
                    m_JumpQueued = true;


                }

                if (Input.GetButtonUp("Jump") || intentsRoll)
                {
                    m_JumpQueued = false;
                }

            }
        }



        
        public static int ClipVelocity(Vector3 input, Vector3 normal, ref Vector3 output, float overbounce)
        //https://github.com/Maugun/SurfUnity/blob/master/Assets/Scripts/SurfPhysics.cs
        {
            var angle = normal[1];
            var blocked = 0x00;
            if (angle > 0)
                blocked |= 0x01;
            if (angle == 0)
                blocked |= 0x02;
            var backoff = Vector3.Dot(input, normal) * overbounce;

            for (int i = 0; i < 3; i++)
            {
                var change = normal[i] * backoff;
                output[i] = input[i] - change;
            }
            float adjust = Vector3.Dot(output, normal);
            if (adjust < 0.0f)
            {
                output -= (normal * adjust);
            }
            return blocked;
        }
        void RunGroundCheck()
        {
            if (!m_Character.isGrounded) { 
                isCurrentlyGrounded = false;
                isOnSlope = false;
                return;
            } //omfg spent like 6 damn hours to realize i should do this

            
            var castVerticalOffset = m_Character.height / 2 + m_Character.radius;
            
            var castLength = castVerticalOffset * 5f;

            /* //Same as raycast - it works but is slightly weird still very usable though
            var castOrigin = transform.position;
            Debug.DrawRay(castOrigin, Vector3.down*castLength, Color.red, 30f);
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, castLength, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
            */
            var p = m_Character.height / 2 - m_Character.radius;
            var castOrigin = transform.position - new Vector3(0, p, 0);


            if (Physics.SphereCast(castOrigin, m_Character.radius - 0.1f, Vector3.down,
                out var hit, 5.5f, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
            {
                slopeNormal = hit.normal;
                Debug.DrawRay(castOrigin, Vector3.down * castLength, Color.green, 30f);
                Debug.DrawRay(hit.point, hit.normal * 0.2f, Color.cyan, 30f);
                var collider = hit.collider;
                var angle = Vector3.Angle(Vector3.up, hit.normal);
                print("floor/slope angle:  " + angle);
                if (angle > m_Character.slopeLimit)
                {

                    m_WasGrounded = isCurrentlyGrounded;
                    isOnSlope = true;
                    isCurrentlyGrounded = false;
                }
                else
                {

                    m_WasGrounded = isCurrentlyGrounded;
                    isOnSlope = false;
                    isCurrentlyGrounded = true;
                }
            }
            else
            {

                m_WasGrounded = isCurrentlyGrounded;
                isCurrentlyGrounded = false;
            }
        }

        void UpdateSlopeSliding()
        {
            if (isOnSlope == true)
            {
                //https://github.com/ValveSoftware/halflife/blob/c7240b965743a53a29491dd49320c88eecf6257b/pm_shared/pm_shared.c#L1575
                //line 732
                ClipVelocity(m_PlayerVelocity, slopeNormal, ref m_PlayerVelocity, 1f);

                m_SlideSound.volume += 0.1f;
                m_SlideSound.volume = Mathf.Clamp(m_SlideSound.volume, 0.0f, 0.3f);

            }
            else
            {
                m_SlideSound.volume -= 0.1f;
                m_SlideSound.volume = Mathf.Clamp(m_SlideSound.volume, 0.0f, 0.3f);

            }
        }
        

        void TryBackflip()
        {
            //if (Vector3.Dot(m_PlayerVelocity, transform.forward) < -0.5f && m_MoveInput.z >= 0) //flip  && Input.GetKey(KeyCode.Q)
            if (Input.GetAxis("Mouse Y") > 3f)
            {
                //print("Dot product:  " + Vector3.Dot(m_PlayerVelocity, transform.forward));
                headAnimator.ResetTrigger("Backflip");
                headAnimator.SetTrigger("Backflip");
                m_PlayerVelocity.y += 3f;

                //m_MouseLook.SetSmooth(true, 3f);
            }
        }
        // Handle air movement.

        public static Vector3 AirAccelerate(Vector3 velocity, Vector3 wishdir, float wishspeed, float accel, float airCap, float deltaTime)
        {
            var wishspd = wishspeed;

            // Cap speed
            wishspd = Mathf.Min(wishspd, airCap);

            // Determine veer amount
            var currentspeed = Vector3.Dot(velocity, wishdir);

            // See how much to add
            var addspeed = wishspd - currentspeed;

            // If not adding any, done.
            if (addspeed <= 0)
            {
                return Vector3.zero;
            }

            // Determine acceleration speed after acceleration
            var accelspeed = accel * wishspeed * deltaTime;

            // Cap it
            accelspeed = Mathf.Min(accelspeed, addspeed);

            var result = Vector3.zero;

            // Adjust pmove vel.
            for (int i = 0; i < 3; i++)
            {
                result[i] += accelspeed * wishdir[i];
            }

            return result;
        }


        private void AirMove()
        {

            float accel;

            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);
            //Tilt Camera thing (roll first)
            float tilt = -Vector3.Dot(transform.right, Vector3.Lerp(m_PlayerVelocity, wishdir, 0.5f)) * m_TiltAmount;
            float tiltLerped = tilt;
            if (isWallRunning)
            {
                tiltLerped = Mathf.Lerp(-tiltLerped * 3f, -tilt * 3f, 0.1f);
            }
            else
            {
                tiltLerped = Mathf.Lerp(tiltLerped, tilt, 0.1f);
            }
            m_MouseLook.SetTilt(tiltLerped);
            //then pitch (doesnt work and no motivation to fix lol)
            /*
            float tiltLerpedP = Mathf.Lerp(m_Camera.transform.eulerAngles.x, 
                Mathf.Clamp(Mathf.Abs(m_PlayerVelocity.y), 0f, 0.1f), 0.5f);
            m_MouseLook.AddFallTilt(tiltLerpedP);
            */

            float wishspeed = wishdir.magnitude;
            wishspeed *= m_AirSettings.MaxSpeed;

            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            // CPM Air control.
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0)
            {
                accel = m_AirSettings.Deceleration;
            }
            else
            {
                accel = m_AirSettings.Acceleration;
            }


            // If the player is ONLY strafing left or right
            if (m_MoveInput.z == 0 && m_MoveInput.x != 0)
            {
                if (wishspeed > m_StrafeSettings.MaxSpeed)
                {
                    wishspeed = m_StrafeSettings.MaxSpeed;
                }

                accel = m_StrafeSettings.Acceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (m_AirControl > 0)
            {
                AirControl(wishdir, wishspeed2);
            }

            // Apply gravity
            m_PlayerVelocity.y -= m_Gravity * Time.deltaTime;
            m_PlayerVelocity.y = Mathf.Clamp(m_PlayerVelocity.y, -m_MaxFallSpeed, m_MaxFallSpeed*10f);
        }

        // Air control occurs when the player is in the air, it allows players to move side 
        // to side much faster rather than being 'sluggish' when it comes to cornering.
        private void AirControl(Vector3 targetDir, float targetSpeed)
        {
            // Only control air movement when moving forward or backward.
            if (Mathf.Abs(m_MoveInput.z) < 0.001 || Mathf.Abs(targetSpeed) < 0.001)
            {
                return;
            }

            
            float zSpeed = m_PlayerVelocity.y;
            m_PlayerVelocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            float speed = m_PlayerVelocity.magnitude;
            m_PlayerVelocity.Normalize();

            float dot = Vector3.Dot(m_PlayerVelocity, targetDir);
            float k = 32;
            k *= m_AirControl * dot * dot * Time.deltaTime;

            // Change direction while slowing down.
            if (dot > 0)
            {
                m_PlayerVelocity.x *= speed + targetDir.x * k;
                m_PlayerVelocity.y *= speed + targetDir.y * k;
                m_PlayerVelocity.z *= speed + targetDir.z * k;

                m_PlayerVelocity.Normalize();
                m_MoveDirectionNorm = m_PlayerVelocity;
            }

            m_PlayerVelocity.x *= speed;
            m_PlayerVelocity.y = zSpeed; // Note this line
            m_PlayerVelocity.z *= speed;
        }
        private void GroundMove()
        {
            if (isOnSlope) { return; } //only use air movement on slopes
            float tilt = -Vector3.Dot(transform.right, m_PlayerVelocity);
            float tiltLerped = tilt;
            tilt = Mathf.Lerp(tiltLerped, tilt, 0.1f);
            if (indoorWalking) { tiltLerped *= 0.25f; }
            m_MouseLook.SetTilt(tiltLerped);

            if (m_MouseLook.GetCursorLock() == false) { ApplyFriction(0.5f); headAnimator.SetBool("Running", false); return; }
            // Do not apply friction if the player is queueing up the next jump
            if (!m_JumpQueued)
            {
                ApplyFriction(1.0f);
                if (m_IsCrouching == true)
                {
                    ApplyFriction(1.5f);
                }
            }
            else
            {
                TryBackflip();
                ApplyFriction(0);
            }


            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);
            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            var wishspeed = wishdir.magnitude;
            if (indoorWalking == false)
            {
                wishspeed *= m_GroundSettings.MaxSpeed;
            }
            if (indoorWalking == true)
            {
                wishspeed *= m_GroundSettings.MaxSpeed * 0.8f;
            }
            if (Mathf.Abs(m_PlayerVelocity.x) + Mathf.Abs(m_PlayerVelocity.z) != 0 && isCurrentlyGrounded)
            {
                if (intentsRoll == false)
                {
                    headAnimator.SetBool("Running", true);
                }
            }
            else
            {
                headAnimator.SetBool("Running", false);
            }

            if (!TowardsWallCheck(0.1f))
            {
                Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);
            }
            else
            {

            }
            // Reset the gravity velocity
            if (!isOnSlope) {
                m_PlayerVelocity.y = -m_Gravity * Time.deltaTime;
                headAnimator.SetFloat("GroundMoveSpeed", m_PlayerVelocity.magnitude * 0.25f);
            }

            if (m_JumpQueued)
            {
                
                m_PlayerVelocity.y = m_JumpForce;
                AudioManager.Instance.PlayAudio("SFX_JumpUp", transform.position);
                //Accelerate(LookRotation, 50, 20.0f);
                m_JumpQueued = false;
            }

        }

        private void ApplyFriction(float t)
        {
            // Equivalent to VectorCopy();
            Vector3 vec = m_PlayerVelocity;
            vec.y = 0;
            float speed = vec.magnitude;
            float drop = 0;

            // Only apply friction when grounded.
            if (isCurrentlyGrounded && !isOnSlope)
            {
                float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
                drop = control * m_Friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            m_PlayerFriction = newSpeed;
            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (speed > 0)
            {
                newSpeed /= speed;
                //m_PlayerVelocity *= newSpeed; //corrected to be more like source engine than q3
            }


            m_PlayerVelocity.x *= newSpeed;
            m_PlayerVelocity.z *= newSpeed;

        }


        private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
        {
            float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
            float addspeed = targetSpeed - currentspeed;
            if (addspeed <= 0)
            {
                return;
            }

            float accelspeed = accel * Time.deltaTime * targetSpeed;
            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }

            //m_HeadAnchor.transform.Rotate(0, 0, Vector3.Dot(transform.right, targetDir) * accel);
            m_PlayerVelocity.x += accelspeed * targetDir.x;
            m_PlayerVelocity.z += accelspeed * targetDir.z;

        }

        public void Teleport(Vector3 pos)
        {
            Vector3 posi = pos;
            transform.position = posi;
        }
    }
}