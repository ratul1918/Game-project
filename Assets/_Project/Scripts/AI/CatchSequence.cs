using System.Collections;
using UnityEngine;
namespace TheLightHouse
{
    public sealed class CatchSequence : MonoBehaviour
    {
        [SerializeField] PlayerHealthState player;
        [SerializeField] Transform stalker;
        [SerializeField] float duration=2;
        [SerializeField] float turnSpeed=180;
        [SerializeField] DialogueLine line=new("Daniel","No—!");
        float fade;bool active;
        void OnEnable()=>GameEvents.CatchRequested+=Begin;
        void OnDisable(){GameEvents.CatchRequested-=Begin;StopAllCoroutines();}
        void Begin(){if(active||player.IsHidden||player.IsCaught)return;StartCoroutine(Catch());}
        IEnumerator Catch()
        {
            active=true;player.IsCaught=true;
            var movement=player.GetComponent<FirstPersonController>();movement.SetMovementLocked(true);movement.SetPeekLimits(0,0);movement.SetLookLocked(true);
            GameEvents.RequestDialogue(line);float elapsed=0;
            while(elapsed<duration){elapsed+=Time.deltaTime;fade=Mathf.Clamp01(elapsed/duration);
                Vector3 direction=stalker.position-player.transform.position;direction.y=0;
                if(direction.sqrMagnitude>0)player.transform.rotation=Quaternion.RotateTowards(player.transform.rotation,Quaternion.LookRotation(direction),turnSpeed*Time.deltaTime);
                yield return null;
            }
            player.Respawn();fade=0;active=false;
        }
        void OnGUI(){if(fade<=0)return;var old=GUI.color;GUI.color=new Color(0,0,0,fade);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=old;}
    }
}
