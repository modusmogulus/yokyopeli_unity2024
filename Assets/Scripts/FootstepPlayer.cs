using UnityEngine;
/*prompt: User
Can you write me a Unity script called Footstep_Player which:
1: Has an array of physics materials (PhysMat) and another array of strings (soundToPlay)
2: Has a function called PlayFootstepSound which will cast a ray 1 meter downwards and call Audiomanager.Instance.Play(PhysSound)
3.Has a string called PhysSound which will be set to the string in soundToPlay of same index as the PhysMat which is returned by the raycast
*/
using static DamageTypes;
public class FootstepPlayer : MonoBehaviour
{
    public PhysicMaterial[] physMaterials;
    public string[] soundToPlay;
    public string defaultSound;
    private string physSound;
    public PhysicMaterial snowMaterial;
    Vector3 pos;

    public string[] soundToPlayOnLanding;
    public string defaultSoundOnLanding;

    private string physSoundOnLanding;
    public enum MATERIAL_SOUND_TYPE
    {
        FOOTSTEP = 0,
        LANDING = 1,
        ROLL = 2,
        SLIP = 3
    }
    public void PlayFootstepSound()
    {
        print("Castatty");
        pos = transform.position + new Vector3(0f, 0.5f, 0f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f))
        {
            print("Osui");

            Collider collider = hit.collider;
            if (collider != null)
            {
                PhysicMaterial hitPhysMaterial = collider.sharedMaterial;
                if (hitPhysMaterial == null) {
                    AudioManager.Instance.PlayAudio(defaultSound, pos);
                }


                int index = System.Array.IndexOf(physMaterials, hitPhysMaterial);

                if (index != -1 && index < soundToPlay.Length)
                {

                    print(physSound);
                    physSound = soundToPlay[index];

                    AudioManager.Instance.PlayAudio(physSound, pos);
                }

                // ----------------------------------------------------- Snow in boots mechanics stuff -----------------------------------------------------
                if (hitPhysMaterial == snowMaterial)
                {
                    MainGameObject.Instance.playerController.SetSnowInBoots(
                        Mathf.Clamp(MainGameObject.Instance.playerController.GetSnowInBoots() + 1, 0, 9)
                    );
                    if (MainGameObject.Instance.playerController.GetSnowInBoots() >= 8 && MainGameObject.Instance.playerController.health > 41) {
                        MainGameObject.Instance.playerController.DealDamage(3, DamageTypes.FROZEN);
                    }
                    MainGameObject.Instance.mainCanvas.GetComponent<MainUIScript>().snowBootIndicator.sprite =
                    MainGameObject.Instance.mainCanvas.GetComponent<MainUIScript>().snowBootIndicatorSprites[MainGameObject.Instance.playerController.GetSnowInBoots()];
                }
                else
                {
                    MainGameObject.Instance.playerController.SetSnowInBoots(
                        MainGameObject.Instance.playerController.GetSnowInBoots() - 1
                    );
                    MainGameObject.Instance.mainCanvas.GetComponent<MainUIScript>().snowBootIndicator.sprite =
                    MainGameObject.Instance.mainCanvas.GetComponent<MainUIScript>().snowBootIndicatorSprites[MainGameObject.Instance.playerController.GetSnowInBoots()];
                }
                // ----------------------------------------------------- Snow in boots mechanics stuff -----------------------------------------------------


            }
        }
    }

    public void PlayOtherMaterialSound(MATERIAL_SOUND_TYPE sndType)
    {
        print("Castatty");

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f))
        {
            print("Osui");

            
            Collider collider = hit.collider;
            if (collider != null)
            {
                PhysicMaterial hitPhysMaterial = collider.sharedMaterial;
                if (hitPhysMaterial == null)
                {
                    switch (sndType) {

                        case MATERIAL_SOUND_TYPE.FOOTSTEP:
                            AudioManager.Instance.PlayAudio(defaultSound, pos);
                            break;

                        case MATERIAL_SOUND_TYPE.LANDING:
                            AudioManager.Instance.PlayAudio(defaultSoundOnLanding, pos);
                            break;
                    }; 
                    
                }

                int index = System.Array.IndexOf(physMaterials, hitPhysMaterial);

                switch (sndType) {
                    case MATERIAL_SOUND_TYPE.FOOTSTEP:
                        if (index != -1 && index < soundToPlay.Length)
                        {

                            physSound = soundToPlay[index];

                            AudioManager.Instance.PlayAudio(physSound, pos);

                        }
                        break;
                    case MATERIAL_SOUND_TYPE.LANDING:
                        if (index != -1 && index < soundToPlay.Length)
                        {

                            print(physSound);
                            physSoundOnLanding = soundToPlayOnLanding[index];

                            AudioManager.Instance.PlayAudio(physSoundOnLanding, pos);

                        }
                        break;
                };
            }
        }
    }
}
