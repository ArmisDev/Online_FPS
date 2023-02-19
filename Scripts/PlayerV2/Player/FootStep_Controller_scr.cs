using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Armis.TextureTypes
{
    [RequireComponent(typeof(CharacterController),typeof(AudioSource))]
    public class FootStep_Controller_scr : MonoBehaviour
    {
        [SerializeField] private LayerMask FloorLayer;
        [SerializeField] private TextureSound[] textureSounds;
        [SerializeField] private bool blendTerrainSounds;
        [SerializeField] private Controller_scr controllerScript;
        [SerializeField] private bool isGrounded;

        private CharacterController controller;
        private AudioSource audioSource;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            StartCoroutine(CheckForGround());
            controllerScript = GetComponentInParent<Controller_scr>();
            isGrounded = false;
            
        }

        private IEnumerator CheckForGround()
        {
            while (true)
            {
                if (controller.velocity.magnitude >= 0.5f && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.3f, FloorLayer))
                {
                    Debug.Log("footstep test");
                    Debug.DrawRay(transform.position - new Vector3(0, 0.5f * controller.height + 0.5f * controller.radius, 0), Vector3.down, Color.green);
                    isGrounded = true;
                    if (hit.collider.TryGetComponent<Terrain>(out Terrain terrain))
                    {
                        yield return StartCoroutine(PlayerFootstepSoundFromTerrain(terrain, hit.point));
                    }

                    else if (hit.collider.TryGetComponent<Renderer>(out Renderer renderer))
                    {
                        yield return StartCoroutine(PlayerFootstepSoundFromRenderer(renderer));
                    }
                }

                yield return null;
            }
        }

        private IEnumerator PlayerFootstepSoundFromTerrain(Terrain terrain, Vector3 HitPoint)
        {
            Vector3 TerrainPosition = HitPoint - terrain.transform.position;
            Vector3 SplatMapPosition = new Vector3(TerrainPosition.x / terrain.terrainData.size.x, 0, TerrainPosition.z / terrain.terrainData.size.z);

            int x = Mathf.FloorToInt(SplatMapPosition.x * terrain.terrainData.alphamapWidth);
            int z = Mathf.FloorToInt(SplatMapPosition.z * terrain.terrainData.alphamapHeight);

            float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

            if(!blendTerrainSounds)
            {
                int primaryIndex = 0;
                for(int i = 0; i < alphaMap.Length; i++)
                {
                    if (alphaMap[0, 0, i] > alphaMap[0,0,primaryIndex])
                    {
                        primaryIndex = i;
                    }
                }

                foreach(TextureSound textureSound in textureSounds)
                {
                    if(textureSound.Albedo == terrain.terrainData.terrainLayers[primaryIndex].diffuseTexture)
                    {
                        AudioClip clip = GetClipFromTextureSounds(textureSound);
                        audioSource.PlayOneShot(clip);
                        if (controllerScript.isRunning)
                        {
                            yield return new WaitForSeconds(controllerScript.characterMagnitude * 0.04f);
                        }

                        else yield return new WaitForSeconds(controllerScript.characterMagnitude * 0.115f);

                        break;
                    }
                }
            }

            else
            {
                List<AudioClip> clips = new List<AudioClip>();
                int clipIndex = 0;
                for (int i = 0; i < alphaMap.Length; i++)
                {
                    if (alphaMap[0, 0, i] > 0)
                    {
                        foreach(TextureSound textureSound in textureSounds)
                        {
                            if(textureSound.Albedo == terrain.terrainData.terrainLayers[i].diffuseTexture)
                            {
                                AudioClip clip = GetClipFromTextureSounds(textureSound);
                                audioSource.PlayOneShot(clip, alphaMap[0, 0, i]);
                                clips.Add(clip);
                                clipIndex++;
                                break;
                            }
                        }
                    }
                }

                float longestClip = clips.Max(clip => clip.length);
                if (controllerScript.isRunning)
                {
                    yield return new WaitForSeconds(longestClip * controllerScript.characterMagnitude * 0.1f);
                }

                else yield return new WaitForSeconds(longestClip * controllerScript.characterMagnitude * 0.27f);
            }
        }

        private IEnumerator PlayerFootstepSoundFromRenderer(Renderer renderer)
        {
            foreach(TextureSound textureSound in textureSounds)
            {
                if (textureSound.Albedo == renderer.material.GetTexture("_MainTex"))
                {
                    AudioClip clip = GetClipFromTextureSounds(textureSound);
                    audioSource.PlayOneShot(clip);

                    if(controllerScript.isRunning)
                    {
                        yield return new WaitForSeconds(controllerScript.characterMagnitude * 0.04f);
                    }

                    else yield return new WaitForSeconds(controllerScript.characterMagnitude * 0.115f);
                }
            }
        }

        private AudioClip GetClipFromTextureSounds(TextureSound textureSound)
        {
            int clipIndex = Random.Range(0, textureSound.footStepClips.Length);
            return textureSound.footStepClips[clipIndex];
        }

        [System.Serializable]
        private class TextureSound
        {
            public Texture Albedo;
            public AudioClip[] footStepClips;
        }
    }
}
